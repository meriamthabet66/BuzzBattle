using UnityEngine;
using GamePlay.Questions;

namespace Managers
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadAllSettings();
        }

        // --- AUDIO ---
        public void SetMusicVolume(float volume) {
            MusicVolume = volume;
            PlayerPrefs.SetFloat("MusicVol", volume);
            PlayerPrefs.Save();
            // TODO: Update your AudioSource.volume here
        }

        public void SetSfxVolume(float volume) {
            SfxVolume = volume;
            PlayerPrefs.SetFloat("SfxVol", volume);
            PlayerPrefs.Save();
        }

        // --- TIMERS ---
        public void UpdateMCQTimer(int seconds) {
            QuestionRules.MultipleChoiceTime = seconds;
            PlayerPrefs.SetInt("Timer_MCQ", seconds);
            PlayerPrefs.Save();
        }

        public void UpdateVerbalTimer(int seconds) {
            QuestionRules.VerbalTime = seconds;
            PlayerPrefs.SetInt("Timer_Verbal", seconds);
            PlayerPrefs.Save();
        }

        public void UpdateBuzzTimer(int seconds) {
            QuestionRules.BuzzTimeLimit = seconds;
            PlayerPrefs.SetInt("Timer_Buzz", seconds);
            PlayerPrefs.Save();
        }

        private void LoadAllSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVol", 1.0f);
            SfxVolume = PlayerPrefs.GetFloat("SfxVol", 1.0f);

            QuestionRules.MultipleChoiceTime = PlayerPrefs.GetInt("Timer_MCQ", 5);
            QuestionRules.VerbalTime = PlayerPrefs.GetInt("Timer_Verbal", 10);
            QuestionRules.BuzzTimeLimit = PlayerPrefs.GetInt("Timer_Buzz", 5);
        }
        
        
        


        public void ResetAllSettings()
        {
            // 1. Delete the saved PlayerPrefs
            PlayerPrefs.DeleteKey("MusicVol");
            PlayerPrefs.DeleteKey("SfxVol");
            PlayerPrefs.DeleteKey("Timer_MCQ");
            PlayerPrefs.DeleteKey("Timer_Verbal");
            PlayerPrefs.DeleteKey("Timer_Buzz");
            PlayerPrefs.Save();

            // 2. Reset the variables in the code
            LoadAllSettings(); // This will reload the default values (1.0f, 5s, 10s, etc.)
            
            Debug.Log("<color=red>SettingsManager: All settings reset to defaults.</color>");
        }
    }
}