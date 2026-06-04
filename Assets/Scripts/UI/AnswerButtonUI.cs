using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using GamePlay.Questions;

namespace UI 
{
    public class AnswerButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text answerText;
        [SerializeField] private Button button;
        
        private AnswerOption currentOption;
        private Action<AnswerOption> onButtonClicked;
        
        private ColorBlock defaultColors;

        private void Awake()
        {
            if (button != null) defaultColors = button.colors;
        }

        public void Setup(AnswerOption option, Action<AnswerOption> callback)
        {
            currentOption = option;
            onButtonClicked = callback;
            answerText.text = ArabicFixer.Fix(option.text);

            button.interactable = true;
            
            // Define your colors
            Color wrongColor = new Color(0.9215f, 0.3960f, 0.1254f, 1.0f); // Orange
            Color rightColor = new Color32(39, 116, 37, 255);             // Green
            
            Color targetColor = currentOption.isCorrect ? rightColor : wrongColor;
            
            ColorBlock cb = defaultColors; 
            
            cb.pressedColor = targetColor;  
            cb.selectedColor = targetColor; 
            cb.disabledColor = targetColor; 
            
            // --- THE FIX: Make the correct answer glow brighter! ---
            if (currentOption.isCorrect)
            {
                // This makes the green 50% brighter than normal
                cb.colorMultiplier = 2.0f; 
            }
            else
            {
                // Leave the orange at normal brightness
                cb.colorMultiplier = 1.0f; 
            }
            
            button.colors = cb;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
        private void HandleClick() 
        {
            // When clicked, the button freezes in the 'disabledColor' we set in Setup!
            button.interactable = false;
            
            onButtonClicked?.Invoke(currentOption);
        }
    }
}