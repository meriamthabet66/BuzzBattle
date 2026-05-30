using System;
using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using Data.DTO;
using System.Collections.Generic;
using GamePlay.Questions;

namespace Managers {
    public class SupabaseManager : MonoBehaviour {
        public static SupabaseManager Instance { get; private set; }

        [Header("Connection")]
        [SerializeField] private string supabaseUrl = "https://aahjaarfkajqlbxubzld.supabase.co";
        [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFhaGphYXJma2FqcWxieHViemxkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc5OTAzMTUsImV4cCI6MjA5MzU2NjMxNX0._0oLFxsXgwvMRSVymoN7SAuuOLPaGq5SLNyJP927SF8";

        public Supabase.Client Client { get; private set; }
        public List<ProfileDTO> CachedGlobalLeaderboard { get; private set; }
        public List<Category> CachedCloudCategories { get; private set; } = new List<Category>();
        public List<long> CachedUnlockedIds { get; private set; } = new List<long>();
        
        public static Action OnLobbyDataReady;
    
        private NetworkReachability lastReachability;

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var options = new SupabaseOptions { 
                AutoRefreshToken = true, 
                AutoConnectRealtime = true,
                SessionHandler = new Data.UnitySessionHandler() 
            };
        
            Client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        private async void Start() 
        {
            
            lastReachability = Application.internetReachability;
            await Task.Delay(100); 

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (LocalAccountManager.Instance != null && LocalAccountManager.Instance.SavedAccount != null)
                {
                    // If offline, the Local Manager ALREADY loaded the data in its Awake method.
                    // We just transition to the Menu.
                    GameManager.Instance.ChangeState(GameState.Menu);
                }
                else
                {
                    GameManager.Instance.ChangeState(GameState.Authentication);
                }
                return; 
            }

            try 
            {
                bool isRecovered = await TryManualRecovery();
                if (isRecovered && Client.Auth.CurrentSession != null) 
                {
                    ProfileDTO profile = await GetMyProfile();
                    if (profile != null) 
                    {
                        // --- THE FIX: REMOVED SetMainAccount(profile) ---
                        // Only let the Local Manager handle this data!
                        if (LocalAccountManager.Instance != null) {
                            LocalAccountManager.Instance.SaveProfileFromCloud(profile);
                        }
                        _ = PreloadLobbyData();_ = PreloadLobbyData();
                        GameManager.Instance.ChangeState(GameState.Menu);
                        return;
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning("Persistence failed: " + e.Message); }

            GameManager.Instance.ChangeState(GameState.Authentication);
        }
        
        
        private void Update()
        {
            // --- THE AUTO-REFRESH MIRACLE ---
            NetworkReachability currentReachability = Application.internetReachability;

            // If we were offline and now we are online
            if (lastReachability == NetworkReachability.NotReachable && 
                currentReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("<color=green>Internet Restored! Refreshing Lobby Data...</color>");
                _ = PreloadLobbyData(); // Silently fetch data
            }

            lastReachability = currentReachability;
        }

        private async Task<bool> TryManualRecovery()
        {
            string key = "supabase_session"; 
            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    string json = PlayerPrefs.GetString(key);
                    var session = Newtonsoft.Json.JsonConvert.DeserializeObject<Supabase.Gotrue.Session>(json);

                    if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                    {
                        await Client.Auth.SetSession(session.AccessToken, session.RefreshToken);
                        return true;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to parse saved session: " + ex.Message);
                    return false;
                }
            }
            return false;
        }
        
        public async Task<ProfileDTO> GetMyProfile() 
        {
            try 
            {
                if (Client.Auth.CurrentUser == null) return null;
                var response = await Client.From<ProfileDTO>().Where(x => x.id == Client.Auth.CurrentUser.Id).Get();
                return response.Model;
            }
            catch { return null; }
        }

        public async Task<bool> Login(string email, string password) {
            try {
                var session = await Client.Auth.SignIn(email, password);
                if (session?.User != null) {
                    var response = await Client.From<ProfileDTO>().Filter("id", Postgrest.Constants.Operator.Equals, session.User.Id).Get();
                    
                    if (LocalAccountManager.Instance != null) {
                        // This is the ONLY call we need. 
                        // It will check if local > cloud before updating.
                        LocalAccountManager.Instance.SaveProfileFromCloud(response.Model);
                    }
                    return true; // Return a bool, not the profile!
                }
                return false;
            } catch { return false; }
        }
        public async Task<bool> SignUp(string email, string password, string username) {
            try {
                var options = new Supabase.Gotrue.SignUpOptions { Data = new Dictionary<string, object> { { "username", username } } };
                var session = await Client.Auth.SignUp(email, password, options);
                return session?.User != null;
            } catch { return false; }
        }
        
       public void Logout()
        {
            Client.Auth.SignOut();
            PlayerPrefs.DeleteKey("supabase_session");
            
            // 1. RESET LOGIC: Revert timers and volume in memory/disk
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ResetAllSettings();
            }

            // 2. RESET VISUALS: Revert sliders and texts (even if panel is hidden)
            var settingsPanel = FindFirstObjectByType<UI.Panels.SettingsPanelUI>(FindObjectsInactive.Include);
            if (settingsPanel != null)
            {
                settingsPanel.ResetUI();
            }
            
            PlayerPrefs.Save();

            if (LocalAccountManager.Instance != null)
                LocalAccountManager.Instance.WipeLocalData();

            if (PlayerManager.Instance != null)
                PlayerManager.Instance.GlobalLogoutReset();

            // 1. WIPE Individual Player Slots
            var setupPanel = FindFirstObjectByType<UI.Panels.PlayerSetupPanelUI>(FindObjectsInactive.Include);
            if (setupPanel != null) setupPanel.ResetAllSlots();

            // 2. WIPE Team Names
            var teamsPanel = FindFirstObjectByType<UI.SelectionPanels.TeamsSetupPanelUI>(FindObjectsInactive.Include);
            if (teamsPanel != null) teamsPanel.ResetFields();

            // 3. --- THE FIX: RESET Profile Popup ---
            var profilePanel = FindFirstObjectByType<UI.ProfilePanelUI>(FindObjectsInactive.Include);
            if (profilePanel != null) profilePanel.ResetUI();

            // 4. RESET MENU: Back to Home tab
            var barMenu = FindFirstObjectByType<UI.BarMenuHandler>(FindObjectsInactive.Include);
            if (barMenu != null) barMenu.OnTabClicked(0);

            GameManager.Instance.ChangeState(GameState.Authentication);
        }

        public async Task<ProfileDTO> SearchPlayerByEmail(string targetEmail) {
            try {
                string cleanEmail = targetEmail.Trim().Replace("\u200B", "").ToLower();
                var response = await Client.From<ProfileDTO>().Where(x => x.email == cleanEmail).Get();
                return (response.Models != null && response.Models.Count > 0) ? response.Models[0] : null;
            } catch { return null; }
        }
        
        public async Task<List<Data.CharacterData>> FetchCharactersForPlayer(string profileId)
        {
            try {
                List<Data.CharacterData> characterList = new List<Data.CharacterData>();
                var savedCharsResponse = await Client.From<Data.DTO.SavedCharacterDTO>().Where(x => x.profile_id == profileId).Get();

                if (savedCharsResponse.Models != null) {
                    foreach (var charDto in savedCharsResponse.Models) {
                        characterList.Add(new Data.CharacterData {
                            id = charDto.id,
                            nickname = charDto.nickname,
                            skin_url = charDto.skin_url,
                            Items = new List<Data.ItemData>() 
                        });
                    }
                }
                return characterList;
            } catch { return new List<Data.CharacterData>(); }
        }

        // --- SHOP & ECONOMY ---
        public async Task<bool> UnlockCategory(long categoryId, int price) {
            try {
                var user = Client.Auth.CurrentUser;
                if (user == null) return false;

                if (LocalAccountManager.Instance.SavedAccount.Stars < price) return false;

                int newStars = LocalAccountManager.Instance.SavedAccount.Stars - price;
                
                // 1. Update Cloud
                await Client.From<Data.DTO.ProfileDTO>()
                    .Where(x => x.id == user.Id).Set(x => x.stars, newStars).Update();

                var unlockData = new Data.DTO.ProfileCategoryDTO {
                    profile_id = user.Id, category_id = categoryId, is_locked = false
                };
                await Client.From<Data.DTO.ProfileCategoryDTO>().Upsert(unlockData);

                // 2. Update Local Memory
                LocalAccountManager.Instance.SavedAccount.Stars = newStars;
                
                // --- THE FIX: Save the new star count to the phone's disk! ---
                // We manually trigger the Local Manager to save the new JSON
                // We use Reflection or make SaveToDisk public. 
                // Let's call AddMatchStats with 0s to force a save:
                LocalAccountManager.Instance.AddMatchStats(0, 0, 0, 0, 0, 0, 0); 
                
                PlayerManager.Instance.UpdateHostStars(newStars);
                return true;
            } catch { return false; }
        }
     
        public async Task<List<long>> GetUnlockedCategoryIds() {
            try {
                if (Client.Auth.CurrentUser == null) return new List<long>();
                var response = await Client.From<Data.DTO.ProfileCategoryDTO>().Where(x => x.profile_id == Client.Auth.CurrentUser.Id).Where(x => x.is_locked == false).Get();

                List<long> ids = new List<long>();
                foreach (var item in response.Models) ids.Add(item.category_id);
                return ids;
            } catch { return new List<long>(); }
        }
        
        
        // Method to fetch data silently in the background
        public async Task PreloadLobbyData()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            try {
                // Fetch Leaderboard
                var leaderResponse = await Client.From<ProfileDTO>()
                    .Order("score", Postgrest.Constants.Ordering.Descending)
                    .Limit(10).Get();
                CachedGlobalLeaderboard = leaderResponse.Models;

                // Fetch Categories
                CachedCloudCategories = await CategoryCloudManager.Instance.GetCategoryList();
                CachedUnlockedIds = await GetUnlockedCategoryIds();
            
                // --- THE SIGNAL: Tell the UI that data is now in memory! ---
                OnLobbyDataReady?.Invoke();
            
                Debug.Log("<color=green>Lobby: Pre-load complete!</color>");
            } catch { }
        }
        
        // --- NEW: Direct Save for Guest Accounts (Non-Hosts) ---
        public async Task SaveMatchResults(string profileId, int stars, int score, int matches, int wins, int tWins, int cSteals, int tSteals)
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_id", profileId }, { "p_stars", stars }, { "p_score", score },
                { "p_matches_played", matches }, { "p_match_wins", wins },
                { "p_tournament_wins", tWins }, { "p_correct_steals", cSteals },
                { "p_total_steals", tSteals }, { "p_last_updated", DateTime.UtcNow }
            };

            try {
                await Client.Rpc("update_player_stats", parameters);
                Debug.Log($"<color=green>Cloud Sync Successful for Guest ID: {profileId}</color>");
            } catch (Exception e) {
                Debug.LogError("Guest Cloud Save Failed: " + e.Message);
            }
        }
        
        
        // public async Task RefreshLobbyCache()
        // {
        //     if (Application.internetReachability == NetworkReachability.NotReachable) return;
        //
        //     try {
        //         // 1. Fetch Categories
        //         CachedCloudCategories = await CategoryCloudManager.Instance.GetCategoryList();
        //
        //         // 2. Fetch Unlocks
        //         CachedUnlockedIds = await GetUnlockedCategoryIds();
        //
        //         Debug.Log("<color=green>Lobby Cache Refreshed Successfully.</color>");
        //     } catch {
        //         Debug.LogWarning("Failed to refresh lobby cache (Internet issue?).");
        //     }
        // }
        
        
        
        
        
        
        // =========================
        // ACCOUNT EDITING
        // =========================

        public async Task<bool> UpdateUsername(string newUsername)
        {
            try
            {
                string userId = Client.Auth.CurrentUser.Id;
                
                // Update the public.profiles table
                await Client.From<ProfileDTO>()
                    .Where(x => x.id == userId)
                    .Set(x => x.username, newUsername)
                    .Update();

                // Update Local Memory
                LocalAccountManager.Instance.SavedAccount.Username = newUsername;
                LocalAccountManager.Instance.ForceSaveToDisk();
                
                Debug.Log("<color=green>Username updated successfully!</color>");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Update Username Failed: " + e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateEmail(string newEmail)
        {
            try
            {
                // --- THE AGGRESSIVE CLEAN FIX ---
                // Strip spaces, line breaks, AND literal quotation marks!
                string cleanEmail = newEmail.Replace("\u200B", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace("\"", "") // Removes "
                    .Replace("'", "")  // Removes '
                    .Trim().ToLower();

                Debug.Log($"<color=yellow>Sending Email Update to Auth Vault: [{cleanEmail}]</color>");

                // 1. Update the Auth Vault (Supabase Auth)
                var attrs = new Supabase.Gotrue.UserAttributes { Email = cleanEmail };
                var user = await Client.Auth.Update(attrs);

                if (user != null)
                {
                    // 2. Update our public.profiles table
                    await Client.From<ProfileDTO>()
                        .Where(x => x.id == user.Id)
                        .Set(x => x.email, cleanEmail)
                        .Update();

                    // 3. Update Local Memory
                    LocalAccountManager.Instance.SavedAccount.email = cleanEmail;
                    LocalAccountManager.Instance.ForceSaveToDisk();
                
                    Debug.Log("<color=green>Email updated successfully in Vault and Profile!</color>");
                    return true;
                }
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Update Email Failed: " + e.Message);
                return false;
            }
        }

        public async Task<bool> UpdatePassword(string newPassword)
        {
            try
            {
                // Passwords only need to be updated in the Auth Vault
                var attrs = new Supabase.Gotrue.UserAttributes { Password = newPassword };
                await Client.Auth.Update(attrs);
                
                Debug.Log("<color=green>Password updated successfully!</color>");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Update Password Failed: " + e.Message);
                return false;
            }
        }

        public async Task<bool> DeleteMyAccount()
        {
            try
            {
                Debug.Log("<color=red>Initiating permanent account deletion...</color>");

                // 1. Call the SQL function we just created
                // This cleans the DB and the Auth vault in one step
                await Client.Rpc("delete_my_account", null);
                
                // 2. Clear local data immediately so the app doesn't try 
                // to sync deleted data on the next frame
                if (LocalAccountManager.Instance != null)
                {
                    LocalAccountManager.Instance.WipeLocalData();
                }

                // 3. Log out and return to Auth screen
                Logout();
                
                Debug.Log("<color=green>Account deleted successfully.</color>");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Delete Account Failed: " + e.Message);
                return false;
            }
        }
    }
}