using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay.Configs;
using GamePlay.Systems;
using GamePlay.Questions;
using UI; 

namespace Managers {
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private QuestionLoader questionLoader;

        // --- NEW: UI Events ---
        public static event Action<int> OnRoundStarted;
        public static event Action<int> OnTimerUpdated; 
        public static event Action OnAnswerTimeOutUI;   
        
        // These events control the HUD and Player Opacity!
        public static event Action OnNewQuestionLoaded;
        public static event Action<int> OnValidPlayerBuzzed; 
        public static event Action<int> OnPlayerLockedOut;   

        private RoundConfig currentRound;
        private int questionIndex;
        private bool roundActive;
        
        private Coroutine activeTimer;
        private int currentlyAnsweringPlayer = -1;
        
        // Referee Variables
        private int currentQuestionAttempts = 0;
        private const int MaxAttemptsPerMCQ = 2; 
        private HashSet<int> blockedPlayers = new HashSet<int>(); 

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

        public void InitializeRound(List<Category> categories, QuestionType type)
        {
            if (roundActive) return;

            roundActive = true;
            questionIndex = 0;
            
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

            OnRoundStarted?.Invoke(MatchManager.Instance.CurrentRoundIndex + 1);
            
            questionLoader.Initialize(currentRound);
            LoadNext();
        }

        private void LoadNext()
        {
            currentlyAnsweringPlayer = -1;
            
            currentQuestionAttempts = 0; 
            blockedPlayers.Clear(); 
            
            OnNewQuestionLoaded?.Invoke(); // Tells PlayerUI to restore 100% opacity!

            if (questionLoader.HasMoreQuestions())
            {
                questionLoader.LoadNextQuestion();
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
            
            StartTimer(currentRound.questionTimerSeconds, OnAnswerTimeOut);
        }

        private void HandleAnswerEvaluated(int playerIndex, bool isCorrect)
        {
            if (activeTimer != null) StopCoroutine(activeTimer); 

            if (isCorrect)
            {
                Debug.Log("Correct! Loading next question IMMEDIATELY.");
                OnQuestionCompleted(); 
            }
            else
            {
                ProcessFailedAttempt();
            }
        }
        
        private void ProcessFailedAttempt()
        {
            currentQuestionAttempts++;      
            blockedPlayers.Add(currentlyAnsweringPlayer); 
            
            // Tell PlayerUI to dim the opacity for this specific player!
            OnPlayerLockedOut?.Invoke(currentlyAnsweringPlayer); 
            
            currentlyAnsweringPlayer = -1;  // Unblock the buzzer system so OTHERS can answer

            bool isTrueFalse = questionLoader.CurrentQuestion is TrueOrFalseQuestion;

            if (isTrueFalse || currentQuestionAttempts >= MaxAttemptsPerMCQ)
            {
                Debug.Log("Question failed! No more chances. Moving to next question.");
                OnQuestionCompleted();
            }
            else
            {
                Debug.Log("Wrong! 1 chance remaining for other players.");
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