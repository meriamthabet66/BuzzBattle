using GamePlay.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
   

    public class PlayerUI : MonoBehaviour
    {
        public int playerIndex;
        public Button buzzerButton;

        private void Start()
        {
            buzzerButton.onClick.AddListener(OnBuzz);
        }

        public void OnBuzz()
        {
            Debug.Log("Player buzzed: " + playerIndex);
            BuzzerSystem.OnPlayerBuzzed?.Invoke(playerIndex);
        }
    }
}