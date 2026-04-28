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
        
        // --- NEW: Tracks if we are mid-game! ---
        public bool IsMatchActive { get; private set; } 
        
        [SerializeField] private RoundManager roundManager;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            IsMatchActive = false; // Match hasn't started yet
        }

        public void StartMatch(GameMode mode, int rounds, int questions)
        {
            currentMode = mode;
            totalRounds = rounds;
            questionsPerRound = questions;
            CurrentRoundIndex = 0;
            IsMatchActive = true; 

            // --- NEW: Tell RoundManager to clear the question memory! ---
            if (roundManager != null) roundManager.ClearQuestionMemory();

            Debug.Log($"Match started | Mode: {mode} | Rounds: {rounds}");
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

        // --- UPDATED: Stop auto-starting the round! ---
        public void OnRoundFinished()
        {
            if (!IsLastRound())
            {
                CurrentRoundIndex++;
                Debug.Log($"Round finished! Showing leaderboard before Round {CurrentRoundIndex + 1}");
                
                // --- CHANGED: Go to the new RoundResults state first! ---
                GameManager.Instance.ChangeState(GameState.RoundResults); 
                // Note: You must add `RoundResults` to your GameState enum in GameManager.cs!
            }
            else
            {
                IsMatchActive = false; 
                Debug.Log("Match Over! Going to Final Results.");
                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
    }
}