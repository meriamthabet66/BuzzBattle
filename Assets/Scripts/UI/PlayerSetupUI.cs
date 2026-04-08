using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Managers;
using UI.UI;

namespace UI {
    public class PlayerSetupUI : MonoBehaviour {
        [Header("Player Inputs")]
        
        [SerializeField] private List<PlayerInputSectionUI> playerSections;

        private void Awake()
        {
            for (int i = 0; i < playerSections.Count; i++)
            {
                playerSections[i].playerIndex = i + 1;
            }
        }


        public void OnStartClicked() {
            PlayerManager.Instance.ResetPlayers();

            string p1 = playerSections[0].GetPlayerName();
            string p2 = playerSections[1].GetPlayerName();

            int c1 = playerSections[0].GetCharacterId();
            int c2 = playerSections[1].GetCharacterId();

            PlayerManager.Instance.AddPlayer(p1, c1);
            PlayerManager.Instance.AddPlayer(p2, c2);

            if (!PlayerManager.Instance.ValidateMinPlayers()) {
                Debug.LogWarning("Not enough players!");
                return;
            }

            GameManager.Instance.ChangeState(GameState.CategorySelection);
        }
    }
}