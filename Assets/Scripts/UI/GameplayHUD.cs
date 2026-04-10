using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GamePlay.Systems;
using GamePlay.Questions;
using Managers; // <-- Added to listen to RoundManager's Timer!

namespace UI 
{
    public class GameplayHUD : MonoBehaviour
    {
        // Sends (PlayerIndex, IsCorrect) to the RoundManager/ScoreManager
        public static Action<int, bool> OnAnswerEvaluated; 

        // --- NEW TIMER LOGIC ---
        [Header("Timer UI")]
        [SerializeField] private TMP_Text timerText; // Drag your Timer Text object here in Unity!
        // -----------------------

        [Header("Question Texts")]
        [SerializeField] private TMP_Text questionTextUP;
        [SerializeField] private TMP_Text questionTextDown;

        [Header("Main Answer Panel (Parent)")]
        [SerializeField] private GameObject answerPanel; // Drag 'AnswerPanel' here
        [SerializeField] private RectTransform answerPanelRect; // Drag 'AnswerPanel' here too!

        [Header("Multiple Choice Section")]
        [SerializeField] private GameObject mcqSection; // Drag 'MCQAnswersSection' here
        [SerializeField] private AnswerButtonUI[] mcqButtons; // Drag the 4 AnswerBtns here

        [Header("True/False Section")]
        [SerializeField] private GameObject tfSection; // Drag 'TFAnswersSection' here
        [SerializeField] private AnswerButtonUI[] tfButtons; // Drag the 2 AnswerBtns here

        private int currentPlayer = -1;
        private List<AnswerOption> currentAnswers;
        private HashSet<AnswerOption> removedAnswers = new HashSet<AnswerOption>();
        private HashSet<int> blockedPlayers = new HashSet<int>();

        private void OnEnable()
        {
            QuestionLoader.OnQuestionLoaded += UpdateQuestion;
            BuzzerSystem.OnPlayerBuzzed += OnPlayerBuzzed;

            // --- NEW TIMER LOGIC: Listen for the timer changing ---
            RoundManager.OnTimerUpdated += UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI += ForceClosePanel;
        }

        private void OnDisable()
        {
            QuestionLoader.OnQuestionLoaded -= UpdateQuestion;
            BuzzerSystem.OnPlayerBuzzed -= OnPlayerBuzzed;

            // --- NEW TIMER LOGIC: Stop listening ---
            RoundManager.OnTimerUpdated -= UpdateTimerUI;
            RoundManager.OnAnswerTimeOutUI -= ForceClosePanel;
        }

        // --- NEW TIMER LOGIC: Update the visual number ---
        private void UpdateTimerUI(int secondsLeft)
        {
            if (timerText != null)
            {
                timerText.text = secondsLeft.ToString();
            }
        }

        // --- NEW TIMER LOGIC: Hide panel if they run out of time ---
        private void ForceClosePanel()
        {
            if (currentPlayer != -1)
            {
                blockedPlayers.Add(currentPlayer); // Block them for taking too long!
            }

            currentPlayer = -1;
            if (answerPanel != null) answerPanel.SetActive(false);
        }

        void UpdateQuestion(BaseQuestion question)
        {
            currentPlayer = -1;
            blockedPlayers.Clear();
            removedAnswers.Clear();

            // 1. Hide the entire AnswerPanel while we wait for someone to buzz
            if (answerPanel != null) answerPanel.SetActive(false);

            // 2. Set the Question Text on the screen
            if (questionTextUP != null) 
                questionTextUP.text = ArabicFixer.Fix(question.questionText);

            if (questionTextDown != null) 
                questionTextDown.text = ArabicFixer.Fix(question.questionText);
            
            // 3. Prep the right section in the background!
            if (question is TrueOrFalseQuestion tfq)
            {
                // Turn ON T/F section, turn OFF MCQ section
                if (mcqSection != null) mcqSection.SetActive(false);
                if (tfSection != null) tfSection.SetActive(true);

                currentAnswers = tfq.GetOptions(); 
                PopulateButtons(tfButtons, currentAnswers);
            }
            else if (question is MultipleChoiceQuestion mcq)
            {
                // Turn ON MCQ section, turn OFF T/F section
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
                    // NEW: If this answer was already guessed incorrectly, hide this button completely!
                    if (removedAnswers.Contains(answersData[i]))
                    {
                        buttonsToUse[i].gameObject.SetActive(false);
                    }
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
            // If someone is answering, or THIS specific player answered wrong earlier, ignore them.
            // (Other players are NOT in blockedPlayers, so they can buzz!)
            if (currentPlayer != -1 || blockedPlayers.Contains(playerIndex)) return;

            currentPlayer = playerIndex;
            Debug.Log("Player " + playerIndex + " is answering");

            if (answerPanel != null)
            {
                // NEW: Refresh the buttons right before showing the panel so the wrong answer disappears!
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
            
            //Players 1 & 2 are top, Players 3 & 4 are bottom
            if (playerIndex == 1 || playerIndex == 2) 
                zRotation = 180f; 
            else if (playerIndex == 3 || playerIndex == 4) 
                zRotation = 0f; 

            answerPanelRect.localEulerAngles = new Vector3(0, 0, zRotation);
        }

        public void OnAnswerSelected(AnswerOption selectedOption)
        {
            if (currentPlayer == -1) return;

            bool isCorrect = selectedOption.isCorrect;

            if (isCorrect)
            {
                Debug.Log($"Player {currentPlayer} got it CORRECT!");
                OnAnswerEvaluated?.Invoke(currentPlayer, true);
                
                // Hide panel, wait for next question
                if (answerPanel != null) answerPanel.SetActive(false);
            }
            else
            {
                Debug.Log($"Player {currentPlayer} got it WRONG!");
                OnAnswerEvaluated?.Invoke(currentPlayer, false);

                // Block this player, remove the wrong answer from screen
                blockedPlayers.Add(currentPlayer);
                removedAnswers.Add(selectedOption);
                
                currentPlayer = -1;
                
                // Hide panel so someone else can buzz!
                if (answerPanel != null) answerPanel.SetActive(false); 
            }
        }
    }
}