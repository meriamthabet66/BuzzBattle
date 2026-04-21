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

        private int localRounds = 3;
        private int localQuestions = 10;

        private void OnEnable()
        {
            // Every time this panel opens, update the text to match the values
            UpdateText();
        }

        // --- BUTTON METHODS (Hook these to your + and - buttons) ---

        public void IncreaseRounds() { localRounds++; UpdateText(); }
        public void DecreaseRounds() { localRounds = Mathf.Max(1, localRounds - 1); UpdateText(); }

        public void IncreaseQuestions() { localQuestions++; UpdateText(); }
        public void DecreaseQuestions() { localQuestions = Mathf.Max(1, localQuestions - 1); UpdateText(); }

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