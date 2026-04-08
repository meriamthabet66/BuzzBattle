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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

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
                GameManager.Instance.ChangeState(GameState.CategorySelection);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Results);
            }
        }
    }
}