using System;
using System.Collections;
using System.Collections.Generic;
using Core.Enums;
using UnityEngine;
using GamePlay.Configs;
using GamePlay.Systems;
using GamePlay.Questions;
using UI; 

namespace Managers {
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private QuestionLoader questionLoader;

        public static event Action<int> OnRoundStarted;
        public static event Action<int> OnTimerUpdated; 
        public static event Action OnAnswerTimeOutUI;   
        
        public static event Action OnNewQuestionLoaded;
        public static event Action<int> OnValidPlayerBuzzed; 
        public static event Action<int> OnPlayerLockedOut;   
        public static event Action OnBuzzerWindowReopened;
        public static event Action<int> OnVerbalEvaluationStarted;

        private RoundConfig currentRound;
        private int questionIndex;
        private bool roundActive;
        private Coroutine activeTimer;
        private int currentlyAnsweringPlayer = -1;
        
        private int currentQuestionAttempts = 0;
        private const int MaxAttemptsPerMCQ = 2; 
        private HashSet<int> blockedPlayers = new HashSet<int>(); 

        // --- NEW: VERBAL STATE MACHINE ---
        private enum VerbalState { None, WaitingForBuzz, AnsweringOriginal, Stealing, AnsweringSteal }
        private VerbalState currentVerbalState = VerbalState.None;
        private int originalBuzzerPlayer = -1; 
        private int verbalTargetPlayer = -1;

        private void OnEnable()
        {
            BuzzerSystem.OnPlayerBuzzed += HandlePlayerBuzzed;
            GameplayHUD.OnAnswerEvaluated += HandleAnswerEvaluated;
            GameManager.OnStateChanged += HandleStateChange; 
        }

        private void OnDisable()
        {
            BuzzerSystem.OnPlayerBuzzed -= HandlePlayerBuzzed;
            GameplayHUD.OnAnswerEvaluated -= HandleAnswerEvaluated;
            GameManager.OnStateChanged -= HandleStateChange; 
        }

        public void ClearQuestionMemory() { if (questionLoader != null) questionLoader.ClearMemory(); }

        public void InitializeRound(List<Category> categories, QuestionType type)
        {
            if (roundActive) return;
            
            if (PlayerManager.Instance != null) PlayerManager.Instance.ResetRoundScores();

            roundActive = true;
            questionIndex = 0;
            
            currentRound = new RoundConfig { categories = categories, questionType = type, questionCount = MatchManager.Instance.questionsPerRound };
            if (type == QuestionType.MultipleChoice) currentRound.questionTimerSeconds = QuestionRules.MultipleChoiceTime;
            else if (type == QuestionType.Verbal) currentRound.questionTimerSeconds = QuestionRules.VerbalTime;

            OnRoundStarted?.Invoke(MatchManager.Instance.CurrentRoundIndex + 1);
            questionLoader.Initialize(currentRound);
            LoadNext();
        }
        
        private void HandleStateChange(GameState state)
        {
            // If the player hits Replay or Main Menu, KILL everything.
            if (state == GameState.Menu || state == GameState.Setup || state == GameState.CategorySelection)
            {
                roundActive = false;
                currentlyAnsweringPlayer = -1;
                originalBuzzerPlayer = -1;
                verbalTargetPlayer = -1;
                currentVerbalState = VerbalState.None;
                currentQuestionAttempts = 0; // Reset attempts too

                // --- THE CRITICAL FIXES ---
                // 1. Clear the list of blocked people
                blockedPlayers.Clear(); 
                
                // 2. Tell all PlayerUI scripts to restore opacity to 100%
                OnNewQuestionLoaded?.Invoke(); 

                // KILL ALL GHOST TIMERS!
                StopAllCoroutines(); 
                CancelInvoke(); 
                
                Debug.Log("RoundManager wiped clean for a fresh start.");
            }
        }

        private void LoadNext()
        {
            currentlyAnsweringPlayer = -1;
            verbalTargetPlayer = -1;
            originalBuzzerPlayer = -1;
            currentQuestionAttempts = 0; 
            blockedPlayers.Clear(); 
            
            OnNewQuestionLoaded?.Invoke(); 

            if (questionLoader.HasMoreQuestions())
            {
                questionLoader.LoadNextQuestion();
                
                // Route Verbal State
                if (questionLoader.CurrentQuestion is VerbalQuestion)
                    currentVerbalState = VerbalState.WaitingForBuzz;
                else
                    currentVerbalState = VerbalState.None;

                StartTimer(QuestionRules.BuzzTimeLimit, OnBuzzTimeOut);
            }
            else
            {
                EndRound();
            }
        }

        private void StartTimer(float duration, Action onTimeOutCallback)
        {
            if (activeTimer != null) StopCoroutine(activeTimer);
            activeTimer = StartCoroutine(TimerRoutine(duration, onTimeOutCallback));
        }

        private IEnumerator TimerRoutine(float duration, Action onTimeOut)
        {
            float timeRemaining = duration;
            while (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(Mathf.CeilToInt(timeRemaining)); 
                yield return null; 
            }
            onTimeOut?.Invoke(); 
        }

        private void OnBuzzTimeOut() { LoadNext(); }

        private void HandlePlayerBuzzed(int playerIndex)
        {
            if (blockedPlayers.Contains(playerIndex) || currentlyAnsweringPlayer != -1) return;

            // --- THE FIX: Record the original buzzer for MCQ & Verbal ---
            if (originalBuzzerPlayer == -1) 
            {
                originalBuzzerPlayer = playerIndex;
            }

            // --- VERBAL FLOW ---
            if (currentVerbalState != VerbalState.None)
            {
                if (currentVerbalState == VerbalState.WaitingForBuzz)
                {
                    verbalTargetPlayer = playerIndex;
                    currentlyAnsweringPlayer = playerIndex; 
                    currentVerbalState = VerbalState.AnsweringOriginal;
                    
                    OnValidPlayerBuzzed?.Invoke(playerIndex); 
                    StartTimer(QuestionRules.VerbalTime, OnVerbalAnswerOriginalFinished);
                }
                else if (currentVerbalState == VerbalState.Stealing)
                {
                    // Phase 2: A player steals!
                    verbalTargetPlayer = playerIndex;
                    currentlyAnsweringPlayer = playerIndex; 
                    currentVerbalState = VerbalState.AnsweringSteal;
                    
                    OnValidPlayerBuzzed?.Invoke(playerIndex); 
                    
                    // --- THE FIX: We MUST start the Steal Answer timer! ---
                    StartTimer(QuestionRules.VerbalTime, OnVerbalStealAnswerFinished);
                }
                return;
            }

            // --- MCQ FLOW ---
            currentlyAnsweringPlayer = playerIndex;
            OnValidPlayerBuzzed?.Invoke(playerIndex); 
            StartTimer(currentRound.questionTimerSeconds, OnAnswerTimeOut);
        }
        // --- VERBAL TIMEOUTS ---
        private void OnVerbalAnswerOriginalFinished()
        {
            // 10s is over. Start Steal Mode!
            currentlyAnsweringPlayer = -1; // Unblock the system so someone can steal
            currentVerbalState = VerbalState.Stealing;

            // Permanently lock the original player
            blockedPlayers.Add(verbalTargetPlayer);
            OnPlayerLockedOut?.Invoke(verbalTargetPlayer);

            // Light up the other players so they know it's Steal Time!
            OnBuzzerWindowReopened?.Invoke();

            // Give them 5 seconds to steal
            StartTimer(QuestionRules.BuzzTimeLimit, OnVerbalStealWindowFinished);
        }

        private void OnVerbalStealWindowFinished()
        {
            // 5s passed and no one stole. Open panel to evaluate original player.
            verbalTargetPlayer =  originalBuzzerPlayer; 
            OnVerbalEvaluationStarted?.Invoke(verbalTargetPlayer);
        }

        private void OnVerbalStealAnswerFinished()
        {
            // The stealer finished talking. Open panel to evaluate them!
            currentlyAnsweringPlayer = -1;
            OnVerbalEvaluationStarted?.Invoke(verbalTargetPlayer);
        }

        // --- MCQ TIMEOUT ---
        private void OnAnswerTimeOut()
        {
            OnAnswerTimeOutUI?.Invoke(); 
            ProcessFailedAttempt(); 
        }

        private void HandleAnswerEvaluated(int playerIndex, AnswerResult result)
        {
            if (activeTimer != null) StopCoroutine(activeTimer); 

            bool isVerbal = questionLoader.CurrentQuestion is VerbalQuestion;
            
            // --- THE FIX: It is a steal if the player answering is NOT the original buzzer! ---
            bool isSteal = (playerIndex != originalBuzzerPlayer);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.CalculateAndApplyScore(playerIndex, result, isVerbal, isSteal);
            }

            if (result == AnswerResult.Correct || result == AnswerResult.Almost)
            {
                OnQuestionCompleted(); 
            }
            else
            {
                if (isVerbal) OnQuestionCompleted(); 
                else ProcessFailedAttempt();         
            }
        }

      
        
        private void ProcessFailedAttempt()
        {
            currentQuestionAttempts++;      
            blockedPlayers.Add(currentlyAnsweringPlayer); 
            OnPlayerLockedOut?.Invoke(currentlyAnsweringPlayer); 
            currentlyAnsweringPlayer = -1;  

            bool isTrueFalse = false;
            if (questionLoader.CurrentQuestion != null)
                isTrueFalse = (questionLoader.CurrentQuestion is TrueOrFalseQuestion) || (questionLoader.CurrentQuestion.GetQuestionType() == QuestionType.TrueOrFalse);

            if (isTrueFalse || currentQuestionAttempts >= MaxAttemptsPerMCQ)
            {
                OnQuestionCompleted();
            }
            else
            {
                OnBuzzerWindowReopened?.Invoke(); // Light the MCQ players back up!
                StartTimer(QuestionRules.BuzzTimeLimit, OnBuzzTimeOut); 
            }
        }

        public void OnQuestionCompleted()
        {
            questionIndex++;
            LoadNext();
        }

        private void EndRound()
        {
            roundActive = false;
            MatchManager.Instance.OnRoundFinished();
        }
    }
}