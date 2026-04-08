using UnityEngine;
using System.Collections.Generic;

namespace GamePlay.Questions
{
    [CreateAssetMenu(menuName = "Questions/Multiple Choice")]
    
    //this class represents the multichoices question type. it contains a list of 4 answers ( AnswerOption class objects)
    public class MultipleChoiceQuestion : BaseQuestion
    {
        [Header("Must contain exactly 4 answers (1 correct, 3 wrong)")]
        public List<AnswerOption> answers = new List<AnswerOption>(4);


        public override QuestionType GetQuestionType()
        {
            return QuestionType.MultipleChoice;
        }

        /*this class shuffles a list of answers:
            1)loops the list from beginning to end 
            2)in each loop it creates a random index from the current index til the end of the list
            3)and exchanges the value of the current index with the random index's
         */
            
        public List<AnswerOption> GetShuffledAnswers()
        {
            List<AnswerOption> shuffled = new List<AnswerOption>(answers);

            for (int i = 0; i < shuffled.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[randomIndex]) = 
                    (shuffled[randomIndex], shuffled[i]);
            }

            return shuffled;
        }

        public bool IsCorrect(AnswerOption selected)
        {
            return selected.isCorrect;
        }
    }
}