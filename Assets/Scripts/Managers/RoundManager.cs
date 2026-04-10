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

        // UI Events
        public static event Action<int> OnRoundStarted;
        public static event Action<int> OnTimerUpdated; // Sends seconds to HUD
        public static event Action OnAnswerTimeOutUI;   // Tells HUD to close answer panel

        private RoundConfig currentRound;
        private int questionIndex;
        private bool roundActive;
        
        private Coroutine activeTimer;
        private int currentlyAnsweringPlayer = -1;

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

            // Set the ANSWER time based on QuestionRules
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

            if (questionLoader.HasMoreQuestions())
            {
                questionLoader.LoadNextQuestion();
                
                // 1. Start Phase 1: Buzz Timer (Always 5 seconds)
                StartTimer(QuestionRules.BuzzTimeLimit, OnBuzzTimeOut);
            }
            else
            {
                EndRound();
            }
        }

        // --- TIMER CORE ---
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
                OnTimerUpdated?.Invoke(Mathf.CeilToInt(timeRemaining)); // Update UI
                yield return null; 
            }

            onTimeOut?.Invoke(); // Time reached 0
        }

        // --- TIMEOUT RULES ---
        private void OnBuzzTimeOut()
        {
            Debug.Log("Time's up! Nobody buzzed. Moving to next question.");
            LoadNext();
        }

        private void OnAnswerTimeOut()
        {
            Debug.Log($"Player {currentlyAnsweringPlayer} took too long to answer!");
            OnAnswerTimeOutUI?.Invoke(); // Tell HUD to hide the panel
            HandleAnswerEvaluated(currentlyAnsweringPlayer, false); // Treat as wrong answer
        }

        // --- EVENTS FROM OTHER SYSTEMS ---
        private void HandlePlayerBuzzed(int playerIndex)
        {
            currentlyAnsweringPlayer = playerIndex;
            
            // 2. Start Phase 2: Answer Timer (5s for MCQ, 10s for Verbal)
            StartTimer(currentRound.questionTimerSeconds, OnAnswerTimeOut);
        }

        private void HandleAnswerEvaluated(int playerIndex, bool isCorrect)
        {
            if (activeTimer != null) StopCoroutine(activeTimer); // Stop timer immediately

            if (isCorrect)
            {
                Debug.Log("Correct! Loading next question IMMEDIATELY.");
                OnQuestionCompleted(); // No delay, instant next question!
            }
            else
            {
                Debug.Log("Wrong! Timer restarted. Other players can buzz.");
                currentlyAnsweringPlayer = -1; // Frees up the system so anyone else can buzz
                
                // Return to Phase 1: Restart Buzz Timer so others can try
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