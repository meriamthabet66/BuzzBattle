using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;
using Data.Data; // Needed to read MatchSetupData

namespace Managers 
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public GameMode currentMode;
        public int totalRounds;
        public int questionsPerRound;

        public int CurrentRoundIndex { get; private set; }
        
        [SerializeField] private RoundManager roundManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // Called by CategoryPanelUI when you click the final "Start" button
        public void StartMatch(GameMode mode, int rounds, int questions)
        {
            currentMode = mode;
            totalRounds = rounds;
            questionsPerRound = questions;
            CurrentRoundIndex = 0;

            Debug.Log($"Match started | Mode: {mode} | Rounds: {rounds} | Questions: {questions}");
        }
        
        // Starts the actual question loop
        public void StartRound(List<Category> categories, QuestionType type)
        {
            if (roundManager == null)
            {
                Debug.LogError("RoundManager missing in MatchManager");
                return;
            }
            roundManager.InitializeRound(categories, type);
        }

        public bool IsLastRound()
        {
            return CurrentRoundIndex >= totalRounds - 1;
        }

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
                Debug.Log("All rounds completed! Game Over. Going to Results.");
                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
    }
}