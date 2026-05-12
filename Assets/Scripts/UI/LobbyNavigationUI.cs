using UnityEngine;

namespace UI
{
    public class LobbyNavigationUI : MonoBehaviour
    {
        [Header("Global UI")]
        [SerializeField] private GameObject barMenuPanel; // Drag your BarMenuPanel here!

        [Header("Views")]
        [SerializeField] private GameObject[] views; 

        private void OnEnable()
        {
            // Subscribe to the state change
            GameManager.OnStateChanged += HandleStateChange;
            
            // Check current state immediately
            if (GameManager.Instance != null)
                HandleStateChange(GameManager.Instance.CurrentState);
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState newState)
        {
            // The Bar only appears in the main 'Menu' state (Lobby)
            if (barMenuPanel != null)
            {
                barMenuPanel.SetActive(newState == GameState.Menu);
            }
        }

        public void ShowViewByIndex(int index)
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] != null)
                {
                    views[i].SetActive(i == index);
                }
            }
        }
    }
}