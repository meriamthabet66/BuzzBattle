using UnityEngine;
using TMPro;
using Managers;
using Data.Data;
using RTLTMPro;

namespace UI.SelectionPanels
{
    public class TeamsSetupPanelUI : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField teamANameInput;
        [SerializeField] private TMP_InputField teamBNameInput;
        [SerializeField] private RTLTextMeshPro teamANameDisplay;
        [SerializeField] private RTLTextMeshPro teamBNameDisplay;

        [Header("Navigation")]
        [SerializeField] private GameObject nextPanel; // Usually MatchConfigPanel
        
        
        
        public void ResetFields()
        {
            if (teamANameInput != null) teamANameInput.text = "";
            if (teamBNameInput != null) teamBNameInput.text = "";
            if (teamANameDisplay != null) teamANameDisplay.text = "";
            if (teamBNameDisplay != null) teamBNameDisplay.text = "";
            Debug.Log("<color=red>Teams Panel: Fields wiped.</color>");
        }


        public void OnClickConfirm()
        {
            // 1. Get Names (fallback to default if empty)
            string nameA = string.IsNullOrWhiteSpace(teamANameInput.text) ? "الفريق أ" : teamANameInput.text;
            string nameB = string.IsNullOrWhiteSpace(teamBNameInput.text) ? "الفريق ب" : teamBNameInput.text;

            // 2. Wipe old players
            PlayerManager.Instance.ResetPlayers();

            // 3. Register Teams as "Players" so the game works normally
            // Slot 0 (Player 1) = Team A
            // Slot 1 (Player 2) = Team B
            PlayerManager.Instance.AddPlayer(nameA, 0); // Default character ID 0
            PlayerManager.Instance.AddPlayer(nameB, 0); 

            // 4. Move to Match Configuration
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}