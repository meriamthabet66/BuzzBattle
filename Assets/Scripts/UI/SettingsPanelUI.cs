using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using GamePlay.Questions;

namespace UI.Panels
{
    public class SettingsPanelUI : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Timer Value Texts")]
        [SerializeField] private TMP_Text buzzTimeText;
        [SerializeField] private TMP_Text mcqTimeText;
        [SerializeField] private TMP_Text verbalTimeText;

        private void OnEnable()
        {
            // 1. Sync Sliders
            if (musicSlider != null) {
                musicSlider.value = SettingsManager.Instance.MusicVolume;
                musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);
            }
            if (sfxSlider != null) {
                sfxSlider.value = SettingsManager.Instance.SfxVolume;
                sfxSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSfxVolume);
            }

            // 2. Sync Timer Texts
            RefreshTimerUI();
        }

        private void OnDisable()
        {
            if (musicSlider != null) musicSlider.onValueChanged.RemoveAllListeners();
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveAllListeners();
        }

        public void RefreshTimerUI()
        {
            if (buzzTimeText != null) buzzTimeText.text = QuestionRules.BuzzTimeLimit.ToString();
            if (mcqTimeText != null) mcqTimeText.text = QuestionRules.MultipleChoiceTime.ToString();
            if (verbalTimeText != null) verbalTimeText.text = QuestionRules.VerbalTime.ToString();
        }

        // --- Timer Button Methods (Connect these to your + and - buttons) ---

        public void ChangeBuzz(int amount) {
            int newVal = Mathf.Clamp(QuestionRules.BuzzTimeLimit + amount, 3, 10);
            SettingsManager.Instance.UpdateBuzzTimer(newVal);
            RefreshTimerUI();
        }

        public void ChangeMCQ(int amount) {
            int newVal = Mathf.Clamp(QuestionRules.MultipleChoiceTime + amount, 3, 15);
            SettingsManager.Instance.UpdateMCQTimer(newVal);
            RefreshTimerUI();
        }

        public void ChangeVerbal(int amount) {
            int newVal = Mathf.Clamp(QuestionRules.VerbalTime + amount, 5, 30);
            SettingsManager.Instance.UpdateVerbalTimer(newVal);
            RefreshTimerUI();
        }
        
        
        

        // --- NEW: Force the UI to match the defaults ---
        public void ResetUI()
        {
            // Update sliders
            if (musicSlider != null) musicSlider.value = 1.0f; // Default 100%
            if (sfxSlider != null) sfxSlider.value = 1.0f;

            // Update timer texts
            RefreshTimerUI();
            
            Debug.Log("<color=red>Settings UI: Sliders and Texts reset.</color>");
        }
    }
}