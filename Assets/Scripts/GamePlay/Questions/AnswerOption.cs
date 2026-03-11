namespace GamePlay.Questions
{
    using UnityEngine;

    [System.Serializable]
    //this class represents the Multichoices answer option 
    //the perpose of it is to have the answer shuffled and still know the right answer( not based on index placement)
    public class AnswerOption
    {
        [TextArea]
        public string text;

        public AudioClip voiceClip;

        public bool isCorrect;
    }
}