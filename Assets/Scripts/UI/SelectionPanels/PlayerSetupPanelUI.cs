using Data.Data;
using Data.DTO;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels 
{
    public class PlayerSetupPanelUI : MonoBehaviour 
    {
        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; 

        [Header("The 4 Input Sections")]
        [SerializeField] private PlayerInputSectionUI[] inputSections; 

        [Header("Host Controls")]
        [SerializeField] private Button addHostButton; 

        private void Start()
        {
            if (addHostButton != null) addHostButton.onClick.AddListener(OnAddHostClicked);
        }

        private void OnEnable()
        {
            int count = MatchSetupData.PlayerCount;
            for (int i = 0; i < inputSections.Length; i++)
            {
                if (inputSections[i] != null) inputSections[i].gameObject.SetActive(i < count);
            }
            
            if (PlayerManager.Instance.Players.Count == 0)
            {
                foreach (var slot in inputSections) slot.ClearSlot();
            }
            
            RefreshHostButtonState();
        }
        
        

        public void RefreshHostButtonState()
        {
            // 1. Get the Host ID (Works offline or online!)
            string hostId = LocalAccountManager.Instance?.SavedAccount?.id;
            
            // 2. If no host is logged in (even offline), disable the button
            if (string.IsNullOrEmpty(hostId) || addHostButton == null) 
            {
                if (addHostButton != null) addHostButton.interactable = false;
                return;
            }
            
            // 3. Disable the button if the host is already in a slot
            addHostButton.interactable = !IsAccountAlreadyInSlot(hostId);
        }


        // --- NEW: Prevents Duplicate Accounts ---
        public bool IsAccountAlreadyInSlot(string accountId)
        {
            if (string.IsNullOrEmpty(accountId)) return false;

            for (int i = 0; i < MatchSetupData.PlayerCount; i++)
            {
                if (inputSections[i] != null && inputSections[i].LinkedProfile != null)
                {
                    if (inputSections[i].LinkedProfile.id == accountId) return true;
                }
            }
            return false;
        }

        private void OnAddHostClicked()
        {
            // 1. Get the SAVED account data from the Local Manager
            var hostAccount = LocalAccountManager.Instance?.SavedAccount;
            if (hostAccount == null) 
            {
                Debug.LogWarning("No Host account is loaded to add.");
                return;
            }

            // 2. We need a ProfileDTO to pass to the UI. We create a temporary one.
            ProfileDTO hostProfile = new ProfileDTO {
                id = hostAccount.id,
                email = hostAccount.email,
                username = hostAccount.Username,
                stars = hostAccount.Stars,
                score = hostAccount.Score,
                matches_played = hostAccount.MatchPlayedCount,
                match_wins = hostAccount.MatchWinCount,
                tournament_wins = hostAccount.TournamentWinCount,
                correct_steals = hostAccount.CorrectSteals,
                total_steals = hostAccount.Steals
            };

            // 3. Find the first empty slot and link the data
            for (int i = 0; i < MatchSetupData.PlayerCount; i++)
            {
                if (inputSections[i] != null && inputSections[i].LinkedProfile == null)
                {
                    // For now, we use a placeholder character ID
                    inputSections[i].SetLinkedData(-1, hostProfile);
                    RefreshHostButtonState(); // Disable the button
                    return;
                }
            }
        }

        public void OnClickNext()
        {
            PlayerManager.Instance.ResetPlayers();
            for (int i = 0; i < MatchSetupData.PlayerCount; i++)
            {
                if (inputSections[i] != null) inputSections[i].AddSelfToManager();
            }
            MenuController.Instance.OpenPanel(nextPanel);
        }
        
        
        public void ResetAllSlots()
        {
            foreach (var slot in inputSections)
            {
                if (slot != null) slot.ClearSlot();
            }
        }
    }
}