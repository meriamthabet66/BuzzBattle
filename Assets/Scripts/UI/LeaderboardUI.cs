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
        [SerializeField] private ResultRowUI[] rows;
        [SerializeField] private float displayTime = 4.0f; 

        private void OnEnable()
        {
            StopAllCoroutines(); 
            foreach (var row in rows) if (row != null) row.gameObject.SetActive(false);

            List<PlayerData> activePlayers = PlayerManager.Instance.Players;
            if (activePlayers.Count > 0)
            {
                List<PlayerData> sortedPlayers = activePlayers.OrderByDescending(p => p.TotalScore).ToList();
                for (int i = 0; i < sortedPlayers.Count; i++)
                {
                    if (i < rows.Length && rows[i] != null)
                    {
                        bool isFirstPlace = (i == 0); 
                        rows[i].Setup(sortedPlayers[i], isFirstPlace);
                    }
                }
            }

            // Auto-skip ONLY if we are mid-game
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