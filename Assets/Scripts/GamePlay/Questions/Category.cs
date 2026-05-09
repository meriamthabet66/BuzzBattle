using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Questions {
    [CreateAssetMenu(menuName = "Questions/Category")]
    
    //this class represents a category, where it contains a list of each question type
    public class Category : ScriptableObject
    {
        public string categoryName;
        public Sprite categoryIcon;
        public string categoryIconUrl;

        [Header("Multiple Choice Questions")]
        public List<MultipleChoiceQuestion> multipleChoiceQuestions;

        [Header("True or False Questions")]
        public List<TrueOrFalseQuestion> trueOrFalseQuestions;

        [Header("Verbal Questions")]
        public List<VerbalQuestion> verbalQuestions;
    }
}