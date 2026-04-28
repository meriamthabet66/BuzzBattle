<<<<<<< HEAD
﻿using Data.Data;
using TMPro;
using UnityEngine;

namespace UI {
    public class PlayerSetupPanelUI : MonoBehaviour
    {
        

        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; // Drag MatchConfigPanel here

        

        

        // Hook to your orange "Next/Continue" button
        public void OnClickNext()
        {
=======
﻿using Data.Data; // Ensure this matches your MatchSetupData location
using Managers;
using UnityEngine;

namespace UI 
{
    // You might want to rename your script file to this to avoid confusion!
    public class PlayerSetupPanelUI : MonoBehaviour 
    {
        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; // Drag MatchConfigPanel here

        [Header("The 4 Input Sections")]
        [Tooltip("Drag your 4 PlayerInputSectionUI GameObjects here IN ORDER (Player 1, 2, 3, 4)")]
        [SerializeField] private PlayerInputSectionUI[] inputSections; 

        private void OnEnable()
        {
            // 1. Look at the number the player chose on the previous screen
            int count = MatchSetupData.PlayerCount;

            // 2. Loop through all 4 inputs and show only the ones we need
            for (int i = 0; i < inputSections.Length; i++)
            {
                if (inputSections[i] != null)
                {
                    // If count is 3, indexes 0, 1, and 2 are active. Index 3 is INACTIVE.
                    inputSections[i].gameObject.SetActive(i < count);
                }
            }
        }

        // Hook this to your orange "Next/Continue" button
        public void OnClickNext()
        {
            // 1. Wipe the old players from the Manager
            PlayerManager.Instance.ResetPlayers();

            // 2. Loop from 0 up to the chosen player count (e.g., 0, 1, 2 for 3 players)
            for (int i = 0; i < MatchSetupData.PlayerCount; i++)
            {
                if (inputSections[i] != null)
                {
                    string playerName = inputSections[i].GetPlayerName();
                    int charId = inputSections[i].GetCharacterId();

                    // Add this player to the central list
                    PlayerManager.Instance.AddPlayer(playerName, charId);
                }
            }

            // 3. Go to the next screen in the menu flow
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}