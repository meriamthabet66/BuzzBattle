using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance;

    private IGameState currentState;

    private void Awake() {
        if (Instance == null) { 
            Instance = this; 
            }
        else  { 
            Destroy(gameObject); 
            }
    }

    public void ChangeState(IGameState newState) {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    private void Update() {
        currentState?.Update();
    }
}
