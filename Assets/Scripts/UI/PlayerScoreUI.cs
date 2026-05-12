using UnityEngine;
using TMPro;
using Managers;

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
            // 1. Listen for points being added during the game
            PlayerManager.OnPlayerScoreUpdated += HandleScoreUpdated;
            
            // 2. THE FIX: Grab the current score instantly when the buzzer turns on!
            // This guarantees it will say "0" when a rematch starts.
            InitializeScoreDisplay();
        }

        private void OnDisable()
        {
            // Stop listening when the buzzer turns off
            PlayerManager.OnPlayerScoreUpdated -= HandleScoreUpdated;
        }

        private void InitializeScoreDisplay()
        {
            if (playerUI == null || scoreText == null) return;

            // Grab the current TotalScore from the PlayerManager Bank
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