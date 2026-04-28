using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

<<<<<<< HEAD
        [SerializeField] private GameObject startingPanel; // e.g., HomePanel
        
        // This remembers our history! 
=======
        [SerializeField] private GameObject startingPanel; 
        [SerializeField] private GameObject categoryPanel; // --- NEW: Drag CategoryPanel here! ---
        
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake()
        {
            Instance = this;
        }

<<<<<<< HEAD
        private void Start()
        {
            // 1. Force ALL panels inside this canvas to turn OFF immediately
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            // 2. Turn ON only the starting panel (e.g. HomePanel)
            if (startingPanel != null)
=======
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
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            {
                OpenPanel(startingPanel);
            }
        }

        public void OpenPanel(GameObject newPanel)
        {
<<<<<<< HEAD
            // Turn off current panel and save it to history
=======
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                currentPanel.SetActive(false);
            }
<<<<<<< HEAD

            // Turn on the new panel
=======
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            currentPanel = newPanel;
            currentPanel.SetActive(true);
        }

        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
<<<<<<< HEAD
                // Turn off current
                currentPanel.SetActive(false);

                // Pop the last panel from history and turn it on
=======
                currentPanel.SetActive(false);
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
                currentPanel = panelHistory.Pop();
                currentPanel.SetActive(true);
            }
            else
            {
<<<<<<< HEAD
                Debug.Log("No history left! Returning to Main Menu state.");
                GameManager.Instance.ChangeState(GameState.Menu);
=======
                if (currentPanel == categoryPanel) {
                    Debug.Log("can't go back in the middle of a match!");
                    
                    
                } else {
                    GameManager.Instance.ChangeState(GameState.Menu);
                }
                
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
            }
        }
    }
}