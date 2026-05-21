using UnityEngine;
using Managers;

namespace UI
{
    public class LobbyNavigationUI : MonoBehaviour
    {
        public static LobbyNavigationUI Instance { get; private set; }
        
        [Header("Global UI")]
        [SerializeField] private GameObject barMenuPanel;
        [SerializeField] private GameObject HeadPanel;

        [Header("Views")]
        [SerializeField] private GameObject[] views; 

        private void Awake()
        {
            Instance = this;
            GameManager.OnStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState newState)
        {
            if (barMenuPanel != null) barMenuPanel.SetActive(newState == GameState.Menu);
            if (HeadPanel != null) HeadPanel.SetActive(newState == GameState.Menu);

            if (newState == GameState.Menu)
            {
                // --- THE INSTANT FIX ---
                // Silently start pre-loading leaderboard and category lists
                // We don't use 'await' here because we don't want to block the screen
                _ = SupabaseManager.Instance.PreloadLobbyData();
                // _ = SupabaseManager.Instance.RefreshLobbyCache();
                if (views.Length > 0) ShowViewByIndex(0);
            }
        }

        public void ShowViewByIndex(int index)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null) views[i].SetActive(i == index);
            }
        }
        
        
        
    }
}