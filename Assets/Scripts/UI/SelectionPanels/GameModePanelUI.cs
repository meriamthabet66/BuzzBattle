using Data.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

namespace UI 
{
    public class GameModePanelUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private GameObject nextPanel; 

        [Header("Player Count UI")]
        [SerializeField] private TMP_Text playerCountText; 
        private int localPlayerCount = 4; // Default to 4
        
        [Header("Game Mode UI Buttons")]
        [SerializeField] private ModeBtn NrmlBtn;
        [SerializeField] private ModeBtn TeamsBtn;
        [SerializeField] private ModeBtn TournBtn;
        
        [Header("Tournament Button Reference")]
        [SerializeField] private Button tournamentButton; 
        [SerializeField] private CanvasGroup tournamentCanvasGroup; // --- NEW: Used to fade the button! ---

        private GameMode localSelectedMode = GameMode.Normal;

        private void OnEnable()
        {
            UpdatePlayerCountUI();
            CheckTournamentEligibility(); // Always check rules first
            UpdateModeVisuals(); 
        }

        // --- GAME MODE LOGIC ---

        public void SelectNormalMode()
        {
            localSelectedMode = GameMode.Normal;
            UpdateModeVisuals();
        }

        public void SelectTeamMode()
        {
            localSelectedMode = GameMode.Teams;
            UpdateModeVisuals();
        }

        public void SelectTournamentMode()
        {
            if (localPlayerCount < 3) return; // Cannot select if less than 3 players!

            localSelectedMode = GameMode.Tournament;
            UpdateModeVisuals();
        }

        private void UpdateModeVisuals()
        {
            if (NrmlBtn != null && NrmlBtn.SelectedOutline != null) 
                NrmlBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Normal);
            
            if (TeamsBtn != null && TeamsBtn.SelectedOutline != null) 
                TeamsBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Teams);
            
            if (TournBtn != null && TournBtn.SelectedOutline != null) 
                TournBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Tournament);
        }

        private void CheckTournamentEligibility()
        {
            if (tournamentButton != null && tournamentCanvasGroup != null)
            {
                bool isAllowed = localPlayerCount >= 3;
                tournamentButton.interactable = isAllowed;
                
                // --- NEW: Fade the button to 50% opacity if not allowed! ---
                tournamentCanvasGroup.alpha = isAllowed ? 1.0f : 0.5f;

                // --- THE HACK PREVENTION ---
                // If they previously selected Tournament, but then clicked the [-] button 
                // to drop to 2 players, we FORCE them back to Normal mode immediately!
                if (!isAllowed && localSelectedMode == GameMode.Tournament)
                {
                    Debug.Log("Tournament no longer allowed for 2 players. Forcing Normal Mode.");
                    SelectNormalMode();
                }
            }
        }

        // --- PLAYER COUNT LOGIC ---

        public void IncreasePlayers()
        {
            localPlayerCount = Mathf.Clamp(localPlayerCount + 1, 2, 4);
            UpdatePlayerCountUI();
            CheckTournamentEligibility(); // Verify rules after changing number!
        }

        public void DecreasePlayers()
        {
            localPlayerCount = Mathf.Clamp(localPlayerCount - 1, 2, 4);
            UpdatePlayerCountUI();
            CheckTournamentEligibility(); // Verify rules after changing number!
        }

        private void UpdatePlayerCountUI()
        {
            if (playerCountText != null)
                playerCountText.text = localPlayerCount.ToString();
        }

        // --- NAVIGATION ---
        
        public void OnClickNext()
        {
            MatchSetupData.PlayerCount = localPlayerCount;
            MatchSetupData.Mode = localSelectedMode;
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}