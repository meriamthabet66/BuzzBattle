using System.Collections.Generic;
using UnityEngine;
using Data.Data; // To check MatchSetupData.Mode

namespace UI {
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        [Header("Menu Panels")]
        [SerializeField] private GameObject startingPanel; // HomePanel
        [SerializeField] private GameObject gameModePanel;
        [SerializeField] private GameObject playerSetupPanel;
        [SerializeField] private GameObject teamsSetupPanel;
        [SerializeField] private GameObject matchConfigPanel;
        [SerializeField] private GameObject categoryPanel; 
        
        private Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake() { Instance = this; }

        private void OnEnable()
        {
            panelHistory.Clear();
            foreach (Transform child in transform) child.gameObject.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.CategorySelection)
            {
                // --- THE DYNAMIC HISTORY FIX ---
                if (!Managers.MatchManager.Instance.IsMatchActive)
                {
                    // 1. Push Home
                    if (startingPanel != null) panelHistory.Push(startingPanel);
                    
                    // 2. Push Mode Selection
                    if (gameModePanel != null) panelHistory.Push(gameModePanel);
                    
                    // 3. BRANCH: Push the correct Setup Panel based on Mode
                    if (MatchSetupData.Mode == GameMode.Teams)
                    {
                        if (teamsSetupPanel != null) panelHistory.Push(teamsSetupPanel);
                    }
                    else
                    {
                        if (playerSetupPanel != null) panelHistory.Push(playerSetupPanel);
                    }
                    
                    // 4. Push Match Config (the screen immediately before Categories)
                    if (matchConfigPanel != null) panelHistory.Push(matchConfigPanel);
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

                if (currentPanel == startingPanel)
                {
                    GameManager.Instance.ChangeState(GameState.Menu);
                }
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }
    }
}