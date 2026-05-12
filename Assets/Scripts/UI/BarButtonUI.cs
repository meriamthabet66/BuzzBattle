using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class BarButtonUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Sprites")]
        [SerializeField] private Sprite activeSprite;   // The "Filled" orange version
        [SerializeField] private Sprite inactiveSprite; // The "Outline" white/gray version

        // [Header("Colors")]
        // [SerializeField] private Color activeColor = new Color(0.92f, 0.39f, 0.12f); // EB6520
        // [SerializeField] private Color inactiveColor = Color.white;

        // This is called by the BarMenuHandler
        public void SetState(bool isActive)
        {
            // 1. Swap the Sprite
            if (iconImage != null)
            {
                iconImage.sprite = isActive ? activeSprite : inactiveSprite;
            }

            // // 2. Change Text Color
            // if (labelText != null)
            // {
            //     labelText.color = isActive ? activeColor : inactiveColor;
            // }
        }
    }
}