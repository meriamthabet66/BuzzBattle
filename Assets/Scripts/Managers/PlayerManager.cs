using System.Collections.Generic;
using UnityEngine;
using System;
using Data;

namespace Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

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