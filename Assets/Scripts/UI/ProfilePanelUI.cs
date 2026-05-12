using UnityEngine;
using TMPro;
using Managers;
using UnityEngine.UI;

namespace UI
{
    public class ProfilePanelUI : MonoBehaviour
    {
        [Header("Stat Texts")]
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text matchesPlayedText;
        [SerializeField] private TMP_Text totalPointsText;
        [SerializeField] private TMP_Text tournamentWinsText;
        
        [Header("Steal Success Rate")]
        [SerializeField] private TMP_Text stealPercentText;
        [SerializeField] private Slider stealProgressBar; // The orange bar from Figma

        [Header("Buttons")]
        [SerializeField] private Button logoutBtn;

        private void OnEnable()
        {
            // Refresh whenever the panel is turned on
            // FillProfileData();
            
            // AND listen for when the GameManager says we are in Menu mode
            GameManager.OnStateChanged += HandleStateChange;
        }
        
        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState newState)
        {
            if (newState == GameState.Menu)
            {
                // Force a data refresh when we enter the menu
                // FillProfileData();
            }
        }

        private void Start()
        {
            if (logoutBtn != null) logoutBtn.onClick.AddListener(OnLogoutClicked);
        }

        // private void FillProfileData()
        // {
        //     var account = PlayerManager.Instance.HostAccount;
        //     if (account == null) return;
        //
        //     usernameText.text = ArabicFixer.Fix(account.Username);
        //     matchesPlayedText.text = account.MatchPlayedCount.ToString();
        //     totalPointsText.text = account.Score.ToString();
        //     tournamentWinsText.text = account.TournamentWinCount.ToString();
        //
        //     // Calculate Steal Success %
        //     if (account.Steals > 0)
        //     {
        //         float rate = ((float)account.CorrectSteals / (float)account.Steals);
        //         stealPercentText.text = Mathf.RoundToInt(rate * 100) + "%";
        //         if (stealProgressBar != null) stealProgressBar.value = rate;
        //     }
        //     else
        //     {
        //         stealPercentText.text = "0%";
        //         if (stealProgressBar != null) stealProgressBar.value = 0;
        //     }
        // }

        private void OnLogoutClicked()
        {
            Debug.Log("Logging out...");
            SupabaseManager.Instance.Logout();
        }
    }
}