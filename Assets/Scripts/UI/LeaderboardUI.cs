using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;

namespace UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Leaderboard Rows")]
        [SerializeField] private ResultRowUI[] rows; // Drag your 4 Row objects here
        [SerializeField] private float displayTime = 4.0f; 

        private void OnEnable()
        {
            StopAllCoroutines(); 

            // 1. Get the players and SORT by total score
            List<PlayerData> activePlayers = PlayerManager.Instance.Players;
            List<PlayerData> sortedPlayers = activePlayers.OrderByDescending(p => p.TotalScore).ToList();

            // 2. Assign data to rows based on RANK (1st, 2nd, 3rd, 4th)
            for (int i = 0; i < rows.Length; i++)
            {
                // Check if we actually have a player for this rank
                if (i < sortedPlayers.Count)
                {
                    // PLAYER EXISTS: Setup and show
                    rows[i].gameObject.SetActive(true); 
                    bool isFirstPlace = (i == 0); 
                    rows[i].Setup(sortedPlayers[i], isFirstPlace);
                }
                else
                {
                    // --- THE FIX: NO PLAYER FOR THIS RANK. HIDE THE BOX! ---
                    rows[i].gameObject.SetActive(false);
                }
            }

            // 3. Auto-skip logic
            if (GameManager.Instance.CurrentState == GameState.RoundResults)
            {
                StartCoroutine(FlashTimerRoutine());
            }
        }

        private IEnumerator FlashTimerRoutine()
        {
            yield return new WaitForSeconds(displayTime);
            GameManager.Instance.ChangeState(GameState.CategorySelection);
        }
    }
}