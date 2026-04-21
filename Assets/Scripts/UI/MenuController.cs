using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        [SerializeField] private GameObject startingPanel; // e.g., HomePanel
        
        // This remembers our history! 
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 1. Force ALL panels inside this canvas to turn OFF immediately
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            // 2. Turn ON only the starting panel (e.g. HomePanel)
            if (startingPanel != null)
            {
                OpenPanel(startingPanel);
            }
        }

        public void OpenPanel(GameObject newPanel)
        {
            // Turn off current panel and save it to history
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                currentPanel.SetActive(false);
            }

            // Turn on the new panel
            currentPanel = newPanel;
            currentPanel.SetActive(true);
        }

        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                // Turn off current
                currentPanel.SetActive(false);

                // Pop the last panel from history and turn it on
                currentPanel = panelHistory.Pop();
                currentPanel.SetActive(true);
            }
            else
            {
                Debug.Log("No history left! Returning to Main Menu state.");
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }
    }
}