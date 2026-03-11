using UnityEngine;

namespace GamePlay.Questions
{
    [CreateAssetMenu(menuName = "Questions/Verbal")]
    
    //this class represents the verbal question type. it has one answer only.
    public class VerbalQuestion : BaseQuestion
    {
        [TextArea(3, 6)]
        public string correctAnswer;

        public AudioClip correctAnswerVoice;

        public override QuestionType GetQuestionType()
        {
            return QuestionType.Verbal;
        }
    }
}