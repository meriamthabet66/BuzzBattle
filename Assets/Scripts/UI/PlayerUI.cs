using UnityEngine;
using UnityEngine.UI;
using GamePlay.Systems;
using Managers;
<<<<<<< HEAD

namespace UI {
    [RequireComponent(typeof(CanvasGroup))] // Automatically adds CanvasGroup in Unity
=======
using RTLTMPro;

namespace UI 
{
    [RequireComponent(typeof(CanvasGroup))]
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
    public class PlayerUI : MonoBehaviour
    {
        public int playerIndex; 
        public Button buzzerButton;
        
<<<<<<< HEAD
        private CanvasGroup canvasGroup;
=======
        [Header("Name Display")]
        [SerializeField] private RTLTextMeshPro nameText; 
        
        private CanvasGroup canvasGroup;
        
        // --- NEW: Tracks if this player is completely dead for this question ---
        private bool isPermanentlyBlocked = false;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

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
<<<<<<< HEAD
            // Listen to the Referee!
            RoundManager.OnNewQuestionLoaded += ResetUI;
            RoundManager.OnPlayerLockedOut += HandleLockout;
=======
            RoundManager.OnNewQuestionLoaded += ResetUI;
            RoundManager.OnPlayerLockedOut += HandlePermanentLockout;
            
            // --- NEW: Listen to temporary phase changes ---
            RoundManager.OnValidPlayerBuzzed += HandleSomeoneBuzzed;
            RoundManager.OnBuzzerWindowReopened += ReopenBuzzer;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
        }

        private void OnDisable()
        {
            RoundManager.OnNewQuestionLoaded -= ResetUI;
<<<<<<< HEAD
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
=======
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
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            }
        }

        public void OnBuzz()
        {
            BuzzerSystem.OnPlayerBuzzed?.Invoke(this.playerIndex);
        }
    }
}