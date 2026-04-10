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

        public void Setup(AnswerOption option, Action<AnswerOption> callback)
        {
            currentOption = option;
            onButtonClicked = callback;
            answerText.text = ArabicFixer.Fix(option.text);

            // Clear old listeners so we don't trigger answers from the previous question!
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            onButtonClicked?.Invoke(currentOption);
        }
    }
}