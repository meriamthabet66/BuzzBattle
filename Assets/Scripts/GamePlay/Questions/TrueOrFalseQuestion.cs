using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Questions
{
    [CreateAssetMenu(menuName = "Questions/True False")]
    
    //this class represents the trueorfalse question type . it differs with a bool variable
    public class TrueOrFalseQuestion : BaseQuestion
    {
        public bool isTrue;
        
        // You can change these default words in the Inspector if needed!
        public string trueText = "صحيح";
        public string falseText = "خطأ";
        

        public override QuestionType GetQuestionType()
        {
            return QuestionType.TrueOrFalse;
        }

        public bool CheckAnswer(bool playerChoice)
        {
            return playerChoice == isTrue;
        }
        
        public List<AnswerOption> GetOptions()
        {
            List<AnswerOption> tfOptions = new List<AnswerOption>();

            // 1. Create the "True" Button Data
            AnswerOption trueOption = new AnswerOption();
            trueOption.text = trueText;
            trueOption.isCorrect = isTrue; // If the question is true, this is the right answer
            tfOptions.Add(trueOption);

            // 2. Create the "False" Button Data
            AnswerOption falseOption = new AnswerOption();
            falseOption.text = falseText;
            falseOption.isCorrect = !isTrue; // If the question is false, THIS is the right answer
            tfOptions.Add(falseOption);

            return tfOptions;
        }
    }
}