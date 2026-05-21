[System.Serializable]
public class PlayerData
{
    public int ID;
    public string DisplayName;

    public int RoundScore; 
    public int TotalScore; 
    
    // --- STATS FOR THIS SPECIFIC MATCH ---
    public int Steals;              // Total attempts
    public int CorrectStealsInMatch; // Only successful ones
    
    public bool MatchWon;      // --- NEW: Did they win the Normal Match?
    public bool TournamentWon; // --- NEW: Did they win the Tournament?

    public int SelectedCharacterID;
    public Data.AccountData LinkedAccount;
    public bool IsEliminated = false; 
}