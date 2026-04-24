using GamePlay.Questions;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public enum CategoryState { ReadyToPlay, NeedsDownload, Locked }

    public class CategoryItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RTLTextMeshPro categoryNameText;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedOutline; // The orange border!
        
        [Header("Status Overlays")]
        [SerializeField] private GameObject lockedOverlay; // Shows the lock icon/price
        [SerializeField] private GameObject downloadOverlay; // Shows the cloud/download icon

        public Category CategoryData { get; private set; }
        private bool isSelected = false;
        private CategoryPopupUI parentPopup;
        private CategoryState currentState;

        public void Setup(Category data, CategoryPopupUI popup, bool alreadySelected)
        {
            CategoryData = data;
            parentPopup = popup;
            
            // --- FIXED: Apply the Icon and Name! ---
            if (categoryNameText != null)
                categoryNameText.text = data.categoryName;

            if (iconImage != null && data.categoryIcon != null)
                iconImage.sprite = data.categoryIcon;
            // ---------------------------------------

            // TODO: In the future, check your Save System here!
            // Example: if (!PlayerStats.HasUnlocked(data.id)) currentState = CategoryState.Locked;
            // For now, let's assume they are all ready to play:
            currentState = CategoryState.ReadyToPlay; 

            UpdateVisualState();
            
            // Set initial selection
            isSelected = alreadySelected;
            if (selectedOutline != null) selectedOutline.SetActive(isSelected);
        }

        // Called when the player taps this specific category row
        public void OnClick()
        {
            if (currentState == CategoryState.Locked)
            {
                Debug.Log("Show Buy Prompt: Purchase for 20 points?");
                return; // Stop them from selecting it
            }
            else if (currentState == CategoryState.NeedsDownload)
            {
                Debug.Log("Start downloading from server...");
                return; // Stop them from selecting until downloaded
            }

            // If it's ready to play, toggle selection!
            isSelected = !isSelected;
            if (selectedOutline != null) selectedOutline.SetActive(isSelected);
            
            // Tell the popup we changed our state
            parentPopup.OnCategoryToggled(CategoryData, isSelected);
        }

        private void UpdateVisualState()
        {
            if (lockedOverlay != null) lockedOverlay.SetActive(currentState == CategoryState.Locked);
            if (downloadOverlay != null) downloadOverlay.SetActive(currentState == CategoryState.NeedsDownload);
        }
    }
}