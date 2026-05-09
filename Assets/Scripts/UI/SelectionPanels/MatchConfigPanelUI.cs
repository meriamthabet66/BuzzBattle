using UnityEngine;
using TMPro;
using Data.Data; // Ensure this matches your MatchSetupData
using UI; 

namespace UI.Panels
{
    public class MatchConfigPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text roundsText;
        [SerializeField] private TMP_Text questionsText;

        [Header("Rounds Section Container")]
        [Tooltip("Put the Rounds Text, +/-, and Label into an empty GameObject and drag it here")]
        [SerializeField] private GameObject roundsUIContainer; 

        [Header("Next Panel Route")]
        [SerializeField] private GameObject nextPanel; 

        private int localRounds = 3;
        private int localQuestions = 10;

        private void OnEnable()
        {
            // --- NEW: Hide the Rounds UI if it's a Tournament! ---
            if (roundsUIContainer != null)
            {
                bool isTournament = (MatchSetupData.Mode == GameMode.Tournament);
                
                // Hide it if tournament, show it if normal
                roundsUIContainer.SetActive(!isTournament);
            }

            UpdateText();
        }

        // --- BUTTON METHODS ---

        public void IncreaseRounds() {
            localRounds = Mathf.Clamp(localRounds + 1, 1, 5); UpdateText();
        }

        public void DecreaseRounds() {
            localRounds = Mathf.Clamp(localRounds - 1, 1, 5); UpdateText();
        }

        public void IncreaseQuestions() {
            localQuestions = Mathf.Clamp(localQuestions + 1, 1, 15); UpdateText();
        }

        public void DecreaseQuestions() {
            localQuestions = Mathf.Clamp(localQuestions - 1, 1, 15); UpdateText();
        }

        private void UpdateText()
        {
            if (roundsText != null) roundsText.text = localRounds.ToString();
            if (questionsText != null) questionsText.text = localQuestions.ToString();
        }

        // --- NAVIGATION ---
        public void OnClickNext()
        {
            // Save data
            MatchSetupData.Rounds = localRounds;
            MatchSetupData.QuestionsPerRound = localQuestions;

            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}