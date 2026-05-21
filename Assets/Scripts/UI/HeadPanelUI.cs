using UnityEngine;
using TMPro;
using Managers;

namespace UI
{
    public class HeadPanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text starsValueText;

        private void OnEnable()
        {
            // 1. Listen for the update event from PlayerManager
            // (Note: Use whichever event name you have in PlayerManager, 
            // e.g., OnHostStarsUpdated or OnHostAccountUpdated)
            PlayerManager.OnHostStarsUpdated += UpdateStarsUI;
            
            // 2. THE FIX: Force an update immediately when this panel wakes up!
            // This prevents the "000" placeholder from showing during the boot sequence.
            UpdateStarsUI();
        }

        private void OnDisable()
        {
            PlayerManager.OnHostStarsUpdated -= UpdateStarsUI;
        }

        public void UpdateStarsUI()
        {
            if (starsValueText == null) return;

            // --- THE LOCAL-FIRST LOGIC ---
            // We check the LocalAccountManager first because it's the "Source of Truth" 
            // and it loads instantly from the phone's storage.
            if (LocalAccountManager.Instance != null && LocalAccountManager.Instance.SavedAccount != null)
            {
                starsValueText.text = LocalAccountManager.Instance.SavedAccount.Stars.ToString();
            }
            // Fallback to PlayerManager if for some reason the local manager isn't ready
            else if (PlayerManager.Instance != null && PlayerManager.Instance.HostAccount != null)
            {
                starsValueText.text = PlayerManager.Instance.HostAccount.Stars.ToString();
            }
            else
            {
                // If everything is null (logged out), show 0
                starsValueText.text = "0";
            }
        }
    }
}