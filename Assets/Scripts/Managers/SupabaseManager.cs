using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using Data.DTO;
using System.Collections.Generic;
using Data;

namespace Managers {
    public class SupabaseManager : MonoBehaviour {
        public static SupabaseManager Instance { get; private set; }

        [Header("Connection")]
        [SerializeField] private string supabaseUrl = "https://your-url.supabase.co";
        [SerializeField] private string supabaseKey = "your-key";

        // THIS IS YOUR VARIABLE NAME -> Client
        public Supabase.Client Client { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- THE PERSISTENCE FIX ---
            var options = new SupabaseOptions { 
                AutoRefreshToken = true, 
                AutoConnectRealtime = true,
                // This line tells Supabase to use our new Unity storage script!
                SessionHandler = new UnitySessionHandler() 
            };
        
            Client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        private async void Start() 
        {
            // 1. Reduced delay to the minimum needed for SDK stability
            await Task.Delay(100); 

            try 
            {
                bool isRecovered = await TryManualRecovery();

                if (isRecovered && Client.Auth.CurrentSession != null) 
                {
                    ProfileDTO profile = await GetMyProfile();
            
                    if (profile != null) 
                    {
                        PlayerManager.Instance.SetMainAccount(profile);
                        
                        // 2. SUCCESS: Jump directly to the Menu (Bar will appear, etc.)
                        GameManager.Instance.ChangeState(GameState.Menu);
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Persistence check failed: " + e.Message);
            }

            // 3. FAILURE: Now show the Authentication Canvas
            GameManager.Instance.ChangeState(GameState.Authentication);
        }

        // --- NEW: THE BULLETPROOF RECOVERY METHOD ---
        private async Task<bool> TryManualRecovery()
        {
            // Make sure this string matches the one in your UnitySessionHandler!
            string key = "supabase_session"; 

            if (PlayerPrefs.HasKey(key))
            {
                try
                {
                    string json = PlayerPrefs.GetString(key);
                    // Manually convert the text back into a Session object
                    var session = Newtonsoft.Json.JsonConvert.DeserializeObject<Supabase.Gotrue.Session>(json);

                    if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                    {
                        // This "Hand-Feeds" the token back into the Supabase Client
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
                // --- FIXED: Changed 'supabase' to 'Client' ---
                if (Client.Auth.CurrentUser == null) return null;

                var response = await Client
                    .From<ProfileDTO>()
                    .Where(x => x.id == Client.Auth.CurrentUser.Id)
                    .Get();

                return response.Model;
            }
            catch (System.Exception e) 
            {
                Debug.LogError($"Failed to fetch profile: {e.Message}");
                return null;
            }
        }

        public async Task<ProfileDTO> Login(string email, string password) {
            try {
                var session = await Client.Auth.SignIn(email, password);
                if (session?.User != null) {
                    var response = await Client.From<ProfileDTO>().Filter("id", Postgrest.Constants.Operator.Equals, session.User.Id).Get();
                    return response.Model;
                }
                return null;
            } catch { return null; }
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
            PlayerPrefs.Save();

            // --- THE FIX: Clear the data in the Manager! ---
            // If we don't do this, the next person who logs in might 
            // briefly see the previous player's stats!
            if (PlayerManager.Instance != null)
            {
                // We'll add this method to PlayerManager next
                PlayerManager.Instance.ClearHostAccount(); 
            }

            GameManager.Instance.ChangeState(GameState.Authentication);
        }

        public async Task<ProfileDTO> SearchPlayerByEmail(string targetEmail) {
            try
            {
                var response = await Client
                    .From<ProfileDTO>()
                    .Where(x => x.email == targetEmail.Trim().ToLower())
                    .Get();

                return response.Model; // Returns the first profile found
            }
            catch (System.Exception e)
            {
                Debug.LogError("Search failed: " + e.Message);
                return null;
            }
        }
        
        public async Task<bool> UnlockCategory(long categoryId, int price) {
            try {
                var user = Client.Auth.CurrentUser;
                if (user == null) return false;

                if (PlayerManager.Instance.HostAccount.Stars < price) return false;

                int newStars = PlayerManager.Instance.HostAccount.Stars - price;
                await Client.From<Data.DTO.ProfileDTO>()
                    .Where(x => x.id == user.Id)
                    .Set(x => x.stars, newStars)
                    .Update();

                var unlockData = new Data.DTO.ProfileCategoryDTO {
                    profile_id = user.Id,
                    category_id = categoryId,
                    is_locked = false
                };
                await Client.From<Data.DTO.ProfileCategoryDTO>().Upsert(unlockData);

                PlayerManager.Instance.HostAccount.Stars = newStars;
                return true;
            } catch (System.Exception e) {
                Debug.LogError("Unlock failed: " + e.Message);
                return false;
            }
        }
     
        public async Task<List<long>> GetUnlockedCategoryIds() {
            try {
                if (Client.Auth.CurrentUser == null) return new List<long>();

                var response = await Client.From<Data.DTO.ProfileCategoryDTO>()
                    .Where(x => x.profile_id == Client.Auth.CurrentUser.Id)
                    .Where(x => x.is_locked == false)
                    .Get();

                List<long> ids = new List<long>();
                foreach (var item in response.Models) {
                    ids.Add(item.category_id);
                }
                return ids;
            } catch {
                return new List<long>(); 
            }
        }
    }
}