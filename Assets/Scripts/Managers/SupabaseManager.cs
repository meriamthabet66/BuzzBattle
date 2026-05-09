using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using Data.DTO; // Ensure this matches your ProfileDTO namespace
using System.Collections.Generic;

namespace Managers {
    public class SupabaseManager : MonoBehaviour {
        public static SupabaseManager Instance { get; private set; }

        [Header("Supabase Credentials")]
        [SerializeField] private string supabaseUrl = "https://aahjaarfkajqlbxubzld.supabase.co";
        [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFhaGphYXJma2FqcWxieHViemxkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc5OTAzMTUsImV4cCI6MjA5MzU2NjMxNX0._0oLFxsXgwvMRSVymoN7SAuuOLPaGq5SLNyJP927SF8";

        private Supabase.Client supabase;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize
            var options = new SupabaseOptions {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        // =========================
        // SIGN UP
        // =========================
        public async Task<bool> SignUp(string email, string password, string username) {
            try {
                // We pack the username into "UserMetadata" so the SQL Trigger can find it
                var signupOptions = new Supabase.Gotrue.SignUpOptions {
                    Data = new Dictionary<string, object> { { "username", username } }
                };

                // Create the account. The SQL Trigger handles the 'profiles' table for us!
                var session = await supabase.Auth.SignUp(email, password, signupOptions);

                if (session != null && session.User != null) {
                    Debug.Log("<color=green>SUCCESS! Account and Profile created via Trigger.</color>");
                    return true;
                }
                return false;
            } catch (System.Exception e) {
                // If it says "User already registered", delete them from the Auth tab and try again
                Debug.LogError($"Signup Error: {e.Message}");
                return false;
            }
        }

        // =========================
        // LOGIN
        // =========================
        // =========================
        // LOGIN & FETCH PROFILE
        // =========================
        public async Task<ProfileDTO> Login(string email, string password) {
            try {
                var session = await supabase.Auth.SignIn(email, password);

                if (session != null && session.User != null) {
                    Debug.Log("<color=green>Auth Successful! ID: " + session.User.Id + "</color>");

                    // We use the ID directly from the session we just got
                    var response = await supabase
                        .From<ProfileDTO>()
                        .Filter("id", Postgrest.Constants.Operator.Equals, session.User.Id)
                        .Get();

                    if (response.Model != null) {
                        return response.Model;
                    } else {
                        // If we reach here, Step 1 (the Repair SQL) wasn't run or failed
                        Debug.LogError("Auth worked, but NO PROFILE ROW found for ID: " + session.User.Id);
                        return null;
                    }
                }
                return null;
            } catch (System.Exception e) {
                Debug.LogError($"Login Error: {e.Message}");
                return null;
            }
        }

        // =========================
        // GET CURRENT PROFILE
        // =========================
        public async Task<ProfileDTO> GetMyProfile() {
            if (supabase.Auth.CurrentUser == null) return null;

            var response = await supabase
                .From<ProfileDTO>()
                .Where(x => x.id == supabase.Auth.CurrentUser.Id)
                .Get();

            return response.Model;
        }

        public async Task<ProfileDTO> SearchPlayerByEmail(string targetEmail) {
            try {
                // We clean the input to avoid invisible space errors
                string cleanEmail = targetEmail.Trim().ToLower();

                // We ask the profiles table: "Who has this email?"
                var response = await supabase
                    .From<ProfileDTO>()
                    .Where(x => x.email == cleanEmail)
                    .Get();

                // .Model returns the profile if found, otherwise null
                if (response.Model != null) {
                    Debug.Log($"<color=cyan>Found player: {response.Model.username}</color>");
                    return response.Model;
                } else {
                    Debug.LogWarning("No account found with that email.");
                    return null;
                }
            } catch (System.Exception e) {
                Debug.LogError($"Search Error: {e.Message}");
                return null;
            }

        }
        
        
        public async Task SaveMatchResults(string profileId, int pointsGained, bool wonMatch)
        {
            try
            {
                // 1. Fetch current stats
                var response = await supabase.From<ProfileDTO>().Where(x => x.id == profileId).Get();
                var profile = response.Model;

                if (profile != null)
                {
                    // 2. Update stats
                    profile.score += pointsGained;
                    profile.matches_played += 1;
                    if (wonMatch) profile.match_wins += 1;

                    // 3. Push back to Supabase
                    await profile.Update<ProfileDTO>();
                    Debug.Log($"Cloud Save Complete for {profile.username}!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Cloud Save Failed: {e.Message}");
            }
        }



    }
}