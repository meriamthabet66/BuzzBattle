using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlay.Configs;
using GamePlay.Systems;
using GamePlay.Questions;

namespace Managers {
    public class RoundManager : MonoBehaviour
    {

        [SerializeField] private QuestionLoader questionLoader;

        private RoundConfig currentRound;
        
        private int questionIndex;
        
        private bool roundActive;
        
        public static event Action<int> OnRoundStarted;
        
        
        private Coroutine questionTimer;

        public void InitializeRound(List<Category> categories, QuestionType type)
        {
            if (roundActive) return;

            
            if (questionLoader == null)
            {
                Debug.LogError("QuestionLoader missing in RoundManager");
                return;
            }
            
            
            if (categories == null || categories.Count == 0)
            {
                Debug.LogError("Cannot start round without categories");
                return;
            }
            
            roundActive = true;
            questionIndex = 0;
            
            currentRound = new RoundConfig
            {
                categories = categories,
                questionType = type,
                questionCount = MatchManager.Instance.questionsPerRound
            };

            switch (currentRound.questionType)
            {
                case QuestionType.MultipleChoice:
                    currentRound.questionTimerSeconds = QuestionRules.MultipleChoiceTime;
                    break;

                case QuestionType.Verbal:
                    currentRound.questionTimerSeconds = QuestionRules.VerbalTime;
                    break;
            }

            Debug.Log("Round started");
            
            OnRoundStarted?.Invoke(MatchManager.Instance.CurrentRoundIndex + 1);
            

            questionLoader.Initialize(currentRound);
            questionLoader.LoadNextQuestion();
            
            
            StartQuestionTimer();
        }

        public void OnQuestionCompleted()
        {
            questionIndex++;

            if (questionLoader.HasMoreQuestions())
            {
                questionLoader.LoadNextQuestion();
                StartQuestionTimer();
            }
            else
            {
                EndRound();
            }
        }
        
        private void StartQuestionTimer()
        {
            if (questionTimer != null)
                StopCoroutine(questionTimer);

            questionTimer = StartCoroutine(QuestionTimerRoutine());
        }

        private IEnumerator QuestionTimerRoutine()
        {
            yield return new WaitForSeconds(currentRound.questionTimerSeconds);

            OnQuestionCompleted();
        }

        private void EndRound()
        {
            Debug.Log("Round finished");
            roundActive = false;
            MatchManager.Instance.OnRoundFinished();

           
        }
    }
}