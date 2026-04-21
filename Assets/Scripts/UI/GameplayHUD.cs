using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GamePlay.Questions;
using GamePlay.Systems;
using Managers; 

namespace UI 
{
    public class GameplayHUD : MonoBehaviour
    {
        public static Action<int, bool> OnAnswerEvaluated; 

        [Header("Timer UI")]
        [SerializeField] private TMP_Text timerText; 

        [Header("Background Question Texts")]
        [SerializeField] private TMP_Text questionTextUP;
        [SerializeField] private TMP_Text questionTextDown;

        [Header("Main Answer Panel (Parent)")]
        [SerializeField] private GameObject answerPanel; 
        [SerializeField] private RectTransform answerPanelRect; 
        [SerializeField] private TMP_Text panelQuestionText; 

        [Header("Multiple Choice Section")]
        [SerializeField] private GameObject mcqSection; 
        [SerializeField] private AnswerButtonUI[] mcqButtons; 

        [Header("True/False Section")]
        [SerializeField] private GameObject tfSection; 
        [SerializeField] private AnswerButtonUI[] tfButtons; 

        private int currentPlayer = -1;
        private List<AnswerOption> currentAnswers;
        private HashSet<AnswerOption> removedAnswers = new HashSet<AnswerOption>();

        private void OnEnable()
        {
            QuestionLoader.OnQuestionLoaded += UpdateQuestion;
            RoundManager.OnValidPlayerBuzzed += OnPlayerBuzzed; 
            RoundManager.OnTimerUpdated += UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI += ForceClosePanel;
        }

        private void OnDisable()
        {
            QuestionLoader.OnQuestionLoaded -= UpdateQuestion;
            RoundManager.OnValidPlayerBuzzed -= OnPlayerBuzzed;
            RoundManager.OnTimerUpdated -= UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI -= ForceClosePanel;
        }

        private void UpdateTimerUI(int secondsLeft)
        {
            if (timerText != null) timerText.text = secondsLeft.ToString();
        }

        private void ForceClosePanel()
        {
            currentPlayer = -1;
            if (answerPanel != null) answerPanel.SetActive(false);
        }

        void UpdateQuestion(BaseQuestion question)
        {
            currentPlayer = -1;
            removedAnswers.Clear(); // Just clear the removed buttons!

            if (answerPanel != null) answerPanel.SetActive(false);

            string fixedQuestionText = ArabicFixer.Fix(question.questionText);

            if (questionTextUP != null) questionTextUP.text = fixedQuestionText;
            if (questionTextDown != null) questionTextDown.text = fixedQuestionText;
            if (panelQuestionText != null) panelQuestionText.text = fixedQuestionText;

            if (question is TrueOrFalseQuestion tfq)
            {
                if (mcqSection != null) mcqSection.SetActive(false);
                if (tfSection != null) tfSection.SetActive(true);

                currentAnswers = tfq.GetOptions(); 
                PopulateButtons(tfButtons, currentAnswers);
            }
            else if (question is MultipleChoiceQuestion mcq)
            {
                if (tfSection != null) tfSection.SetActive(false);
                if (mcqSection != null) mcqSection.SetActive(true);

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
            currentPlayer = playerIndex;
            
            if (answerPanel != null)
            {
                if (currentAnswers.Count == 2)
                    PopulateButtons(tfButtons, currentAnswers);
                else
                    PopulateButtons(mcqButtons, currentAnswers);

                RotateAnswerPanel(playerIndex); 
                answerPanel.SetActive(true);    
            }
        }

        private void RotateAnswerPanel(int playerIndex)
        {
            if (answerPanelRect == null) return;
            
            float zRotation = 0f;
            if (playerIndex == 1 || playerIndex == 2) zRotation = 180f; 
            else if (playerIndex == 3 || playerIndex == 4) zRotation = 0f; 

            answerPanelRect.localEulerAngles = new Vector3(0, 0, zRotation);
        }

        public void OnAnswerSelected(AnswerOption selectedOption)
        {
            if (currentPlayer == -1) return;

            bool isCorrect = selectedOption.isCorrect;

            if (isCorrect)
            {
                // --- THE FIX ---
                OnAnswerEvaluated?.Invoke(currentPlayer, true);
                
                // (Removed the line that hides the panel here so the green button stays visible!)
            }
            else
            {
                OnAnswerEvaluated?.Invoke(currentPlayer, false);
                removedAnswers.Add(selectedOption); // Remove the wrong button
                
                currentPlayer = -1;
                
                // For a WRONG answer, we DO hide it instantly so the other players can see the screen and buzz!
                if (answerPanel != null) answerPanel.SetActive(false); 
            }
        }
    }
}