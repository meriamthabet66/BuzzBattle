using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data; 
using Data.DTO;
using UI.Panels;

namespace UI 
{
    public class PlayerInputSectionUI : MonoBehaviour
    {
        [Header("Guest Input")]
        [SerializeField] private TMP_InputField guestNameInput;

        [Header("Display")]
        [SerializeField] private RTLTextMeshPro displayNameText; // RTLInputDisplay handles visual updates
        [SerializeField] private Button openPopupButton;
        [SerializeField] private Image characterOverlayImage; 
        
        [Header("Controls")]
        [SerializeField] private Button clearAccountButton; 
        
        [Header("Popup")]
        [SerializeField] private CharacterLinkPopup linkPopup;

        public ProfileDTO LinkedProfile { get; private set; }
        public string DisplayName { get; private set; }
        public long CharacterId { get; private set; }
        
        [Tooltip("Set this to 1, 2, 3, or 4 in the inspector")]
        public int playerIndex;

        private void Start()
        {
            if (openPopupButton != null) openPopupButton.onClick.AddListener(OnOpenPopup);
            if (clearAccountButton != null) clearAccountButton.onClick.AddListener(ClearSlot);
            if (guestNameInput != null) guestNameInput.onValueChanged.AddListener(OnNameTyped);

            ClearSlot(); // Start clean
        }

        private void OnNameTyped(string name)
        {
            // Only update the internal string if no account is linked.
            // The RTLInputDisplay component handles updating the visual text automatically!
            if (LinkedProfile == null)
            {
                DisplayName = name;
            }
        }

        private void OnOpenPopup()
        {
            if (linkPopup != null) linkPopup.OpenForSlot(this);
        }

        public void SetLinkedData(long charId, ProfileDTO profile)
        {
            this.CharacterId = charId;
            this.LinkedProfile = profile;

            if (profile != null)
            {
                // 1. Lock the slot to the Account
                DisplayName = profile.username;
                
                // We physically update the input field so the RTL script reacts to it
                if (guestNameInput != null) 
                {
                    // Temporarily remove listener so it doesn't trigger OnNameTyped
                    guestNameInput.onValueChanged.RemoveListener(OnNameTyped);
                    guestNameInput.text = profile.username;
                    guestNameInput.onValueChanged.AddListener(OnNameTyped);
                    
                    guestNameInput.interactable = false; // Lock the input
                }
                
                if (clearAccountButton != null) clearAccountButton.gameObject.SetActive(true);
            }

            if (characterOverlayImage != null)
            {
                characterOverlayImage.gameObject.SetActive(charId != -1);
            }

            // Tell the main panel to recheck if the Host Button should be disabled
            PlayerSetupPanelUI hostPanel = GetComponentInParent<PlayerSetupPanelUI>();
            if (hostPanel != null) hostPanel.RefreshHostButtonState();
        }

        public void ClearSlot()
        {
            // 1. Wipe the data
            this.LinkedProfile = null;
            this.CharacterId = -1;
            this.DisplayName = ""; // Wipe the internal name
            
            // 2. Wipe the input field completely (This will trigger OnNameTyped automatically with "")
            if (guestNameInput != null) 
            {
                guestNameInput.text = ""; 
                guestNameInput.interactable = true; // Unlock the input
            }

            // 3. Set the visual placeholder (if input is empty, RTL will show nothing, so we set a default)
            if (displayNameText != null) 
            {
                displayNameText.text = $"لاعب {playerIndex}";
            }

            // 4. Hide visuals
            if (characterOverlayImage != null) characterOverlayImage.gameObject.SetActive(false);
            if (clearAccountButton != null) clearAccountButton.gameObject.SetActive(false);

            // 5. Tell the main panel to recheck the Host Button
            PlayerSetupPanelUI hostPanel = GetComponentInParent<PlayerSetupPanelUI>();
            if (hostPanel != null) hostPanel.RefreshHostButtonState();
        }

        public void AddSelfToManager()
        {
            string finalName = !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : $"لاعب {playerIndex}";
            AccountData accountData = null;

            if (LinkedProfile != null)
            {
                accountData = new AccountData {
                    id = LinkedProfile.id,
                    email = LinkedProfile.email,
                    Username = LinkedProfile.username,
                    Stars = LinkedProfile.stars,
                    Score = LinkedProfile.score,
                    Steals = LinkedProfile.total_steals,
                    CorrectSteals = LinkedProfile.correct_steals,
                    MatchWinCount = LinkedProfile.match_wins,
                    MatchPlayedCount = LinkedProfile.matches_played,
                    TournamentWinCount = LinkedProfile.tournament_wins
                };
            }

            Managers.PlayerManager.Instance.AddPlayer(finalName, (int)CharacterId, accountData);
        }
    }
}