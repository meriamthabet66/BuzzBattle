using Data.Data;
using TMPro;
using UnityEngine;

namespace UI {
    public class MatchConfigPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text roundsText;
        [SerializeField] private TMP_Text questionsText;

        [Header("Next Panel Route")]
        [SerializeField] private GameObject categoryPanel; // Drag the CategoryPanel GameObject here in Unity!

        private int localRounds = 5;
        private int localQuestions = 15;

        private void OnEnable()
        {
            // Every time this panel opens, update the text to match the values
            UpdateText();
        }

        // --- BUTTON METHODS (Hook these to your + and - buttons) ---

        // --- ROUNDS (Max 5) ---
        public void IncreaseRounds()
        {
            // Clamps the number between 1 and 5!
            localRounds = Mathf.Clamp(localRounds + 1, 1, 5);
            UpdateText();
        }

        public void DecreaseRounds()
        {
            localRounds = Mathf.Clamp(localRounds - 1, 1, 5);
            UpdateText();
        }

        // --- QUESTIONS (Max 15) ---
        public void IncreaseQuestions()
        {
            // Clamps the number between 1 and 15!
            localQuestions = Mathf.Clamp(localQuestions + 1, 1, 15);
            UpdateText();
        }

        public void DecreaseQuestions()
        {
            localQuestions = Mathf.Clamp(localQuestions - 1, 1, 15);
            UpdateText();
        }

        private void UpdateText()
        {
            roundsText.text = localRounds.ToString();
            questionsText.text = localQuestions.ToString();
        }

        // --- NAVIGATION (Hook this to your Next/Continue button) ---
        public void OnClickNext()
        {
            // 1. Save this panel's logic into the central bucket
            MatchSetupData.Rounds = localRounds;
            MatchSetupData.QuestionsPerRound = localQuestions;

            // 2. Tell the Menu Controller to open the next specific panel!
            MenuController.Instance.OpenPanel(categoryPanel);
        }

        // Hook this to your `<` Back button
        public void OnClickBack()
        {
            MenuController.Instance.GoBack();
        }
    }
}