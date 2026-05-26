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

        // Inside LobbyNavigationUI.cs

        // --- THE FIX: Added 'async' here ---
        private async void HandleStateChange(GameState newState)
        {
            if (barMenuPanel != null) barMenuPanel.SetActive(newState == GameState.Menu);
            if (HeadPanel != null) HeadPanel.SetActive(newState == GameState.Menu);

            if (newState == GameState.Menu)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    Debug.Log("Refreshing profile from Cloud...");
                    
                    // Now that the method is 'async', this 'await' will work!
                    var freshProfile = await SupabaseManager.Instance.GetMyProfile();
                    
                    if (freshProfile != null)
                    {
                        LocalAccountManager.Instance.SaveProfileFromCloud(freshProfile);
                    }
                    
                    _ = SupabaseManager.Instance.PreloadLobbyData();
                }
                
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