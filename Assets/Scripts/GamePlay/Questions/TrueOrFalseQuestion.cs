using UnityEngine;

namespace GamePlay.Questions
{
    [CreateAssetMenu(menuName = "Questions/True False")]
    
    //this class represents the trueorfalse question type . it differs with a bool variable
    public class TrueOrFalseQuestion : BaseQuestion
    {
        public bool isTrue;
        

        public override QuestionType GetQuestionType()
        {
            return QuestionType.TrueOrFalse;
        }

        public bool CheckAnswer(bool playerChoice)
        {
            return playerChoice == isTrue;
        }
    }
}