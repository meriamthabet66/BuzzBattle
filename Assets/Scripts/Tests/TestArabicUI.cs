using UnityEngine;
using TMPro;
using UI;

namespace Tests
{
    public class TestArabicUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text textDisplay;

        public void OnTextChanged()
        {
            string raw = inputField.text;

            Debug.Log("Raw: " + raw);

            string fixedText = ArabicFixer.Fix(raw);

            Debug.Log("Fixed: " + fixedText);

            textDisplay.text = fixedText;
        }
    }
}