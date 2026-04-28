using System;
using GamePlay.Questions;
using UnityEngine;


//this class is responsible for managing the game so it all comes together 
public class GameManager : MonoBehaviour
{
    //we make it an instance and static so it can be used by any class
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public bool IsVoiceEnabled { get; private set; }

    public static event Action<GameState> OnStateChanged;

    [SerializeField] private CategoryDatabase categoryDatabase;

    public CategoryDatabase CategoryDatabase => categoryDatabase;
    
<<<<<<< HEAD
=======
    public static event Action OnGameplayStart;
    
>>>>>>> 1e5fdd180fcec37ecb4a88b8147f9106d99a8006
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // FORCE the event to fire on boot up so all UI canvases know what to do!
        CurrentState = GameState.Menu;
        EnterState(CurrentState);
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log($"Game Booted! GameState is: {CurrentState}");
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);

        OnStateChanged?.Invoke(CurrentState);
        Debug.Log($"GameState changed to: {CurrentState}");
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                break;

            case GameState.Setup:
                break;

            case GameState.CategorySelection:
                break;

            case GameState.MiniGame:
                break;

            case GameState.Gameplay:
                // --- NEW: Fire the event! ---
                OnGameplayStart?.Invoke();
                break;

            case GameState.Steal:
                break;

            case GameState.Results:
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                break;

            case GameState.Setup:
                break;

            case GameState.CategorySelection:
                break;

            case GameState.MiniGame:
                break;

            case GameState.Gameplay:
                break;

            case GameState.Steal:
                break;

            case GameState.Results:
                break;
        }
    }

    [ContextMenu("Print Categories")]
    private void DebugPrintCategories()
    {
        foreach (var cat in categoryDatabase.categories)
        {
            Debug.Log("Category: " + cat.categoryName);
        }
    }
}