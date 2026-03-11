namespace GamePlay.Configs {
    using System.Collections.Generic;

    [System.Serializable]
    
    //this class represents a match ( 1 to 5 rounds)
    public class MatchConfig
    {
        public List<RoundConfig> rounds = new List<RoundConfig>();
    }
}