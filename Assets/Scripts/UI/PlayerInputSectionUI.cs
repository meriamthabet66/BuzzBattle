using Data.DTO;
using Managers;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI 
{
    public class PlayerInputSectionUI : MonoBehaviour
    {
        [Header("Input")]
        public TMP_InputField nameInput;

        [Header("Display (RTL Fixed)")]
        public RTLTextMeshPro nameDisplayText; // RTLInputDisplay handles this visually!

        public Button characterButton;
        public int playerIndex; 
        private int selectedCharacterId = 0;
        
        [Header("Account Link")]
        [SerializeField] private Button searchButton;
        [SerializeField] private GameObject accountLinkedIcon; // Optional: A checkmark icon
        private ProfileDTO linkedProfile; // Stores the found account

        public string GetPlayerName()
        {
            // --- THE FIX: Return RAW text! ---
            // Do NOT use ArabicFixer here anymore, because the Gameplay Buzzer is using RTLTMPro!
            if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
            {
                return nameInput.text; // Send raw text
            }

            // Default raw Arabic name
            return $"لاعب {playerIndex}";
        }

        public int GetCharacterId()
        {
            return selectedCharacterId;
        }

        public void SetCharacter(int characterId)
        {
            selectedCharacterId = characterId;
        }
        
        private void Start() 
        {
            if (searchButton != null) 
                searchButton.onClick.AddListener(OnSearchClicked);
        }

        private async void OnSearchClicked()
        {
            string emailToSearch = nameInput.text; // The email they typed
            if (string.IsNullOrEmpty(emailToSearch)) return;

            searchButton.interactable = false;
    
            ProfileDTO found = await SupabaseManager.Instance.SearchPlayerByEmail(emailToSearch);
    
            if (found != null)
            {
                linkedProfile = found;
                // 1. Instantly change the display name to their REAL username
                nameDisplayText.text = found.username; 
        
                // 2. Visually show they are logged in
                if (accountLinkedIcon != null) accountLinkedIcon.SetActive(true);
        
                Debug.Log($"{found.username} is ready to play on Buzzer {playerIndex}!");
            }
            else
            {
                Debug.Log("No account found. They will play as a Guest.");
                linkedProfile = null;
                if (accountLinkedIcon != null) accountLinkedIcon.SetActive(false);
            }

            searchButton.interactable = true;
        }

// Modify your existing GetPlayerProfile logic to return the linked data
        public ProfileDTO GetLinkedProfile() => linkedProfile;
    }
}