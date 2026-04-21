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

            // 1. Reset the button to its standard state
            button.interactable = true;
            
            // 2. Define the colors
            Color wrongColor = new Color(0.9215f, 0.3960f, 0.1254f, 1.0f); // Orange
            Color rightColor = new Color32(39, 116, 37, 255);             // Green
            
            Color targetColor = currentOption.isCorrect ? rightColor : wrongColor;
            
            // 3. Update the ColorBlock for ALL states
            ColorBlock cb = defaultColors; // Start with a fresh set of defaults
            
            cb.pressedColor = targetColor;  // Color while finger is down
            cb.selectedColor = targetColor; // Color after you let go
            cb.disabledColor = targetColor; // Color when interactable = false (THIS fixes the visibility!)
            
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