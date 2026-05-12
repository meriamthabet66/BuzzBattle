using System.Collections;
using UnityEngine;
using Managers;
using Data.Data;

namespace UI
{
    public class GameOverlayMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform menuPanelRect; 
        [SerializeField] private float slideDuration = 0.3f;  

        [Header("Responsive Targets (Drag Empty GameObjects Here)")]
        [SerializeField] private RectTransform closedTarget; // The position when hidden at the bottom
        [SerializeField] private RectTransform openTarget;   // The position when dead center

        private bool isOpen = false;
        private Coroutine autoDropCoroutine;

        private void Start()
        {
            // Instantly snap to the closed position on boot
            menuPanelRect.position = closedTarget.position;
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (autoDropCoroutine != null) StopCoroutine(autoDropCoroutine);

            if (state == GameState.Results)
            {
                autoDropCoroutine = StartCoroutine(AutoOpenRoutine());
            }
            else if (state == GameState.Menu || state == GameState.Setup || state == GameState.CategorySelection)
            {
                Time.timeScale = 1f;
                if (isOpen) 
                {
                    isOpen = false;
                    menuPanelRect.position = closedTarget.position;
                }
            }
        }

        private IEnumerator AutoOpenRoutine()
        {
            yield return new WaitForSeconds(5.0f);
            if (!isOpen) ToggleMenu(); 
        }

        public void ToggleMenu()
        {
            isOpen = !isOpen;
            StopAllCoroutines(); 
            
            RectTransform target = isOpen ? openTarget : closedTarget;
            StartCoroutine(SlideRoutine(target.position));

            // --- THE FIX: Pause EVERYTHING instantly. No exceptions! ---
            Time.timeScale = isOpen ? 0f : 1f; 
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f; 
            isOpen = false;
            menuPanelRect.position = closedTarget.position;

            MatchManager.Instance.CancelMatch();
            MatchSetupData.ResetData();
            GameManager.Instance.ChangeState(GameState.Menu);
        }

        public void OnClickPlayAgain()
        {
            Time.timeScale = 1f; 
            isOpen = false;
            menuPanelRect.position = closedTarget.position;

            MatchManager.Instance.CancelMatch();

            foreach (var player in PlayerManager.Instance.Players) player.IsEliminated = false;
            PlayerManager.Instance.ResetScoresForRematch();

            GameManager.Instance.ChangeState(GameState.CategorySelection);
        }

        private IEnumerator SlideRoutine(Vector3 targetWorldPos)
        {
            Vector3 startPos = menuPanelRect.position;
            float time = 0;

            while (time < slideDuration)
            {
                time += Time.unscaledDeltaTime; 
                menuPanelRect.position = Vector3.Lerp(startPos, targetWorldPos, time / slideDuration);
                yield return null;
            }
            
            menuPanelRect.position = targetWorldPos;
        }
    }
}