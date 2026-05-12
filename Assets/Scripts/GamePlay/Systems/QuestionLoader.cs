using System;
using System.Collections.Generic;
using UnityEngine;
using GamePlay.Questions;
using GamePlay.Configs;

namespace GamePlay.Systems
{
    public class QuestionLoader : MonoBehaviour
    {
        public static Action<BaseQuestion> OnQuestionLoaded;
        [SerializeField] private AudioSource audioSource;

        private List<BaseQuestion> workingQuestions = new List<BaseQuestion>();
        private int currentIndex = 0;
        public BaseQuestion CurrentQuestion { get; private set; }
        private RoundConfig currentRound;

        // --- NEW: The Memory! ---
        private HashSet<BaseQuestion> askedQuestions = new HashSet<BaseQuestion>();

        // Called when a brand new match starts (Round 1)
        public void ClearMemory()
        {
            askedQuestions.Clear();
            Debug.Log("Question Memory Cleared for new match!");
        }

        public void Initialize(RoundConfig config)
        {
            currentRound = config;
            PrepareQuestions();
        }
        
        private void PrepareQuestions()
        {
            workingQuestions.Clear();
            currentIndex = 0;
            List<BaseQuestion> pool = new List<BaseQuestion>();

            foreach (Category category in currentRound.categories)
            {
                // DEBUG: Let's see if the list is actually empty
                Debug.Log($"Loader checking Category: {category.categoryName}. MCQ Count: {category.multipleChoiceQuestions.Count}");

                switch (currentRound.questionType)
                {
                    case QuestionType.MultipleChoice:
                        foreach (var q in category.multipleChoiceQuestions) 
                            if (!askedQuestions.Contains(q)) pool.Add(q);
                        
                        foreach (var q in category.trueOrFalseQuestions) 
                            if (!askedQuestions.Contains(q)) pool.Add(q);
                        break;
                    
                    case QuestionType.Verbal:
                        foreach (var q in category.verbalQuestions) 
                            if (!askedQuestions.Contains(q)) pool.Add(q);
                        break;
                }
            }
            
            Shuffle(pool);
            
            int count = Mathf.Min(currentRound.questionCount, pool.Count);
            for (int i = 0; i < count; i++)
            {
                workingQuestions.Add(pool[i]);
            }

            if (pool.Count < currentRound.questionCount)
                Debug.LogWarning("Not enough fresh questions left! Some players might not get a question.");
        }
        
        public bool HasMoreQuestions() => currentIndex < workingQuestions.Count;
        
        public void LoadNextQuestion()
        {
            if (!HasMoreQuestions()) return;
            
            BaseQuestion question = workingQuestions[currentIndex++];
            CurrentQuestion = question;
            
            // --- NEW: Add this question to memory so it is never asked again! ---
            askedQuestions.Add(question); 
            
            OnQuestionLoaded?.Invoke(question);
            
            if (GameManager.Instance.IsVoiceEnabled && question.voiceClip != null)
                audioSource.PlayOneShot(question.voiceClip);
        }
        
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