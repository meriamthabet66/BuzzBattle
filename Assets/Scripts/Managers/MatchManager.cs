using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;
<<<<<<< HEAD
using Data.Data; // Needed to read MatchSetupData
=======
using Data.Data;
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006

namespace Managers 
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public GameMode currentMode;
        public int totalRounds;
        public int questionsPerRound;

        public int CurrentRoundIndex { get; private set; }
        
        // --- NEW: Tracks if we are mid-game! ---
        public bool IsMatchActive { get; private set; } 
        
        [SerializeField] private RoundManager roundManager;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            IsMatchActive = false; // Match hasn't started yet
        }

<<<<<<< HEAD
        // Called by CategoryPanelUI when you click the final "Start" button
=======
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
        public void StartMatch(GameMode mode, int rounds, int questions)
        {
            currentMode = mode;
            totalRounds = rounds;
            questionsPerRound = questions;
            CurrentRoundIndex = 0;
            IsMatchActive = true; 

            // --- NEW: Tell RoundManager to clear the question memory! ---
            if (roundManager != null) roundManager.ClearQuestionMemory();

            Debug.Log($"Match started | Mode: {mode} | Rounds: {rounds} | Questions: {questions}");
        }
        
        // Starts the actual question loop
        public void StartRound(List<Category> categories, QuestionType type)
        {
            if (roundManager == null) return;
            roundManager.InitializeRound(categories, type);
        }

        public bool IsLastRound()
        {
            return CurrentRoundIndex >= totalRounds - 1;
        }

<<<<<<< HEAD
        // RoundManager calls this when it runs out of questions
        public void OnRoundFinished()
        {
            Debug.Log($"Round {CurrentRoundIndex + 1} Finished!");

            if (!IsLastRound())
            {
                CurrentRoundIndex++;
                Debug.Log($"Starting Round {CurrentRoundIndex + 1} automatically!");
                
                // Automatically grab the saved categories/type and start the next round!
                StartRound(MatchSetupData.SelectedCategories, MatchSetupData.QType);
            }
            else
            {
=======
        // --- UPDATED: Stop auto-starting the round! ---
        public void OnRoundFinished()
        {
            if (!IsLastRound())
            {
                CurrentRoundIndex++;
                Debug.Log($"Round finished! Going back to UI to setup Round {CurrentRoundIndex + 1}");
                
                // Tell GameManager to open the Selection UI again!
                GameManager.Instance.ChangeState(GameState.CategorySelection);
            }
            else
            {
                IsMatchActive = false; // Match is completely over
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
                Debug.Log("All rounds completed! Game Over. Going to Results.");
                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
    }
}