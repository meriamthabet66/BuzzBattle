using UnityEngine;
using UnityEngine.UI;
using GamePlay.Systems;
using Managers;

namespace UI {
    [RequireComponent(typeof(CanvasGroup))] // Automatically adds CanvasGroup in Unity
    public class PlayerUI : MonoBehaviour
    {
        public int playerIndex;
        public Button buzzerButton;
        
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            buzzerButton.onClick.AddListener(OnBuzz);
        }

        private void OnEnable()
        {
            // Listen to the Referee!
            RoundManager.OnNewQuestionLoaded += ResetUI;
            RoundManager.OnPlayerLockedOut += HandleLockout;
        }

        private void OnDisable()
        {
            RoundManager.OnNewQuestionLoaded -= ResetUI;
            RoundManager.OnPlayerLockedOut -= HandleLockout;
        }

        private void ResetUI()
        {
            // Unblock this player and restore 100% opacity when a new question starts
            buzzerButton.interactable = true;
            canvasGroup.alpha = 1f; 
        }

        private void HandleLockout(int lockedPlayerIndex)
        {
            // If the RoundManager says THIS player is blocked, dim them!
            if (lockedPlayerIndex == playerIndex)
            {
                buzzerButton.interactable = false; // Physically prevents clicking
                canvasGroup.alpha = 0.5f;          // Lowers opacity to 50%
            }
        }

        public void OnBuzz()
        {
            Debug.Log("Player buzzed: " + playerIndex);
            BuzzerSystem.OnPlayerBuzzed?.Invoke(playerIndex);
        }
    }
}