using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        [SerializeField] private GameObject startingPanel; 
        [SerializeField] private GameObject categoryPanel; 
        
        [Header("Rematch Back-Button Route")]
        [Tooltip("Drag the panels here in the order of your flow: Home -> Mode -> Players -> Config")]
        [SerializeField] private GameObject[] rematchBackHistory; 
        
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            panelHistory.Clear();
            foreach (Transform child in transform) child.gameObject.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.CategorySelection)
            {
                // --- THE FULL HISTORY FIX ---
                // If it's a rematch, build the entire history stack!
                if (!Managers.MatchManager.Instance.IsMatchActive)
                {
                    // Push them in order, so the last one pushed is the first one they go back to.
                    foreach (GameObject panel in rematchBackHistory)
                    {
                        if (panel != null) panelHistory.Push(panel);
                    }
                }

                if (categoryPanel != null) OpenPanel(categoryPanel);
            }
            else if (startingPanel != null)
            {
                OpenPanel(startingPanel);
            }
        }

        public void OpenPanel(GameObject newPanel)
        {
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                currentPanel.SetActive(false);
            }
            currentPanel = newPanel;
            currentPanel.SetActive(true);
        }

        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                currentPanel.SetActive(false);
                currentPanel = panelHistory.Pop();
                currentPanel.SetActive(true);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }
    }
}