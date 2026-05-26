using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;
using Data.Data;

namespace Managers 
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public GameMode currentMode;
        public int totalRounds;
        public int questionsPerRound;

        public int CurrentRoundIndex { get; private set; }
        
        public bool IsMatchActive { get; private set; } 
        
        [SerializeField] private RoundManager roundManager;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            IsMatchActive = false; 
        }

        public void StartMatch(GameMode mode, int rounds, int questions)
        {
            currentMode = mode;
            questionsPerRound = questions;
            CurrentRoundIndex = 0;
            IsMatchActive = true; 

            // --- THE TOURNAMENT FIX: Override the rounds! ---
            if (mode == GameMode.Tournament)
            {
                // If 4 players = 3 rounds. If 3 players = 2 rounds.
                totalRounds = PlayerManager.Instance.Players.Count - 1;
            }
            else
            {
                totalRounds = rounds;
            }

            if (roundManager != null) roundManager.ClearQuestionMemory();

            Debug.Log($"Match started | Mode: {mode} | Rounds: {totalRounds}");
        }
        
        public void StartRound(List<Category> categories, QuestionType type)
        {
            if (roundManager == null) return;
            roundManager.InitializeRound(categories, type);
        }

        public bool IsLastRound()
        {
            return CurrentRoundIndex >= totalRounds - 1;
        }

     public async void OnRoundFinished()
        {
            if (currentMode == GameMode.Tournament && !IsLastRound())
            {
                PlayerManager.Instance.EliminateLowestScoringPlayer();
            }

            if (!IsLastRound())
            {
                CurrentRoundIndex++;
                GameManager.Instance.ChangeState(GameState.RoundResults); 
            }
            else
            {
                IsMatchActive = false; 
                Debug.Log("Match Over! Calculating rewards...");

                int highestScore = -1;
                foreach (var p in PlayerManager.Instance.Players)
                    if (p.TotalScore > highestScore) highestScore = p.TotalScore;

                foreach (var player in PlayerManager.Instance.Players)
    {
        // 1. Calculate Rewards (Participation + Win + Steals)
        int starsEarned = Core.Enums.StarRules.ParticipationReward;
        bool isWinner = (player.TotalScore == highestScore);
        bool wonTournament = isWinner && (currentMode == GameMode.Tournament);

        if (wonTournament) starsEarned += Core.Enums.StarRules.TournamentWinReward;
        else if (isWinner) starsEarned += Core.Enums.StarRules.NormalMatchWinReward;
        
        starsEarned += (player.CorrectStealsInMatch * Core.Enums.StarRules.CorrectStealBonus);

        // 2. CHECK FOR LINKED ACCOUNTS
        if (player.LinkedAccount != null)
        {
            // CASE A: The player is the HOST of this device
            if (LocalAccountManager.Instance.SavedAccount != null && 
                player.LinkedAccount.id == LocalAccountManager.Instance.SavedAccount.id)
            {
                Debug.Log($"Saving Host Stats: {player.DisplayName}");
                LocalAccountManager.Instance.AddMatchStats(
                    starsEarned, player.TotalScore, 1, isWinner ? 1 : 0, 
                    wonTournament ? 1 : 0, player.CorrectStealsInMatch, player.Steals
                );
            }
            // CASE B: The player is a GUEST ACCOUNT (Friend playing on Host's phone)
            else 
            {
                // We cannot save their data locally (it's not their phone), 
                // so we push it directly to the Cloud!
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    Debug.Log($"Pushing Guest Account stats to Cloud for: {player.DisplayName}");
                    
                    // We calculate the NEW totals based on their current cloud snapshot + match gains
                    int nextStars = player.LinkedAccount.Stars + starsEarned;
                    int nextScore = player.LinkedAccount.Score + player.TotalScore;
                    int nextMatches = player.LinkedAccount.MatchPlayedCount + 1;
                    int nextWins = isWinner ? player.LinkedAccount.MatchWinCount + 1 : player.LinkedAccount.MatchWinCount;

                    // Call the RPC directly for this specific player ID
                    _ = SupabaseManager.Instance.SaveMatchResults(
                        player.LinkedAccount.id, nextStars, nextScore, nextMatches, nextWins, 
                        (wonTournament ? 1 : 0), player.CorrectStealsInMatch, player.Steals
                    );
                }
            }
        }
    }

                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
        
        // --- NEW: Safely aborts a match if the user hits "Play Again" or "Home" early ---
        // --- UPDATED: Safely aborts a match and resets the round index ---
        public void CancelMatch()
        {
            IsMatchActive = false;
            CurrentRoundIndex = 0; // Force it back to Round 1!
        }
    }
}