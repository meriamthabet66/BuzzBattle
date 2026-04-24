using Data.Data;
using TMPro;
using UnityEngine;

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
        
        private GameMode localSelectedMode = GameMode.Normal;

        private void OnEnable()
        {
            UpdatePlayerCountUI();
            UpdateModeVisuals(); // Ensure the outlines are correct the moment the panel opens!
        }

        // --- GAME MODE LOGIC ---

        public void SelectNormalMode()
        {
            localSelectedMode = GameMode.Normal;
            UpdateModeVisuals();
            Debug.Log("Normal Mode Selected");
        }

        public void SelectTeamMode()
        {
            localSelectedMode = GameMode.Teams;
            UpdateModeVisuals();
            Debug.Log("Teams Mode Selected");
        }

        public void SelectTournamentMode()
        {
            localSelectedMode = GameMode.Tournament;
            UpdateModeVisuals();
            Debug.Log("Tournament Mode Selected");
        }

        // Helper method to keep code perfectly clean and avoid copy-pasting
        private void UpdateModeVisuals()
        {
            // SetActive evaluates the condition. If localSelectedMode is Normal, it returns true!
            if (NrmlBtn != null && NrmlBtn.SelectedOutline != null) 
                NrmlBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Normal);
            
            if (TeamsBtn != null && TeamsBtn.SelectedOutline != null) 
                TeamsBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Teams);
            
            if (TournBtn != null && TournBtn.SelectedOutline != null) 
                TournBtn.SelectedOutline.SetActive(localSelectedMode == GameMode.Tournament);
        }


        // --- PLAYER COUNT LOGIC ---

        public void IncreasePlayers()
        {
            // Mathf.Clamp keeps the number strictly between 2 and 4 in one clean line
            localPlayerCount = Mathf.Clamp(localPlayerCount + 1, 2, 4);
            UpdatePlayerCountUI();
        }

        public void DecreasePlayers()
        {
            localPlayerCount = Mathf.Clamp(localPlayerCount - 1, 2, 4);
            UpdatePlayerCountUI();
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