using System.Collections;
using System.Collections.Generic;
using System.Linq; // Needed for sorting!
using UnityEngine;
using Managers;

namespace UI
{
    public class RoundLeaderboardUI : MonoBehaviour
    {
        [Tooltip("Drag your 4 row GameObjects here")]
        [SerializeField] private ResultRowUI[] rows;
        
        [SerializeField] private float displayTime = 4.0f; // Shows for 4 seconds

        private void OnEnable()
        {
            // 1. Hide all rows initially
            foreach (var row in rows) row.gameObject.SetActive(false);

            // 2. Get the players from the Manager
            List<PlayerData> activePlayers = PlayerManager.Instance.Players;

            if (activePlayers.Count == 0) return;

            // 3. SORT the players by TotalScore (Highest to Lowest)
            List<PlayerData> sortedPlayers = activePlayers.OrderByDescending(p => p.TotalScore).ToList();

            // 4. Fill in the UI Rows
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                if (i < rows.Length)
                {
                    bool isFirstPlace = (i == 0); // i=0 is the player with the highest score!
                    rows[i].Setup(sortedPlayers[i], isFirstPlace);
                }
            }

            // 5. Start the timer to auto-skip!
            StartCoroutine(FlashTimerRoutine());
        }

        private IEnumerator FlashTimerRoutine()
        {
            yield return new WaitForSeconds(displayTime);

            // Time's up! Send them back to Category Selection for the next round!
            GameManager.Instance.ChangeState(GameState.CategorySelection);
        }
    }
}