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

        public async void RefreshLeaderboard()
        {
            var data = SupabaseManager.Instance.CachedGlobalLeaderboard;

            // --- THE AUTO-FETCH FIX ---
            // If data is missing but we have internet, force a fetch!
            if ((data == null || data.Count == 0) && Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("Leaderboard: Data missing while online. Requesting fresh fetch...");
                await SupabaseManager.Instance.PreloadLobbyData();
                data = SupabaseManager.Instance.CachedGlobalLeaderboard; // Update our local reference
            }

            if (data == null || data.Count == 0)
            {
                if (offlineMessage != null) offlineMessage.SetActive(true);
                if (rowsContainer != null) rowsContainer.SetActive(false);
                return;
            }

            // SUCCESS: Show the data
            if (offlineMessage != null) offlineMessage.SetActive(false);
            if (rowsContainer != null) rowsContainer.SetActive(true);

            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            for (int i = 0; i < data.Count; i++)
            {
                GameObject newRow = Instantiate(rowPrefab, contentContainer);
                newRow.GetComponent<GlobalLeaderboardRowUI>().Setup(data[i], i + 1);
            }
        }
    }
}