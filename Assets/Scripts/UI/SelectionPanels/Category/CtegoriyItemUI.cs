using GamePlay.Questions;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;

namespace UI {
    public class CategoryItemUI : MonoBehaviour
    {
        [Header("Main UI References")]
        [SerializeField] private RTLTextMeshPro categoryNameText; 
        [SerializeField] private Image iconImage;               
        [SerializeField] private GameObject selectedOutline;    

        [Header("State Overlays")]
        [SerializeField] private GameObject lockedOverlay;     // The container with the lock and price
        [SerializeField] private TMP_Text priceText;           
        [SerializeField] private Button downloadBtn;           

        public Category CategoryData { get; private set; }
        private CategoryPopupUI parentPopup;
        
        private bool isUnlocked = false;
        private bool isDownloaded = false;

        public void SetupWithState(Category data, CategoryPopupUI popup, bool unlocked, bool downloaded, bool alreadySelected) {
            this.CategoryData = data;
            this.parentPopup = popup;
            this.isUnlocked = unlocked;
            this.isDownloaded = downloaded;

            if (categoryNameText != null) categoryNameText.text = data.categoryName;
            if (iconImage != null && data.categoryIcon != null) iconImage.sprite = data.categoryIcon;
            if (priceText != null) priceText.text = data.price.ToString();

            RefreshVisuals(alreadySelected);
        }

        private void RefreshVisuals(bool alreadySelected) {
            // 1. If not unlocked, show the locked overlay (which has your Unlock Button)
            if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);

            // 2. If unlocked but not downloaded, show the download button
            if (downloadBtn != null) downloadBtn.gameObject.SetActive(isUnlocked && !isDownloaded);

            // 3. If ready, show selection outline
            if (selectedOutline != null) {
                selectedOutline.SetActive(alreadySelected && isUnlocked && isDownloaded);
            }
        }

        // --- BUTTON: CLICK THE WHOLE BOX ---
        public void OnItemClicked() {
            // Only allow selection if it is fully ready
            if (isUnlocked && isDownloaded) {
                bool isCurrentlySelected = selectedOutline.activeSelf;
                selectedOutline.SetActive(!isCurrentlySelected);
                parentPopup.OnCategoryToggled(CategoryData, !isCurrentlySelected);
            }
        }

        // --- BUTTON: THE LOCKED OVERLAY (Unlock Button) ---
        public async void OnUnlockClicked() {
            bool success = await SupabaseManager.Instance.UnlockCategory(CategoryData.id, CategoryData.price);
            if (success) {
                isUnlocked = true;
                RefreshVisuals(false);
                Debug.Log("Category Unlocked!");
            }
        }

        // --- BUTTON: THE DOWNLOAD ARROW ---
        public async void OnDownloadClicked() {
            downloadBtn.interactable = false;
            
            await CategoryCloudManager.Instance.DownloadQuestions(CategoryData);
            CategoryCloudManager.Instance.SaveCategoryLocally(CategoryData);

            isDownloaded = true;
            RefreshVisuals(false);
            Debug.Log("Download Complete!");
        }
    }
}