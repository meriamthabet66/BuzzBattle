using System.Collections.Generic;
using Data;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int ID;

    public string DisplayName;

    public int Score;
    public int Steals;

    public int SelectedCharacterID;

    // Optional link to account
    public AccountData LinkedAccount;
}

