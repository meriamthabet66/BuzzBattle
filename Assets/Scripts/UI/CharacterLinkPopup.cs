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
        
        

        public void OpenForSlot(PlayerInputSectionUI slot) 
        {
            this.targetSlot = slot;
            this.foundProfile = null;
            this.selectedCharacterChoice = null;

            // --- THE FIX: WIPE THE UI CLEAN ---
            if (emailInput != null) emailInput.text = "";
            if (searchStatusText != null) searchStatusText.text = "";
            foreach (Transform child in characterGridContent) Destroy(child.gameObject);
            
            linkAccountToggle.isOn = false;
            OnToggleChanged(false);

            gameObject.SetActive(true);
        }

        private async void OnToggleChanged(bool isAccountLinkActive) 
        {
            
            linkAccountToggleImage.sprite = isAccountLinkActive ? Toggled : NotToggled;
            accountSearchSection.SetActive(isAccountLinkActive);
            
            // --- THE FIX: Clear characters when toggling back to Guest ---
            if (!isAccountLinkActive) 
            {
                foundProfile = null;
                if (searchStatusText != null) searchStatusText.text = "";
                if (emailInput != null) emailInput.text = "";
                
                foreach (Transform child in characterGridContent) Destroy(child.gameObject);
                
                // Optional: You can spawn default "Guest" foxes here later!
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

        private void PopulateCharacterList(List<CharacterData> characters) 
        {
            // 1. Delete the "Dummy" display characters
            foreach (Transform child in characterGridContent) 
            {
                Destroy(child.gameObject);
            }
        
            selectedCharacterChoice = null;

            // 2. If no characters exist in the database, do nothing (or spawn a default one)
            if (characters == null || characters.Count == 0) 
            {
                Debug.Log("No saved characters found for this account.");
                return;
            }
        
            // 3. Spawn the REAL characters from the database
            foreach (var characterData in characters) 
            {
                GameObject newObj = Instantiate(characterItemPrefab, characterGridContent);
                var choiceUI = newObj.GetComponent<CharacterChoiceUI>();
            
                if (choiceUI != null) 
                {
                    // Send the database data to the prefab
                    choiceUI.Setup(characterData, this);
                }

                // Auto-select the first one in the list so the player doesn't have to
                if (selectedCharacterChoice == null) 
                {
                    OnCharacterSelected(choiceUI);
                }
            }
        }

        public void OnCharacterSelected(CharacterChoiceUI choice) {
            if (selectedCharacterChoice != null) selectedCharacterChoice.Deselect();
            selectedCharacterChoice = choice;
            selectedCharacterChoice.Select();
        }

        public void OnConfirm() {
            long finalCharId = selectedCharacterChoice != null ? selectedCharacterChoice.CharacterId : -1;
            targetSlot.SetLinkedData(finalCharId, foundProfile);
            gameObject.SetActive(false);
        }
    }
}