using UnityEngine;
using TMPro;
using UI;

namespace UI {


    public class FixTextOnStart : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;

        void Start()
        {
            string raw = textComponent.text;

            string fixedText = ArabicFixer.Fix(raw);

            textComponent.text = fixedText;
        }
    }
}