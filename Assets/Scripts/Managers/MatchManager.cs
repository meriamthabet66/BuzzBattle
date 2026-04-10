using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;

namespace Managers {
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public GameMode currentMode;
        public int totalRounds;
        public int questionsPerRound;

        public int CurrentRoundIndex { get; private set; }
        
        [SerializeField] private RoundManager roundManager;

        // --- ADDED FOR SPRINT 2 TESTING ---
        [Header("Sprint 2 Test Data")]
        public Category testCategory; // Drag one of your Category ScriptableObjects here in Unity!
        // ----------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // --- ADDED FOR SPRINT 2 TESTING ---
        private void Start()
        {
            if (testCategory != null)
            {
                Debug.Log("Starting Sprint 2 Test Match...");
                StartMatch(GameMode.Normal, 3, 5); // 3 rounds, 5 questions
                
                List<Category> testList = new List<Category> { testCategory };
                StartRound(testList, QuestionType.MultipleChoice);
            }
            else
            {
                Debug.LogWarning("Please drag a Test Category into MatchManager to start the game!");
            }
        }
        // ----------------------------------

        public void StartMatch(GameMode mode, int rounds, int questions)
        {
            currentMode = mode;
            totalRounds = rounds;
            questionsPerRound = questions;

            CurrentRoundIndex = 0;

            Debug.Log($"Match started | Mode: {mode} | Rounds: {rounds}");
        }
        
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

        public void AdvanceRound()
        {
            CurrentRoundIndex++;
        }
        
        public void OnRoundFinished()
        {
            AdvanceRound();

            if (!IsLastRound())
            {
                // NOTE: Make sure your GameManager actually exists in the scene to avoid NullReferenceErrors here!
                GameManager.Instance.ChangeState(GameState.CategorySelection);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
    }
}