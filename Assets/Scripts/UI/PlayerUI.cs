using Data;
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
        
        [Header("Character Display")]
        [SerializeField] private FoxOutfitRenderer buzzerOutfitRenderer; 
        
        private CanvasGroup canvasGroup;
        
        // --- NEW: Tracks if this player is completely dead for this question ---
        private bool isPermanentlyBlocked = false;
        
        [Header("Animation")]
        [SerializeField] private Animator characterAnimator; // Drag the GameObject with the Animator here!

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
            
            RoundManager.OnPlayAnimation += TriggerAnimation; // NEW
        }

        private void OnDisable()
        {
            RoundManager.OnNewQuestionLoaded -= ResetUI;
            RoundManager.OnPlayerLockedOut -= HandlePermanentLockout;
            
            RoundManager.OnValidPlayerBuzzed -= HandleSomeoneBuzzed;
            RoundManager.OnBuzzerWindowReopened -= ReopenBuzzer;
            
            RoundManager.OnPlayAnimation -= TriggerAnimation; // NEW
        }

        public void SetupForMatch(int managerIndex)
        {
            this.playerIndex = managerIndex + 1;
            PlayerData pData = PlayerManager.Instance.GetPlayer(managerIndex);

            if (pData == null || pData.IsEliminated) 
            {
                gameObject.SetActive(false); 
            }
            else
            {
                gameObject.SetActive(true); 
                isPermanentlyBlocked = false; 

                if (buzzerButton != null) buzzerButton.interactable = true;
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                if (nameText != null) nameText.text = pData.DisplayName;

                // --- THE FIX: Always show the Fox on the buzzer! ---
                if (buzzerOutfitRenderer != null)
                {
                    buzzerOutfitRenderer.gameObject.SetActive(true); // Always true
                    
                    // If -1, it will strip the clothes off and just show the base fox
                    CharacterData tempFox = new CharacterData { id = pData.SelectedCharacterID };
                    buzzerOutfitRenderer.RenderOutfit(tempFox);
                }
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
        
        
        private void TriggerAnimation(int targetPlayerIndex, string animationName)
        {
            // Only play if this message is for ME!
            if (targetPlayerIndex == this.playerIndex && characterAnimator != null)
            {
                characterAnimator.SetTrigger(animationName);
            }
        }
    }
}