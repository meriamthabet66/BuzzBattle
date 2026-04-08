using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    namespace UI
    {
        public class PlayerInputSectionUI : MonoBehaviour
        {
            [Header("Input")]
            public TMP_InputField nameInput;

            [Header("Display (RTL Fixed)")]
            public TMP_Text nameDisplayText;

            public Button characterButton;
            
            public int playerIndex; // 1, 2, 3, 4

            private int selectedCharacterId = 0;

            public string GetPlayerName()
            {
                // Priority 1: Display text (already Arabic fixed)
                if (!string.IsNullOrEmpty(nameDisplayText.text))
                    return nameDisplayText.text;

                // Priority 2: Raw input (fallback)
                if (!string.IsNullOrEmpty(nameInput.text))
                    return ArabicFixer.Fix(nameInput.text);

                // Default Arabic name with numbering
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
}