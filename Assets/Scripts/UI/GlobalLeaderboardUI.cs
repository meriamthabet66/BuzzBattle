using System.Collections.Generic;
using UnityEngine;
using Managers;
using Data.DTO;

namespace UI
{
    public class GlobalLeaderboardUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject rowPrefab;

        [Header("Offline Feedback")]
        [SerializeField] private GameObject offlineMessage; // Drag a Text object here
        [SerializeField] private GameObject rowsContainer;    // Drag the parent of the rows here

        private void OnEnable()
        {
            // 1. Listen for the "Data is ready" shout from SupabaseManager
            SupabaseManager.OnLobbyDataReady += RefreshLeaderboard;
            
            // 2. Initial attempt to show data
            RefreshLeaderboard();
        }

        private void OnDisable()
        {
            SupabaseManager.OnLobbyDataReady -= RefreshLeaderboard;
        }

        public void RefreshLeaderboard()
        {
            var data = SupabaseManager.Instance.CachedGlobalLeaderboard;

            // --- THE OFFLINE MESSAGE LOGIC ---
            if (data == null || data.Count == 0)
            {
                if (offlineMessage != null) offlineMessage.SetActive(true);
                if (rowsContainer != null) rowsContainer.SetActive(false);
                return;
            }

            // DATA FOUND: Hide message and show rows
            if (offlineMessage != null) offlineMessage.SetActive(false);
            if (rowsContainer != null) rowsContainer.SetActive(true);

            // Clear old rows and spawn new ones
            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            for (int i = 0; i < data.Count; i++)
            {
                GameObject newRow = Instantiate(rowPrefab, contentContainer);
                newRow.GetComponent<GlobalLeaderboardRowUI>().Setup(data[i], i + 1);
            }
        }
    }
}