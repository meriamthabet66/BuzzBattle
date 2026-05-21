using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using RTLTMPro;

namespace UI
{
    public class ProfilePanelUI : MonoBehaviour
    {
        [Header("Profile Info")]
        [SerializeField] private RTLTextMeshPro usernameText; // Optional: if you show their name
        // [SerializeField] private Image characterImage; // To load the 2D Fox later

        [Header("Stats Displays")]
        [SerializeField] private TMP_Text totalScoreText;      // 390
        [SerializeField] private TMP_Text matchesPlayedText;   // 342
        [SerializeField] private TMP_Text tournamentWinsText;  // 18

        [Header("Steal Success Rate")]
        [SerializeField] private TMP_Text stealPercentageText; // 76%
        [SerializeField] private Image stealFillImage;         // The orange bar filling up
        [SerializeField] private Image stealFillLimit;     
        
        [Header("Logout Warning Popup")]
        [SerializeField] private GameObject logoutWarningPopup; // Drag your popup panel here
        [SerializeField] private Button confirmLogoutBtn;       // The "Yes, Logout" button
        [SerializeField] private Button cancelLogoutBtn;        // The "No, Go Back" button

        [Header("Controls")]
        [SerializeField] private Button logoutBtn;

        private void Start()
        {
            // The Main Logout button now runs a "Check" instead of just opening the popup
            if (logoutBtn != null) logoutBtn.onClick.AddListener(HandleLogoutRequest);

            // The buttons inside the warning
            if (confirmLogoutBtn != null) confirmLogoutBtn.onClick.AddListener(ExecuteFinalLogout);
            if (cancelLogoutBtn != null) cancelLogoutBtn.onClick.AddListener(() => ShowLogoutWarning(false));
        }
        // Update the OnEnable method in ProfilePanelUI.cs
        private void OnEnable()
        {
            // --- THE FIX: NO MORE ASYNC! ---
            // The LocalAccountManager is the Single Source of Truth for the UI.
            // It is always up to date, whether online or offline.
    
            // The PlayerManager gets its data from the LocalAccountManager at boot.
            RefreshProfileStats();
    
            GameManager.OnStateChanged += HandleStateChange;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }
        
        private void HandleStateChange(GameState state)
        {
            if (state == GameState.Menu) RefreshProfileStats();
        }

        private void RefreshProfileStats()
        {
            var host = LocalAccountManager.Instance?.SavedAccount;
            if (host == null) {
                totalScoreText.text = "0";
                matchesPlayedText.text = "0";
                return;
            }

            // 1. Basic Stats
            if (totalScoreText != null) totalScoreText.text = host.Score.ToString();
            if (matchesPlayedText != null) matchesPlayedText.text = host.MatchPlayedCount.ToString();
            if (tournamentWinsText != null) tournamentWinsText.text = host.TournamentWinCount.ToString();
            if (usernameText != null) usernameText.text = host.Username;
            
            
            PlayerManager.Instance.UpdateHostStars(host.Stars);

            // 2. Steal Percentage Math
            float stealRate = 0f;
            if (host.Steals > 0) 
            {
                stealRate = Mathf.Clamp01((float)host.CorrectSteals / (float)host.Steals); 
            }

            if (stealPercentageText != null) 
                stealPercentageText.text = Mathf.RoundToInt(stealRate * 100).ToString() + "%";

            // --- THE BULLETPROOF ANCHOR FIX ---
            // --- THE RIGHT-TO-LEFT ANCHOR FIX ---
            if (stealFillImage != null) 
            {
                RectTransform fillRect = stealFillImage.GetComponent<RectTransform>();
                
                // 1. AnchorMax.x is fixed at 1 (the far right edge)
                // 2. AnchorMin.x moves from 1.0 (empty) down to 0.0 (full)
                // If stealRate is 0.76, the bar starts at 0.24 and ends at 1.0
                float leftEdge = 1f - stealRate;

                fillRect.anchorMin = new Vector2(leftEdge, 0); // Bottom-Left starts at (1 - rate)
                fillRect.anchorMax = new Vector2(1, 1);        // Top-Right stays at 1
                
                // Reset offsets to ensure it sticks to the anchors
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }
        }
        
        private void HandleLogoutRequest()
        {
            // 1. Check if we are offline
            bool isOffline = Application.internetReachability == NetworkReachability.NotReachable;
            
            // 2. Check if we have unsynced progress (we'll add this property to LocalAccountManager)
            bool hasUnsyncedData = LocalAccountManager.Instance != null && LocalAccountManager.Instance.NeedsSync;

            if (isOffline || hasUnsyncedData)
            {
                // SHOW WARNING: User is offline or has data that hasn't reached the cloud yet
                Debug.Log("<color=orange>Logout Warning: Data at risk. Showing Popup.</color>");
                ShowLogoutWarning(true);
            }
            else
            {
                // SILENT LOGOUT: User is online and data is already synced. Safe to exit!
                Debug.Log("<color=green>Safe Logout: User is online and synced. Executing instantly.</color>");
                ExecuteFinalLogout();
            }
        }
        
        private void ShowLogoutWarning(bool show)
        {
            if (logoutWarningPopup != null) logoutWarningPopup.SetActive(show);
        }

        private void ExecuteFinalLogout()
        {
            Debug.Log("<color=red>User confirmed logout. Wiping local data...</color>");
            SupabaseManager.Instance.Logout();
        }

        private void OnLogoutClicked()
        {
            SupabaseManager.Instance.Logout();
        }
    }
}