using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class BarMenuHandler : MonoBehaviour
    {
        [Header("Navigation Link")]
        [SerializeField] private LobbyNavigationUI navigationUI;

        [Header("Buttons")]
        [SerializeField] private Button homeBtn;
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button leaderboardBtn;
        [SerializeField] private Button accountBtn;

        private List<BarButtonUI> allBarButtons = new List<BarButtonUI>();
        

        private void Start()
        {
            allBarButtons.Add(homeBtn.GetComponent<BarButtonUI>());
            allBarButtons.Add(shopBtn.GetComponent<BarButtonUI>());
            allBarButtons.Add(leaderboardBtn.GetComponent<BarButtonUI>());
            allBarButtons.Add(accountBtn.GetComponent<BarButtonUI>());

            homeBtn.onClick.AddListener(() => OnTabClicked(0));
            shopBtn.onClick.AddListener(() => OnTabClicked(1));
            leaderboardBtn.onClick.AddListener(() => OnTabClicked(2));
            accountBtn.onClick.AddListener(() => OnTabClicked(3));

            OnTabClicked(0); 
        }

        public void OnTabClicked(int index)
        {
            for (int i = 0; i < allBarButtons.Count; i++)
            {
                if (allBarButtons[i] != null)
                    allBarButtons[i].SetState(i == index);
            }

            if (navigationUI != null)
                navigationUI.ShowViewByIndex(index);
        }
    }
}