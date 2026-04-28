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

<<<<<<< HEAD
        // --- NEW: UI Events ---
=======
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
        public static event Action<int> OnRoundStarted;
        public static event Action<int> OnTimerUpdated; 
        public static event Action OnAnswerTimeOutUI;   
        
<<<<<<< HEAD
        // These events control the HUD and Player Opacity!
        public static event Action OnNewQuestionLoaded;
        public static event Action<int> OnValidPlayerBuzzed; 
        public static event Action<int> OnPlayerLockedOut;   
=======
        public static event Action OnNewQuestionLoaded;
        public static event Action<int> OnValidPlayerBuzzed; 
        public static event Action<int> OnPlayerLockedOut;   
        public static event Action OnBuzzerWindowReopened;
        public static event Action<int> OnVerbalEvaluationStarted;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

        private RoundConfig currentRound;
        private int questionIndex;
        private bool roundActive;
        private Coroutine activeTimer;
        private int currentlyAnsweringPlayer = -1;
        
<<<<<<< HEAD
        // Referee Variables
        private int currentQuestionAttempts = 0;
        private const int MaxAttemptsPerMCQ = 2; 
        private HashSet<int> blockedPlayers = new HashSet<int>(); 
=======
        private int currentQuestionAttempts = 0;
        private const int MaxAttemptsPerMCQ = 2; 
        private HashSet<int> blockedPlayers = new HashSet<int>(); 

        // --- NEW: VERBAL STATE MACHINE ---
        private enum VerbalState { None, WaitingForBuzz, AnsweringOriginal, Stealing, AnsweringSteal }
        private VerbalState currentVerbalState = VerbalState.None;
        private int originalVerbalPlayer = -1;
        private int verbalTargetPlayer = -1;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

        private void OnEnable()
        {
            BuzzerSystem.OnPlayerBuzzed += HandlePlayerBuzzed;
            GameplayHUD.OnAnswerEvaluated += HandleAnswerEvaluated;
        }

        private void OnDisable()
        {
            BuzzerSystem.OnPlayerBuzzed -= HandlePlayerBuzzed;
            GameplayHUD.OnAnswerEvaluated -= HandleAnswerEvaluated;
        }

        public void ClearQuestionMemory() { if (questionLoader != null) questionLoader.ClearMemory(); }

        public void InitializeRound(List<Category> categories, QuestionType type)
        {
            if (roundActive) return;

            roundActive = true;
            questionIndex = 0;
            
<<<<<<< HEAD
            currentRound = new RoundConfig
            {
                categories = categories,
                questionType = type,
                questionCount = MatchManager.Instance.questionsPerRound
            };

            if (type == QuestionType.MultipleChoice)
                currentRound.questionTimerSeconds = QuestionRules.MultipleChoiceTime;
            else if (type == QuestionType.Verbal)
                currentRound.questionTimerSeconds = QuestionRules.VerbalTime;
=======
            currentRound = new RoundConfig { categories = categories, questionType = type, questionCount = MatchManager.Instance.questionsPerRound };
            if (type == QuestionType.MultipleChoice) currentRound.questionTimerSeconds = QuestionRules.MultipleChoiceTime;
            else if (type == QuestionType.Verbal) currentRound.questionTimerSeconds = QuestionRules.VerbalTime;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

            OnRoundStarted?.Invoke(MatchManager.Instance.CurrentRoundIndex + 1);
            questionLoader.Initialize(currentRound);
            LoadNext();
        }

        private void LoadNext()
        {
            currentlyAnsweringPlayer = -1;
<<<<<<< HEAD
            
            currentQuestionAttempts = 0; 
            blockedPlayers.Clear(); 
            
            OnNewQuestionLoaded?.Invoke(); // Tells PlayerUI to restore 100% opacity!
=======
            verbalTargetPlayer = -1;
            originalVerbalPlayer = -1;
            currentQuestionAttempts = 0; 
            blockedPlayers.Clear(); 
            
            OnNewQuestionLoaded?.Invoke(); 
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

            if (questionLoader.HasMoreQuestions())
            {
                questionLoader.LoadNextQuestion();
<<<<<<< HEAD
=======
                
                // Route Verbal State
                if (questionLoader.CurrentQuestion is VerbalQuestion)
                    currentVerbalState = VerbalState.WaitingForBuzz;
                else
                    currentVerbalState = VerbalState.None;

>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
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
<<<<<<< HEAD

            onTimeOut?.Invoke(); 
        }

        private void OnBuzzTimeOut()
        {
            Debug.Log("Time's up! Nobody buzzed. Moving to next question.");
            LoadNext();
        }

        private void OnAnswerTimeOut()
        {
            Debug.Log($"Player {currentlyAnsweringPlayer} took too long to answer!");
            OnAnswerTimeOutUI?.Invoke(); 
            ProcessFailedAttempt(); 
        }

        private void HandlePlayerBuzzed(int playerIndex)
        {
            // Ignore if player is permanently blocked, OR if someone else is currently answering
            if (blockedPlayers.Contains(playerIndex) || currentlyAnsweringPlayer != -1) return;

            currentlyAnsweringPlayer = playerIndex;
            
            // Tell GameplayHUD that this is a legal buzz, open the panel!
            OnValidPlayerBuzzed?.Invoke(playerIndex); 
            
=======
            onTimeOut?.Invoke(); 
        }

        private void OnBuzzTimeOut() { LoadNext(); }

        private void HandlePlayerBuzzed(int playerIndex)
        {
            if (blockedPlayers.Contains(playerIndex) || currentlyAnsweringPlayer != -1) return;

            // --- THE VERBAL STEAL FLOW ---
            if (currentVerbalState != VerbalState.None)
            {
                if (currentVerbalState == VerbalState.WaitingForBuzz)
                {
                    // Phase 1: Original Answer
                    originalVerbalPlayer = playerIndex;
                    verbalTargetPlayer = playerIndex;
                    currentlyAnsweringPlayer = playerIndex; // Blocks others from buzzing
                    currentVerbalState = VerbalState.AnsweringOriginal;
                    
                    OnValidPlayerBuzzed?.Invoke(playerIndex); // Dims other players
                    StartTimer(QuestionRules.VerbalTime, OnVerbalAnswerOriginalFinished);
                }
                else if (currentVerbalState == VerbalState.Stealing)
                {
                    // Phase 2: A player steals!
                    verbalTargetPlayer = playerIndex;
                    currentlyAnsweringPlayer = playerIndex; // Blocks others from stealing
                    currentVerbalState = VerbalState.AnsweringSteal;
                    
                    OnValidPlayerBuzzed?.Invoke(playerIndex); // Dims other players
                    StartTimer(QuestionRules.VerbalTime, OnVerbalStealAnswerFinished);
                }
                return;
            }

            // --- MCQ FLOW ---
            currentlyAnsweringPlayer = playerIndex;
            OnValidPlayerBuzzed?.Invoke(playerIndex); 
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            StartTimer(currentRound.questionTimerSeconds, OnAnswerTimeOut);
        }

        // --- VERBAL TIMEOUTS ---
        private void OnVerbalAnswerOriginalFinished()
        {
<<<<<<< HEAD
            if (activeTimer != null) StopCoroutine(activeTimer); 
=======
            // 10s is over. Start Steal Mode!
            currentlyAnsweringPlayer = -1; // Unblock the system so someone can steal
            currentVerbalState = VerbalState.Stealing;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

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
            verbalTargetPlayer = originalVerbalPlayer; 
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

            if (result == AnswerResult.Correct || result == AnswerResult.Almost)
            {
<<<<<<< HEAD
                Debug.Log("Correct! Loading next question IMMEDIATELY.");
=======
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
                OnQuestionCompleted(); 
            }
            else
            {
<<<<<<< HEAD
                ProcessFailedAttempt();
=======
                // If they are evaluated WRONG in Verbal Mode, the question is just over
                if (questionLoader.CurrentQuestion is VerbalQuestion)
                {
                    OnQuestionCompleted();
                }
                else
                {
                    ProcessFailedAttempt();
                }
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            }
        }
        
        private void ProcessFailedAttempt()
        {
            currentQuestionAttempts++;      
            blockedPlayers.Add(currentlyAnsweringPlayer); 
<<<<<<< HEAD
            
            // Tell PlayerUI to dim the opacity for this specific player!
            OnPlayerLockedOut?.Invoke(currentlyAnsweringPlayer); 
            
            currentlyAnsweringPlayer = -1;  // Unblock the buzzer system so OTHERS can answer

            // --- THE FIX: Bulletproof True/False check ---
            bool isTrueFalse = false;
            if (questionLoader.CurrentQuestion != null)
            {
                // We check the Enum type (GetQuestionType) just in case the Unity asset was created as an MCQ by mistake!
                isTrueFalse = (questionLoader.CurrentQuestion is TrueOrFalseQuestion) || 
                              (questionLoader.CurrentQuestion.GetQuestionType() == QuestionType.TrueOrFalse);
            }

            // Rule 1: If it's True/False, skip immediately. 
            // Rule 2: If it's MCQ and we reached 2 attempts, skip immediately.
            if (isTrueFalse || currentQuestionAttempts >= MaxAttemptsPerMCQ)
            {
                Debug.Log("Question failed! No more chances. Moving to next question.");
=======
            OnPlayerLockedOut?.Invoke(currentlyAnsweringPlayer); 
            currentlyAnsweringPlayer = -1;  

            bool isTrueFalse = false;
            if (questionLoader.CurrentQuestion != null)
                isTrueFalse = (questionLoader.CurrentQuestion is TrueOrFalseQuestion) || (questionLoader.CurrentQuestion.GetQuestionType() == QuestionType.TrueOrFalse);

            if (isTrueFalse || currentQuestionAttempts >= MaxAttemptsPerMCQ)
            {
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
                OnQuestionCompleted();
            }
            else
            {
<<<<<<< HEAD
                Debug.Log("Wrong! 1 chance remaining for other players.");
=======
                OnBuzzerWindowReopened?.Invoke(); // Light the MCQ players back up!
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
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