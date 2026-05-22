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
        
        public AccountData HostAccount { get; private set; }
        public List<PlayerData> Players = new List<PlayerData>();

        // 🔥 Events for UI
        public static Action<List<PlayerData>> OnPlayersUpdated;
        public static Action<int, int> OnPlayerScoreUpdated; 
        
        // --- NEW: Event to update the Stars in the Header Panel ---
        public static Action OnHostStarsUpdated; 

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

        private void SetMainAccount(ProfileDTO dto)
        {
            HostAccount = new AccountData
            {
                id = dto.id,
                email = dto.email,
                Username = dto.username,
                Stars = dto.stars,
                Score = dto.score,
                Steals = dto.total_steals,
                CorrectSteals = dto.correct_steals, // Make sure this is mapped!
                MatchWinCount = dto.match_wins,
                TournamentWinCount = dto.tournament_wins
            };

            Debug.Log($"<color=orange>Host Account set: {HostAccount.Username}. Stars: {HostAccount.Stars}</color>");
            
            // --- NEW: Tell the HeadPanel to update the visual number! ---
            OnHostStarsUpdated?.Invoke(); 
        }
        
        public void SetMainAccount(AccountData account)
        {
            HostAccount = account;
            Debug.Log($"<color=orange>Host Account set from LOCAL SAVE: {HostAccount.Username}.</color>");
            OnHostStarsUpdated?.Invoke(); 
        }
        
  
        
        // --- NEW: Called by LocalAccountManager to sync the UI ---
        public void UpdateHostStatsVisually(AccountData updatedData)
        {
            if (updatedData == null) return;

            HostAccount = updatedData;
            
            // This event tells the HeadPanelUI to redraw the stars!
            OnHostStarsUpdated?.Invoke(); 
            
            Debug.Log($"<color=cyan>Host Stats Updated Visually. New Stars: {HostAccount.Stars}</color>");
        }

        // --- NEW: Call this when spending or earning stars ---
        public void UpdateHostStars(int newStarCount)
        {
            if (HostAccount != null)
            {
                HostAccount.Stars = newStarCount;
                OnHostStarsUpdated?.Invoke(); 
            }
        }
        
        public void ClearHostAccount()
        {
            HostAccount = null;
            OnHostStarsUpdated?.Invoke(); // Will clear the text to 0
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
                RoundScore = 0,
                Steals = 0,
                SelectedCharacterID = selectedCharacterId,
                LinkedAccount = account
            };

            Players.Add(player);
            Debug.Log($"Player added: {name}");
            OnPlayersUpdated?.Invoke(Players);
        }
        
        // Helper to convert DTO to AccountData
        public AccountData ConvertDTOToAccount(ProfileDTO dto)
        {
            return new AccountData {
                id = dto.id,
                email = dto.email,
                Username = dto.username,
                Stars = dto.stars,
                Score = dto.score,
                Steals = dto.total_steals,
                CorrectSteals = dto.correct_steals,
                MatchWinCount = dto.match_wins,
                TournamentWinCount = dto.tournament_wins
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
            if (index < 0 || index >= Players.Count) return null;
            return Players[index];
        }
        
        public void AddScore(int index, int pointsToAdd)
        {
            if (index < 0 || index >= Players.Count) return;

            Players[index].RoundScore += pointsToAdd;
            Players[index].TotalScore += pointsToAdd;

            OnPlayerScoreUpdated?.Invoke(index, Players[index].TotalScore);
        }

        public void ResetRoundScores()
        {
            foreach (var player in Players) player.RoundScore = 0;
        }
        
        public void ResetScoresForRematch()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].TotalScore = 0;
                Players[i].RoundScore = 0;
                OnPlayerScoreUpdated?.Invoke(i, 0); 
            }
        }
        
        public void EliminateLowestScoringPlayer()
        {
            PlayerData lowestPlayer = null;
            int lowestScore = int.MaxValue;

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
        
        
        
        public void GlobalLogoutReset()
        {
            // 1. Clear the Host
            ClearHostAccount();

            // 2. Clear the active player list
            ResetPlayers();

            // 3. Reset the static match config
            Data.Data.MatchSetupData.ResetData();
    
            Debug.Log("<color=red>PlayerManager: Global Reset Complete.</color>");
        }
    }
}