using System;
using System.Collections.Generic;
using UnityEngine;
using GamePlay.Questions;
using GamePlay.Configs;

namespace GamePlay.Systems
{
    
    //this class is responsible for loading the questions according to the type 
    public class QuestionLoader : MonoBehaviour
    {
        //this is  listener on the question 
        public static Action<BaseQuestion> OnQuestionLoaded;

        [SerializeField] private AudioSource audioSource;

        private List<BaseQuestion> workingQuestions = new List<BaseQuestion>();
        private int currentIndex = 0;
        public BaseQuestion CurrentQuestion { get; private set; }

        private RoundConfig currentRound;

        public void Initialize(RoundConfig config)
        {
            currentRound = config;
            PrepareQuestions();
        }
        
        //this method prepares the question according to their type. 
        private void PrepareQuestions()
        {
            workingQuestions.Clear();
            currentIndex = 0;
            //it puts the requested type in a list.
            List<BaseQuestion> pool = new List<BaseQuestion>();

            foreach (Category category in currentRound.categories)
            {
                switch (currentRound.questionType)
                {
                    case QuestionType.MultipleChoice:

                        // Regular MCQ
                        pool.AddRange(category.multipleChoiceQuestions);

                        // Include True/False because it's a subtype of MCQ
                        pool.AddRange(category.trueOrFalseQuestions);

                        break;
                    
                    case QuestionType.Verbal:
                        pool.AddRange(category.verbalQuestions);
                        break;
                }
            }
            
            Debug.Log("Pool size before shuffle: " + pool.Count);
            //then shuffles them randomly.
            Shuffle(pool);
            
            if(pool.Count < currentRound.questionCount)
            {
                Debug.LogWarning("Not enough questions in pool, using available ones.");
            }
            
            //and finally puts the indicated number in the workingQuestions list
            int count = Mathf.Min(currentRound.questionCount, pool.Count);

            for (int i = 0; i < count; i++)
            {
                workingQuestions.Add(pool[i]);
            }
        }
        
        
        //this method checks if there are more questions left
        public bool HasMoreQuestions()
        {
            return currentIndex < workingQuestions.Count;
        }
        
        
        //this methods loads the next question in the list
        public void LoadNextQuestion()
        {
            //if there are more questions in the list
            if (!HasMoreQuestions())
            {
                Debug.Log("No more questions.");
                return;
            }
            
            //the next question is loaded
            BaseQuestion question = workingQuestions[currentIndex++];
            CurrentQuestion = question;
            
            OnQuestionLoaded?.Invoke(question);
            
            //the question's voice over play. note: maybe we just turn it to a button later 
            if (GameManager.Instance.IsVoiceEnabled && question.voiceClip != null)
            {
                audioSource.PlayOneShot(question.voiceClip);
            }
        }
        
        
        //same as the method that shuffles the answers in multiChoices class
        private void Shuffle(List<BaseQuestion> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}