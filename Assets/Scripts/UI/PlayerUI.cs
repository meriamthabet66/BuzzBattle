using UnityEngine;
using UnityEngine.UI;
using GamePlay.Systems;
using Managers;
using RTLTMPro;

namespace UI 
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayerUI : MonoBehaviour
    {
        public int playerIndex; 
        public Button buzzerButton;
        
        [Header("Name Display")]
        [SerializeField] private RTLTextMeshPro nameText; 
        
        private CanvasGroup canvasGroup;
        
        // --- NEW: Tracks if this player is completely dead for this question ---
        private bool isPermanentlyBlocked = false;

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
            RoundManager.OnNewQuestionLoaded += ResetUI;
            RoundManager.OnPlayerLockedOut += HandlePermanentLockout;
            
            // --- NEW: Listen to temporary phase changes ---
            RoundManager.OnValidPlayerBuzzed += HandleSomeoneBuzzed;
            RoundManager.OnBuzzerWindowReopened += ReopenBuzzer;
        }

        private void OnDisable()
        {
            RoundManager.OnNewQuestionLoaded -= ResetUI;
            RoundManager.OnPlayerLockedOut -= HandlePermanentLockout;
            
            RoundManager.OnValidPlayerBuzzed -= HandleSomeoneBuzzed;
            RoundManager.OnBuzzerWindowReopened -= ReopenBuzzer;
        }

        public void SetupForMatch(int managerIndex)
        {
            this.playerIndex = managerIndex + 1;
            PlayerData pData = PlayerManager.Instance.GetPlayer(managerIndex);

            if (pData == null) gameObject.SetActive(false);
            else
            {
                gameObject.SetActive(true); 
                if (nameText != null) nameText.text = pData.DisplayName;
            }
        }

        // Runs when a totally fresh question loads
        private void ResetUI()
        {
            if(gameObject.activeInHierarchy) 
            {
                 isPermanentlyBlocked = false;
                 buzzerButton.interactable = true;
                 canvasGroup.alpha = 1f; 
            }
        }

        // Runs if THIS player answers wrong, OR in Verbal when Steal mode starts
        private void HandlePermanentLockout(int lockedPlayerIndex)
        {
            if (lockedPlayerIndex == this.playerIndex)
            {
                isPermanentlyBlocked = true;
                buzzerButton.interactable = false; 
                canvasGroup.alpha = 0.5f;          
            }
        }

        // --- NEW: Runs when ANY player touches the buzzer ---
        private void HandleSomeoneBuzzed(int activePlayerIndex)
        {
            if (activePlayerIndex != this.playerIndex)
            {
                // Someone else buzzed! Temporarily dim me.
                buzzerButton.interactable = false;
                canvasGroup.alpha = 0.5f;
            }
            else
            {
                // I buzzed! Keep me bright, but stop me from spam clicking.
                buzzerButton.interactable = false;
                canvasGroup.alpha = 1f;
            }
        }

        // --- NEW: Runs when the timer resets for a Steal or a Retry ---
        private void ReopenBuzzer()
        {
            // Only light back up if I haven't been permanently blocked!
            if (!isPermanentlyBlocked && gameObject.activeInHierarchy)
            {
                buzzerButton.interactable = true;
                canvasGroup.alpha = 1f;
            }
        }

        public void OnBuzz()
        {
            BuzzerSystem.OnPlayerBuzzed?.Invoke(this.playerIndex);
        }
    }
}