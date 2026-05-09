using System.Collections.Generic;

namespace Data {
    [System.Serializable]
    public class AccountData
    {
        public string id; // The Supabase UUID
        public string email; // THE SEARCH KEY
        public string Username;

        public int Stars;
        public int Score;
        public int Steals;
        public int CorrectSteals; // Match your SQL attribute
    
        public int MatchWinCount;
        public int MatchPlayedCount; // Match your SQL attribute
        public int TournamentWinCount;

        public List<CharacterData> OwnedCharacters = new();
    }
}