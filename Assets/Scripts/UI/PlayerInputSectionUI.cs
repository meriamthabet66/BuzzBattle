using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI 
{
    public class PlayerInputSectionUI : MonoBehaviour
    {
        [Header("Input")]
        public TMP_InputField nameInput;

        [Header("Display (RTL Fixed)")]
        public RTLTextMeshPro nameDisplayText; // RTLInputDisplay handles this visually!

        public Button characterButton;
        public int playerIndex; 
        private int selectedCharacterId = 0;

        public string GetPlayerName()
        {
            // --- THE FIX: Return RAW text! ---
            // Do NOT use ArabicFixer here anymore, because the Gameplay Buzzer is using RTLTMPro!
            if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
            {
                return nameInput.text; // Send raw text
            }

            // Default raw Arabic name
            return $"لاعب {playerIndex}";
        }

        public int GetCharacterId()
        {
            return selectedCharacterId;
        }

        public void SetCharacter(int characterId)
        {
            selectedCharacterId = characterId;
        }
    }
}