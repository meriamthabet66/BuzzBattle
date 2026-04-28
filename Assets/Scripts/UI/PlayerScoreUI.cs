using UnityEngine;
using TMPro;
using Managers;
using System;

namespace UI 
{
    public class PlayerScoreUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Drag the PlayerUI script from this buzzer here")]
        [SerializeField] private PlayerUI playerUI; 
        
        [Header("Display")]
        [SerializeField] private TMP_Text scoreText;

        private void OnEnable()
        {
            // Listen to the Bank for point changes
            PlayerManager.OnPlayerScoreUpdated += HandleScoreUpdated;
            
            // Listen for the game start so we can set the text to "0"
            GameManager.OnGameplayStart += InitializeScoreDisplay;
        }

        private void OnDisable()
        {
            PlayerManager.OnPlayerScoreUpdated -= HandleScoreUpdated;
            GameManager.OnGameplayStart -= InitializeScoreDisplay;
        }

        private void InitializeScoreDisplay()
        {
            if (playerUI == null || scoreText == null) return;

            // Get the starting score (usually 0) from the manager
            PlayerData pData = PlayerManager.Instance.GetPlayer(playerUI.playerIndex - 1);
            if (pData != null)
            {
                scoreText.text = pData.TotalScore.ToString();
            }
        }

        private void HandleScoreUpdated(int listIndex, int newScore)
        {
            if (playerUI == null || scoreText == null) return;

            // Is the Bank talking about ME?
            if (listIndex == playerUI.playerIndex - 1)
            {
                // Update my screen!
                scoreText.text = newScore.ToString();
            }
        }
    }
}