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
                if (isSteal)
                {
                    // --- NEW: MCQ STEAL SCORES ---
                    if (result == AnswerResult.Correct) points = ScoreRules.StealCorrect;
                    else if (result == AnswerResult.Wrong) points = ScoreRules.StealWrong;
                }
                else
                {
                    if (result == AnswerResult.Correct) points = ScoreRules.McqCorrect;
                    else if (result == AnswerResult.Wrong) points = ScoreRules.McqWrong;
                }
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

            if (PlayerManager.Instance != null)
            {
                int listIndex = playerIndex - 1;
                PlayerManager.Instance.AddScore(listIndex, points);

                // --- THE FIX: TRACK BOTH ATTEMPTS AND SUCCESSES ---
                if (isSteal)
                {
                    var pData = PlayerManager.Instance.GetPlayer(listIndex);
                    if (pData != null)
                    {
                        // 1. Always increment the total attempts (for the denominator)
                        pData.Steals++; 
                        
                        // 2. Increment success only if they got it Right or Almost Right
                        if (result == AnswerResult.Correct || result == AnswerResult.Almost)
                        {
                            pData.CorrectStealsInMatch++;
                            Debug.Log($"<color=magenta>STEAL SUCCESS! {pData.DisplayName} Successes: {pData.CorrectStealsInMatch} / Attempts: {pData.Steals}</color>");
                        }
                    }
                }
            }
        }
    }
}