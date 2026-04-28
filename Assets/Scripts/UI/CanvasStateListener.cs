using UnityEngine;

namespace UI
{
    public class CanvasStateListener : MonoBehaviour
    {
        [Header("When should this Canvas be ON?")]
        public GameState[] activeStates;

        private void Awake()
        {
            // Subscribe to the STATIC event. It will never let go, even if disabled!
            GameManager.OnStateChanged += HandleStateChanged;
        }

        private void Start()
        {
            // Run it once on start to ensure it sets the correct visibility on boot
            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            // Only stop listening if the Canvas is actually deleted from the game
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState newState)
        {
            bool shouldBeActive = false;
            foreach (GameState state in activeStates)
            {
                if (state == newState)
                {
                    shouldBeActive = true;
                    break;
                }
            }
            
            // Turn on/off
            gameObject.SetActive(shouldBeActive);
        }
    }
}