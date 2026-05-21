using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;
using System.Threading.Tasks;
using Managers;

namespace UI {
    public class CategoryPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentContainer; 
        [SerializeField] private GameObject categoryItemPrefab; 

        private CategoryPanelUI parentPanel;
        private List<Category> temporarySelections = new List<Category>();
        
        

        // Update the OpenPopup method in CategoryPopupUI.cs

        public void OpenPopup(CategoryPanelUI parent, List<Category> currentlySelected) {
            parentPanel = parent;
            temporarySelections = new List<Category>(currentlySelected);
            
            // SHOW THE POPUP INSTANTLY
            gameObject.SetActive(true);

            // 1. Clear UI
            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            // 2. LOAD LOCAL (Instant)
            List<Category> displayedCategories = CategoryCloudManager.Instance.GetLocalDownloadedCategories();
            
            // 3. GET CLOUD DATA FROM CACHE (Instant - no 'await'!)
            List<Category> cloudCategories = SupabaseManager.Instance.CachedCloudCategories;
            List<long> unlockedIds = SupabaseManager.Instance.CachedUnlockedIds;

            // 4. MERGE (Avoid duplicates)
            if (cloudCategories != null)
            {
                foreach (var cloudCat in cloudCategories) {
                    if (!displayedCategories.Exists(x => x.id == cloudCat.id)) {
                        displayedCategories.Add(cloudCat);
                    }
                }
            }

            // 5. SPAWN UI ITEMS
            foreach (Category cat in displayedCategories) {
                GameObject newObj = Instantiate(categoryItemPrefab, contentContainer);
                CategoryItemUI itemUI = newObj.GetComponent<CategoryItemUI>();

                bool isDownloaded = CategoryCloudManager.Instance.IsCategoryDownloaded(cat.id);
                // If on disk, it's unlocked. Otherwise check the cloud cache.
                bool isUnlocked = isDownloaded || (unlockedIds != null && unlockedIds.Contains(cat.id));
                bool isAlreadySelected = temporarySelections.Exists(x => x.id == cat.id);

                itemUI.SetupWithState(cat, this, isUnlocked, isDownloaded, isAlreadySelected);
            }
        }
        public void OnCategoryToggled(Category cat, bool isSelected)
        {
            if (isSelected)
            {
                if (!temporarySelections.Exists(x => x.id == cat.id))
                    temporarySelections.Add(cat);
            }
            else
            {
                temporarySelections.RemoveAll(x => x.id == cat.id);
            }
        }

        public void OnConfirmClicked()
        {
            parentPanel.ReceiveSelectedCategories(temporarySelections);
            gameObject.SetActive(false); 
        }
    }
}