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

        public void OnRoundFinished()
        {
            // --- THE TOURNAMENT FIX: Eliminate the loser! ---
            if (currentMode == GameMode.Tournament && !IsLastRound())
            {
                PlayerManager.Instance.EliminateLowestScoringPlayer();
            }

            if (!IsLastRound())
            {
                CurrentRoundIndex++;
                Debug.Log($"Round finished! Showing leaderboard before Round {CurrentRoundIndex + 1}");
                
                GameManager.Instance.ChangeState(GameState.RoundResults); 
            }
            else
            {
                IsMatchActive = false; 
                Debug.Log("Match Over! Going to Final Results.");
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