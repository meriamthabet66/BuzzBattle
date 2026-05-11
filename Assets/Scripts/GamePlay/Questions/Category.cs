using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Questions {
    [CreateAssetMenu(menuName = "Questions/Category")]
    
    //this class represents a category, where it contains a list of each question type
    public class Category : ScriptableObject
    {
        public long id;
        public string categoryName;
        public Sprite categoryIcon;
        public string categoryIconUrl;
        public int price;

        [Header("Multiple Choice Questions")]
        public List<MultipleChoiceQuestion> multipleChoiceQuestions= new List<MultipleChoiceQuestion>();

        [Header("True or False Questions")]
        public List<TrueOrFalseQuestion> trueOrFalseQuestions= new List<TrueOrFalseQuestion>();

        [Header("Verbal Questions")]
        public List<VerbalQuestion> verbalQuestions= new List<VerbalQuestion>();
    }
}