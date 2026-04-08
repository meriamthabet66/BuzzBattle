using UnityEngine;
using TMPro;
using GamePlay.Systems;
using GamePlay.Questions;

namespace UI {
    namespace UI
    {
        public class GameplayHUD : MonoBehaviour
        {
            [Header("Question")]
            [SerializeField] private TMP_Text questionTextUP;
            [SerializeField] private TMP_Text questionTextDown;

            [Header("Answer Panel")]
            [SerializeField] private GameObject answerPanel;

            private int currentPlayer = -1;

            private void OnEnable()
            {
                QuestionLoader.OnQuestionLoaded += UpdateQuestion;
                BuzzerSystem.OnPlayerBuzzed += OnPlayerBuzzed;
            }

            private void OnDisable()
            {
                QuestionLoader.OnQuestionLoaded -= UpdateQuestion;
                BuzzerSystem.OnPlayerBuzzed -= OnPlayerBuzzed;
            }

            void UpdateQuestion(BaseQuestion question)
            {
                currentPlayer = -1;

                if (answerPanel != null)
                    answerPanel.SetActive(false);

                // ✅ Update BOTH texts
                if (questionTextUP != null)
                    questionTextUP.text = question.questionText;

                if (questionTextDown != null)
                    questionTextDown.text = question.questionText;
            }

            void OnPlayerBuzzed(int playerIndex)
            {
                if (currentPlayer != -1) return;

                currentPlayer = playerIndex;

                Debug.Log("Player " + playerIndex + " is answering");

                if (answerPanel != null)
                    answerPanel.SetActive(true);
            }
        }
    }
}