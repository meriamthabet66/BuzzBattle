using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Data;
using Data.Data;
using Data.DTO;
using Managers;
using RTLTMPro;
using UI.Panels;

namespace UI
{
    public class CharacterLinkPopup : MonoBehaviour
    {
        [Header("UI Sections")]
        [SerializeField] private GameObject accountSearchSection;
        [SerializeField] private Toggle linkAccountToggle; 
        [SerializeField] private Image linkAccountToggleImage;
        [SerializeField] private Sprite Toggled;
        [SerializeField] private Sprite NotToggled;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private Button searchButton; // The search button
        

        [Header("Character Selection")]
        [SerializeField] private Transform characterGridContent;
        [SerializeField] private GameObject characterItemPrefab;
        
        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro searchStatusText; 

        private PlayerInputSectionUI targetSlot;
        private ProfileDTO foundProfile = null;
        private CharacterChoiceUI selectedCharacterChoice = null;

        private void Awake() {
            linkAccountToggle.onValueChanged.AddListener(OnToggleChanged);
            if (searchButton != null) searchButton.onClick.AddListener(OnSearchByEmail);
        }
        
        

        public async void OpenForSlot(PlayerInputSectionUI slot) 
        {
            this.targetSlot = slot;
            this.foundProfile = null;
            this.selectedCharacterChoice = null;

            if (emailInput != null) emailInput.text = "";
            if (searchStatusText != null) searchStatusText.text = "";
            
            linkAccountToggle.isOn = false;
            OnToggleChanged(false);

            gameObject.SetActive(true);

            // Fetch and populate with the Host's characters
            string hostId = PlayerManager.Instance.HostAccount?.id;
            if (!string.IsNullOrEmpty(hostId)) 
            {
                if (searchStatusText) searchStatusText.text = "Loading Host Foxes...";
                
                // WAIT for the database to return the Host's characters
                List<CharacterData> hostChars = await SupabaseManager.Instance.FetchCharactersForPlayer(hostId);
                PopulateCharacterList(hostChars);
                
                if (searchStatusText) searchStatusText.text = "";
            }
            else
            {
                Debug.LogWarning("No Host ID found. Cannot load default characters.");
            }
        }

        private async void OnToggleChanged(bool isAccountLinkActive) 
        {
            if (linkAccountToggleImage != null)
                linkAccountToggleImage.sprite = isAccountLinkActive ? Toggled : NotToggled;
            
            if (accountSearchSection != null)
                accountSearchSection.SetActive(isAccountLinkActive);
            
            if (!isAccountLinkActive) 
            {
                foundProfile = null;
                if (searchStatusText != null) searchStatusText.text = "";
                if (emailInput != null) emailInput.text = "";
                
                // Revert to Host's characters
                string hostId = PlayerManager.Instance.HostAccount?.id;
                if (!string.IsNullOrEmpty(hostId)) 
                {
                    List<CharacterData> hostChars = await SupabaseManager.Instance.FetchCharactersForPlayer(hostId);
                    PopulateCharacterList(hostChars);
                }
            }
        }

        public async void OnSearchByEmail()
        {
            if (searchStatusText) searchStatusText.text = "Searching...";
            foundProfile = await SupabaseManager.Instance.SearchPlayerByEmail(emailInput.text);

            if (foundProfile != null)
            {
                // --- THE NEW DUPLICATE CHECK ---
                PlayerSetupPanelUI hostPanel = GetComponentInParent<PlayerSetupPanelUI>();
                if (hostPanel != null && hostPanel.IsAccountAlreadyInSlot(foundProfile.id))
                {
                    if (searchStatusText) searchStatusText.text = "الحساب مستخدم مسبقاً!"; // "Account already in use!"
                    foundProfile = null; // Prevent them from linking it
                    return;
                }

                if (searchStatusText) searchStatusText.text = $"Found: {foundProfile.username}";
                
                // Fetch and populate characters...
                List<CharacterData> characters = await SupabaseManager.Instance.FetchCharactersForPlayer(foundProfile.id);
                PopulateCharacterList(characters);
            }
            else
            {
                if (searchStatusText) searchStatusText.text = "User Not Found";
            }
        }

        // Update these methods in CharacterLinkPopup.cs

        private void PopulateCharacterList(List<CharacterData> characters) 
        {
            foreach (Transform child in characterGridContent) Destroy(child.gameObject);
        
            selectedCharacterChoice = null;

            if (characters == null || characters.Count == 0) return;
        
            foreach (var characterData in characters) 
            {
                GameObject newObj = Instantiate(characterItemPrefab, characterGridContent);
                var choiceUI = newObj.GetComponent<CharacterChoiceUI>();
            
                if (choiceUI != null) 
                {
                    choiceUI.Setup(characterData, this);
                }

                // --- THE FIX: We REMOVED the Auto-Select logic here! ---
                // Now, they all start unselected by default.
            }
        }

        public void OnCharacterSelected(CharacterChoiceUI choice) 
        {
            // --- THE FIX: Toggle Logic ---
            // If they clicked the one that is already selected, DESELECT it!
            if (selectedCharacterChoice == choice)
            {
                selectedCharacterChoice.Deselect();
                selectedCharacterChoice = null; // No character is selected now
            }
            else
            {
                // Deselect the old one
                if (selectedCharacterChoice != null) selectedCharacterChoice.Deselect();
                
                // Select the new one
                selectedCharacterChoice = choice;
                selectedCharacterChoice.Select();
            }
        }

        public void OnConfirm() {
            long finalCharId = selectedCharacterChoice != null ? selectedCharacterChoice.CharacterId : -1;
            targetSlot.SetLinkedData(finalCharId, foundProfile);
            gameObject.SetActive(false);
        }
    }
}