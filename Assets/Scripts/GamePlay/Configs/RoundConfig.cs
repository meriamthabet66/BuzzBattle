using System.Collections.Generic;

namespace GamePlay.Configs {
    using UnityEngine;
    using GamePlay.Questions;

    [System.Serializable]
    //this class represents a round  
    public class RoundConfig
    {
        public List<Category> categories;
        public QuestionType questionType;
        public int questionCount;
        public int questionTimerSeconds;
    }
}