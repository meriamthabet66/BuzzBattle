using System.Collections.Generic;

namespace Data {
    [System.Serializable]
    public class AccountData
    {
        public string Username;

        public int Stars;
        
        public int Score;
        public int Steals;
        
        public int MatchWinCount;
        public int TournementWinCount;

        public List<CharacterData> OwnedCharacters = new();
    }
}