using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        [SerializeField] private GameObject startingPanel; 
        [SerializeField] private GameObject categoryPanel; // --- NEW: Drag CategoryPanel here! ---
        
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake()
        {
            Instance = this;
        }

        // --- CHANGED: Use OnEnable instead of Start so it checks every round! ---
        private void OnEnable()
        {
            panelHistory.Clear();
            foreach (Transform child in transform) child.gameObject.SetActive(false);

            // 1. Are we mid-match doing Category Selection?
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.CategorySelection)
            {
                if (categoryPanel != null) OpenPanel(categoryPanel);
            }
            // 2. Otherwise, we must be in Menu or Setup. Open the Home Panel!
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
                if (currentPanel == categoryPanel) {
                    Debug.Log("can't go back in the middle of a match!");
                    
                    
                } else {
                    GameManager.Instance.ChangeState(GameState.Menu);
                }
                
            }
        }
    }
}