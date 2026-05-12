using System.Collections.Generic;
using UnityEngine;
using System;
using Data;
using Data.DTO;

namespace Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        
        // --- NEW: This stores the logged-in Host's data ---
        public AccountData HostAccount { get; private set; }

        public List<PlayerData> Players = new List<PlayerData>();

        // 🔥 Event for UI (HUD updates later)
        public static Action<List<PlayerData>> OnPlayersUpdated;
        
        public static Action<int, int> OnPlayerScoreUpdated; // Sends: ListIndex, NewTotalScore

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
        }

        
        
        
        // =========================
        // ACCOUNT MANAGEMENT
        // =========================

        // --- NEW: This takes the database DTO and turns it into Game Data ---
        public void SetMainAccount(ProfileDTO dto)
        {
            HostAccount = new AccountData
            {
                Username = dto.username,
                Stars = dto.stars,
                Score = dto.score,
                Steals = dto.total_steals,
                MatchWinCount = dto.match_wins,
                TournamentWinCount = dto.tournament_wins
                // Link characters if needed later...
            };

            Debug.Log($"<color=orange>Host Account set: {HostAccount.Username}. Stars: {HostAccount.Stars}</color>");
        }
        
        public void ClearHostAccount()
        {
            HostAccount = null;
        }

        // =========================
        // PLAYER MANAGEMENT
        // =========================

        public void AddPlayer(string name, int selectedCharacterId, AccountData account = null)
        {
            PlayerData player = new PlayerData
            {
                ID = Players.Count,
                DisplayName = name,
                TotalScore = 0,
                RoundScore =0,
                Steals = 0,
                SelectedCharacterID = selectedCharacterId,
                LinkedAccount = account
            };

            Players.Add(player);

            Debug.Log($"Player added: {name}");

            OnPlayersUpdated?.Invoke(Players);
        }
        
        
        // Helper to convert DTO to AccountData
        private AccountData ConvertDTOToAccount(ProfileDTO dto)
        {
            return new AccountData {
                Username = dto.username,
                Stars = dto.stars,
                Score = dto.score
            };
        }

        public bool ValidateMinPlayers(int min = 2)
        {
            return Players.Count >= min;
        }

        public void ResetPlayers()
        {
            Players.Clear();
            OnPlayersUpdated?.Invoke(Players);
        }

        public PlayerData GetPlayer(int index)
        {
            if (index < 0 || index >= Players.Count)
                return null;

            return Players[index];
        }
        
        public void AddScore(int index, int pointsToAdd)
        {
            if (index < 0 || index >= Players.Count) return;

            Players[index].RoundScore += pointsToAdd;
            Players[index].TotalScore += pointsToAdd;

            // Tell the Gameplay HUD to update the number on their buzzer (showing total score)
            OnPlayerScoreUpdated?.Invoke(index, Players[index].TotalScore);
        }

        // Add this method to clear the round score when a new round starts!
        public void ResetRoundScores()
        {
            foreach (var player in Players)
            {
                player.RoundScore = 0;
            }
        }
        
        
        
        // --- ADD THIS METHOD ANYWHERE INSIDE PlayerManager.cs ---
        public void ResetScoresForRematch()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].TotalScore = 0;
                Players[i].RoundScore = 0;
                
                // FORCE the UI to update to 0 instantly!
                OnPlayerScoreUpdated?.Invoke(i, 0); 
            }
        }
        
        // --- NEW: Tournament Elimination Logic ---
        public void EliminateLowestScoringPlayer()
        {
            PlayerData lowestPlayer = null;
            int lowestScore = int.MaxValue;

            // Find the active player with the lowest total score
            foreach (var p in Players)
            {
                if (!p.IsEliminated && p.TotalScore < lowestScore)
                {
                    lowestScore = p.TotalScore;
                    lowestPlayer = p;
                }
            }

            if (lowestPlayer != null)
            {
                lowestPlayer.IsEliminated = true;
                Debug.Log($"TOURNAMENT: {lowestPlayer.DisplayName} has been ELIMINATED!");
            }
        }

        // =========================
        // GAME STATE INTEGRATION
        // =========================

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Setup:
                    ResetPlayers();
                    break;

                case GameState.Gameplay:
                    Debug.Log("Gameplay started with " + Players.Count + " players");
                    break;  

                case GameState.Results:
                    Debug.Log("Match ended");
                    break;
            }
        }
    }
}