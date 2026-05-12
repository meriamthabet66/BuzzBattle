using UnityEngine;
    
    // this represents the game states, so it's easier to control the game panels in game manager
    public enum GameState
    {
        Authentication, 
        Menu,
        Setup,
        CategorySelection,
        MiniGame,
        Gameplay,
        Steal,
        RoundResults,
        Results,
        Initializing
    }

