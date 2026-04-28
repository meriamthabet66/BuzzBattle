using Core.Enums;
using UnityEngine;


namespace Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void CalculateAndApplyScore(int playerIndex, AnswerResult result, bool isVerbal, bool isSteal)
        {
            int points = 0;

            if (!isVerbal) 
            {
                // --- MCQ / TF ---
                if (result == AnswerResult.Correct) points = ScoreRules.McqCorrect;
                else if (result == AnswerResult.Wrong) points = ScoreRules.McqWrong;
            }
            else 
            {
                // --- VERBAL ---
                if (isSteal)
                {
                    if (result == AnswerResult.Correct) points = ScoreRules.StealCorrect;
                    else if (result == AnswerResult.Almost) points = ScoreRules.StealAlmost;
                    else if (result == AnswerResult.Wrong) points = ScoreRules.StealWrong;
                }
                else
                {
                    if (result == AnswerResult.Correct) points = ScoreRules.VerbalCorrect;
                    else if (result == AnswerResult.Almost) points = ScoreRules.VerbalAlmost;
                    else if (result == AnswerResult.Wrong) points = ScoreRules.VerbalWrong;
                }
            }

            // --- THE FIX: Convert the 1-4 Buzzer ID to the 0-3 List Index ---
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.AddScore(playerIndex - 1, points);
            }
        }
    }
}