using RTLTMPro;
using UnityEngine;

namespace UI {
    using UnityEngine;
    using TMPro;

    public class RTLInputDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RTLTextMeshPro displayText;

        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(UpdateDisplay);
        }

        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(UpdateDisplay);
        }

        void UpdateDisplay(string raw)
        {
            displayText.text = raw;
        }
    }
}