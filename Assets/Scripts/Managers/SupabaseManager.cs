using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using Data.DTO;
using System.Collections.Generic;

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

            var options = new SupabaseOptions { AutoRefreshToken = true, AutoConnectRealtime = true };
            
            // Initialization
            Client = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }
        
        private async void Start() 
        {
            await Task.Delay(100);

            // --- FIXED: Changed 'supabase' to 'Client' ---
            if (Client.Auth.CurrentSession != null) 
            {
                Debug.Log("<color=green>Session found! Attempting auto-login...</color>");
        
                ProfileDTO profile = await GetMyProfile();
                
                if (profile != null) 
                {
                    PlayerManager.Instance.SetMainAccount(profile);
                    GameManager.Instance.ChangeState(GameState.Menu);
                    return;
                }
            }

            Debug.Log("No session found. Stay on Authentication.");
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

        public async Task<ProfileDTO> SearchPlayerByEmail(string targetEmail) {
            var response = await Client.From<ProfileDTO>().Where(x => x.email == targetEmail.Trim().ToLower()).Get();
            return response.Model;
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