using System;
using System.Collections.Generic;
using Core.Enums;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using GamePlay.Questions;
using GamePlay.Systems;
using Managers;
using RTLTMPro;

namespace UI 
{
    public class GameplayHUD : MonoBehaviour
    {
        public static Action<int, AnswerResult> OnAnswerEvaluated; 

        [Header("Player Buzzers")]
        [SerializeField] private PlayerUI[] playerBuzzers;
        [SerializeField] private TMP_Text timerText; 

        [Header("Background Question Texts")]
        [SerializeField] private RTLTextMeshPro questionTextUP;
        [SerializeField] private RTLTextMeshPro questionTextDown;

        [Header("Main Answer Panel (MCQ)")]
        [SerializeField] private GameObject answerPanel; 
        [SerializeField] private RectTransform answerPanelRect; 
        [SerializeField] private RTLTextMeshPro panelQuestionText; 

        [Header("Multiple Choice Section")]
        [SerializeField] private GameObject mcqSection; 
        [SerializeField] private AnswerButtonUI[] mcqButtons; 
        [SerializeField] private GameObject tfSection; 
        [SerializeField] private AnswerButtonUI[] tfButtons; 

        [Header("Verbal Section")]
        [SerializeField] private GameObject verbalSection; 
        [SerializeField] private RTLTextMeshPro verbalPanelQuestionText; 
        [SerializeField] private RTLTextMeshPro verbalAnswerText; 
        [SerializeField] private Button correctBtn;
        [SerializeField] private Button almostBtn;
        [SerializeField] private Button wrongBtn;

        private int currentPlayer = -1;
        private List<AnswerOption> currentAnswers;
        private HashSet<AnswerOption> removedAnswers = new HashSet<AnswerOption>();
        
        // --- NEW: Remembers the current question type ---
        private BaseQuestion currentLoadedQuestion;

        private void Awake()
        {
            if (correctBtn != null) correctBtn.onClick.AddListener(() => EvaluateVerbal(AnswerResult.Correct));
            if (almostBtn != null) almostBtn.onClick.AddListener(() => EvaluateVerbal(AnswerResult.Almost));
            if (wrongBtn != null) wrongBtn.onClick.AddListener(() => EvaluateVerbal(AnswerResult.Wrong));
        }

        private void OnEnable()
        {
            QuestionLoader.OnQuestionLoaded += UpdateQuestion;
            RoundManager.OnValidPlayerBuzzed += OnPlayerBuzzed; 
            RoundManager.OnVerbalEvaluationStarted += OpenVerbalPanel;
            RoundManager.OnTimerUpdated += UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI += ForceClosePanel;
            GameManager.OnGameplayStart += InitializePlayerBuzzers;
        }

        private void OnDisable()
        {
            QuestionLoader.OnQuestionLoaded -= UpdateQuestion;
            RoundManager.OnValidPlayerBuzzed -= OnPlayerBuzzed;
            RoundManager.OnVerbalEvaluationStarted -= OpenVerbalPanel;
            RoundManager.OnTimerUpdated -= UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI -= ForceClosePanel;
            GameManager.OnGameplayStart -= InitializePlayerBuzzers;
            
            ForceClosePanel();
        }

        private void InitializePlayerBuzzers() 
        {
            for (int i = 0; i < playerBuzzers.Length; i++)
                if (playerBuzzers[i] != null) playerBuzzers[i].SetupForMatch(i);
        }

        private void UpdateTimerUI(int secondsLeft)
        {
            if (timerText != null) timerText.text = secondsLeft.ToString();
        }

        private void ForceClosePanel()
        {
            currentPlayer = -1;
            if (answerPanel != null) answerPanel.SetActive(false);
            if (verbalSection != null) verbalSection.SetActive(false);
        }

        void UpdateQuestion(BaseQuestion question)
        {
            currentPlayer = -1;
            removedAnswers.Clear(); 
            currentLoadedQuestion = question; // Save it!
            
            if (answerPanel != null) answerPanel.SetActive(false);
            if (verbalSection != null) verbalSection.SetActive(false);

            string fixedQuestionText = question.questionText;
            if (questionTextUP != null) questionTextUP.text = fixedQuestionText;
            if (questionTextDown != null) questionTextDown.text = fixedQuestionText;
            if (panelQuestionText != null) panelQuestionText.text = fixedQuestionText;
            if (verbalPanelQuestionText != null) verbalPanelQuestionText.text = fixedQuestionText;

            if (mcqSection != null) mcqSection.SetActive(false);
            if (tfSection != null) tfSection.SetActive(false);

            if (question is VerbalQuestion vq)
            {
                if (verbalAnswerText != null) verbalAnswerText.text = vq.correctAnswer;
            }
            else if (question is TrueOrFalseQuestion tfq)
            {
                currentAnswers = tfq.GetOptions(); 
                PopulateButtons(tfButtons, currentAnswers);
            }
            else if (question is MultipleChoiceQuestion mcq)
            {
                currentAnswers = mcq.GetShuffledAnswers();
                PopulateButtons(mcqButtons, currentAnswers);
            }
        }

        private void PopulateButtons(AnswerButtonUI[] buttonsToUse, List<AnswerOption> answersData)
        {
            for (int i = 0; i < buttonsToUse.Length; i++)
            {
                if (i < answersData.Count)
                {
                    if (removedAnswers.Contains(answersData[i]))
                        buttonsToUse[i].gameObject.SetActive(false);
                    else
                    {
                        buttonsToUse[i].gameObject.SetActive(true);
                        buttonsToUse[i].Setup(answersData[i], OnAnswerSelected); 
                    }
                }
                else
                {
                    buttonsToUse[i].gameObject.SetActive(false); 
                }
            }
        }

        void OnPlayerBuzzed(int playerIndex)
        {
            if (currentLoadedQuestion == null) {
                Debug.LogWarning("Buzzer pressed but no question is loaded!");
                return;
            }

            currentPlayer = playerIndex;
            
            if (currentLoadedQuestion is VerbalQuestion) return; 

            if (answerPanel != null)
            {
                if (currentAnswers.Count == 2) PopulateButtons(tfButtons, currentAnswers);
                else PopulateButtons(mcqButtons, currentAnswers);

                if (tfSection != null) tfSection.SetActive(currentAnswers.Count == 2);
                if (mcqSection != null) mcqSection.SetActive(currentAnswers.Count > 2);

                RotateAnswerPanel(playerIndex, answerPanelRect); 
                answerPanel.SetActive(true);    
            }
        }

        private void OpenVerbalPanel(int playerIndex)
        {
            currentPlayer = playerIndex;
            if (verbalSection != null)
            {
                verbalSection.SetActive(true);
            }
        }

        private void RotateAnswerPanel(int playerIndex, RectTransform rect)
        {
            if (rect == null) return;
            float zRotation = 0f;
            if (playerIndex == 1 || playerIndex == 2) zRotation = 180f; 
            else if (playerIndex == 3 || playerIndex == 4) zRotation = 0f; 
            rect.localEulerAngles = new Vector3(0, 0, zRotation);
        }

        public void OnAnswerSelected(AnswerOption selectedOption)
        {
            if (currentPlayer == -1) return;

            if (selectedOption.isCorrect)
            {
                OnAnswerEvaluated?.Invoke(currentPlayer, AnswerResult.Correct);
            }
            else
            {
                OnAnswerEvaluated?.Invoke(currentPlayer, AnswerResult.Wrong);
                removedAnswers.Add(selectedOption); 
                currentPlayer = -1;
                if (answerPanel != null) answerPanel.SetActive(false); 
            }
        }

        private void EvaluateVerbal(AnswerResult result)
        {
            if (currentPlayer == -1) return;
            OnAnswerEvaluated?.Invoke(currentPlayer, result);
            if (verbalSection != null) verbalSection.SetActive(false); 
        }
    }
}