using System;
using System.Collections.Generic;

namespace Data {
    [System.Serializable]
    public class AccountData
    {
        public string id; 
        public string email; 
        public string Username;
        public int Stars;
        public int Score;
        public int Steals;
        public int CorrectSteals; 
        public int MatchWinCount;
        public int MatchPlayedCount; 
        public int TournamentWinCount;
        
        // --- THE KEY TO SYNCING ---
        public DateTime last_updated; 

        public List<CharacterData> OwnedCharacters = new();
    }
}