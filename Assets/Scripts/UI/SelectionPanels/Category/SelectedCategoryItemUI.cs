using GamePlay.Questions;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SelectedCategoryItemUI : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro nameText;
        [SerializeField] private Image iconImage;

        public void Setup(Category data)
        {
            if (nameText != null)
                nameText.text = data.categoryName;

            if (iconImage != null && data.categoryIcon != null)
                iconImage.sprite = data.categoryIcon;
        }
    }
}