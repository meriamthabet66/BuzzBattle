using System.Collections.Generic;
using Data;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int ID;

    public string DisplayName;

    public int RoundScore; // <--- NEW: Tracks just this round
    public int TotalScore; // <--- NEW: Accumulates the whole match
    public int Steals;

    public int SelectedCharacterID;

    // Optional link to account
    public AccountData LinkedAccount;
}

