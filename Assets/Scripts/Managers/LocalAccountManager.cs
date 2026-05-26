using UnityEngine;
using Data;
using Data.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Managers
{
    public class LocalAccountManager : MonoBehaviour
    {
        public static LocalAccountManager Instance { get; private set; }
        public AccountData SavedAccount { get; private set; }

        private const string SAVE_KEY = "local_player_stats";
        private bool isSyncing = false;
        private bool needsCloudSync = false; 

        private void Awake() { 
            Instance = this; 
            LoadFromDisk(); 
        }

        private void Update()
        {
            // --- THE AUTO-SYNC MIRACLE ---
            // Every frame, if we need a sync and internet is back, DO IT NOW.
            if (needsCloudSync && !isSyncing && Application.internetReachability != NetworkReachability.NotReachable)
            {
                SyncWithCloud();
            }
        }
        
        // Add this near the top of LocalAccountManager.cs
        public bool NeedsSync => needsCloudSync;

        public bool IsSyncingInProgress() => isSyncing;

        public void SaveProfileFromCloud(ProfileDTO dto)
        {
            // --- THE IMPENETRABLE GUARD ---
            if (SavedAccount != null && SavedAccount.id == dto.id)
            {
                if (SavedAccount.Stars > dto.stars)
                {
                    Debug.Log($"<color=orange>GUARD: Phone:{SavedAccount.Stars} > Cloud:{dto.stars}.</color>");
                    needsCloudSync = true;
                    
                    // --- THE FIX: Trigger the sync UP immediately! ---
                    // SyncWithCloud(); 
                    
                    PlayerManager.Instance.SetMainAccount(SavedAccount);
                    return; 
                }
            }

            // If we get here, the cloud is actually newer or we are fresh
            Debug.Log($"<color=cyan>SYNC: Accepting Cloud Data. Stars: {dto.stars}</color>");
            
            SavedAccount = new AccountData {
                id = dto.id,
                email = dto.email,
                Username = dto.username,
                Stars = dto.stars,
                Score = dto.score,
                Steals = dto.total_steals,
                CorrectSteals = dto.correct_steals,
                MatchWinCount = dto.match_wins,
                MatchPlayedCount = dto.matches_played,
                TournamentWinCount = dto.tournament_wins,
                last_updated = dto.last_updated
            };
            
            needsCloudSync = false;
            SaveToDisk();
            PlayerManager.Instance.SetMainAccount(SavedAccount);
        }

        public void AddMatchStats(int newStars, int newScore, int matchesPlayed, int matchesWon, int tournamentWins, int correctSteals, int totalSteals)
        {
            if (SavedAccount == null) return;

            SavedAccount.Stars += newStars;
            SavedAccount.Score += newScore;
            SavedAccount.MatchPlayedCount += matchesPlayed;
            SavedAccount.MatchWinCount += matchesWon;
            SavedAccount.TournamentWinCount += tournamentWins;
            SavedAccount.CorrectSteals += correctSteals;
            SavedAccount.Steals += totalSteals;
            
            SavedAccount.last_updated = DateTime.UtcNow;
            needsCloudSync = true; // Mark for automatic push in Update()

            SaveToDisk();
            PlayerManager.Instance.UpdateHostStatsVisually(SavedAccount);
        }

        public async Task SyncWithCloud()
        {
            if (isSyncing || SavedAccount == null) return;
            isSyncing = true;
            
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                await CacheCharactersLocally();
            }

            try
            {
                // 1. Double check the cloud one last time
                var response = await SupabaseManager.Instance.Client.From<ProfileDTO>()
                    .Where(x => x.id == SavedAccount.id).Get();
                
                ProfileDTO cloud = response.Model;
                
                // 2. If phone is better, PUSH.
                if (cloud == null || SavedAccount.Stars > cloud.stars || SavedAccount.last_updated > cloud.last_updated)
                {
                    Debug.Log($"<color=green>Pushing Local Stats to Supabase: {SavedAccount.Stars} stars...</color>");
                    await PushToCloud();
                    needsCloudSync = false;
                    SaveToDisk();
                }
                // 3. If cloud is better, PULL.
                else if (cloud.last_updated > SavedAccount.last_updated)
                {
                    SaveProfileFromCloud(cloud);
                }
            }
            catch (Exception e) { Debug.LogError("Sync Error: " + e.Message); }
            finally { isSyncing = false; }
            
            
            
        }
        
        
        private async Task CacheCharactersLocally()
        {
            try
            {
                var chars = await SupabaseManager.Instance.Client.From<SavedCharacterDTO>().Where(x => x.profile_id == SavedAccount.id).Get();
                var registry = await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>().Where(x => x.profile_id == SavedAccount.id).Get();
                var catalog = await SupabaseManager.Instance.Client.From<ItemDTO>().Get();

                SavedAccount.OwnedCharacters.Clear();

                if (chars.Models != null)
                {
                    foreach (var c in chars.Models)
                    {
                        var newFox = new CharacterData { id = c.id, nickname = c.nickname };
                        
                        // Find what this fox is wearing
                        var outfit = registry.Models.FindAll(x => x.character_id == c.id);
                        foreach (var itemLink in outfit)
                        {
                            var itemDetails = catalog.Models.Find(i => i.id == itemLink.item_id);
                            if (itemDetails != null) newFox.EquippedItemNames.Add(itemDetails.item_name);
                        }
                        
                        SavedAccount.OwnedCharacters.Add(newFox);
                    }
                }
                SaveToDisk(); // Save to phone!
                Debug.Log("<color=green>Characters Cached for Offline Use!</color>");
            }
            catch { Debug.LogWarning("Failed to cache characters."); }
        }
        
        private async Task PushToCloud()
        {
            if (SavedAccount == null || SupabaseManager.Instance == null) return;

            // Prepare the parameters to match the SQL function exactly
            var parameters = new Dictionary<string, object>
            {
                { "p_id", SavedAccount.id },
                { "p_stars", SavedAccount.Stars },
                { "p_score", SavedAccount.Score },
                { "p_matches_played", SavedAccount.MatchPlayedCount },
                { "p_match_wins", SavedAccount.MatchWinCount },
                { "p_tournament_wins", SavedAccount.TournamentWinCount },
                { "p_correct_steals", SavedAccount.CorrectSteals },
                { "p_total_steals", SavedAccount.Steals },
                // Use UtcNow or the saved timestamp to ensure timezone consistency
                { "p_last_updated", SavedAccount.last_updated } 
            };

            try 
            {
                // Call the Stored Procedure we created in the SQL Editor
                await SupabaseManager.Instance.Client.Rpc("update_player_stats", parameters);
                Debug.Log("<color=green>PushToCloud: Successfully updated database.</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError("PushToCloud Failed: " + e.Message);
                // If it fails, keep needsCloudSync true so it tries again later
                needsCloudSync = true;
            }
        }

        private void SaveToDisk()
        {
            if (SavedAccount == null) return;
            string json = JsonConvert.SerializeObject(SavedAccount);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.SetInt("needs_sync", needsCloudSync ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadFromDisk()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                SavedAccount = JsonConvert.DeserializeObject<AccountData>(json);
                needsCloudSync = PlayerPrefs.GetInt("needs_sync", 0) == 1;
                
                // --- THE FIX: Add a null check for the Instance ---
                if (SavedAccount != null && PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.SetMainAccount(SavedAccount);
                }
            }
        }

        public void WipeLocalData()
        {
            SavedAccount = null;
            needsCloudSync = false;
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey("needs_sync");
            PlayerPrefs.Save();
            if (PlayerManager.Instance != null) PlayerManager.Instance.ClearHostAccount();
        }
        
        
        
        // Change from private to public!
        public void ForceSaveToDisk()
        {
            if (SavedAccount == null) return;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(SavedAccount);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.SetInt("needs_sync", needsCloudSync ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        // Don't forget to update any places inside this script that were calling 
        // SaveToDisk() to now call ForceSaveToDisk() instead!
        
        
        
    }
}