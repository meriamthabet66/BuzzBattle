using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;
using System.Threading.Tasks;

namespace UI {
    public class CategoryPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentContainer; 
        [SerializeField] private GameObject categoryItemPrefab; 

        private CategoryPanelUI parentPanel;
        private List<Category> temporarySelections = new List<Category>();

        // Update the OpenPopup method in CategoryPopupUI.cs

        public async void OpenPopup(CategoryPanelUI parent, List<Category> currentlySelected) {
            parentPanel = parent;
            temporarySelections = new List<Category>(currentlySelected);
            gameObject.SetActive(true);

            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            // 1. Get ALL categories from Supabase (including current Cloud versions)
            List<Category> cloudCategories = new List<Category>();
            if (Application.internetReachability != NetworkReachability.NotReachable) {
                cloudCategories = await Managers.CategoryCloudManager.Instance.GetCategoryList();
            }

            // 2. Get the player's UNLOCKS from Supabase
            List<long> unlockedIds = new List<long>();
            if (Application.internetReachability != NetworkReachability.NotReachable) {
                unlockedIds = await Managers.SupabaseManager.Instance.GetUnlockedCategoryIds();
            }

            // 3. Loop through the CLOUD categories to check for updates
            foreach (Category cloudCat in cloudCategories) {
                GameObject newObj = Instantiate(categoryItemPrefab, contentContainer);
                CategoryItemUI itemUI = newObj.GetComponent<CategoryItemUI>();

                int localVer = Managers.CategoryCloudManager.Instance.GetLocalVersion(cloudCat.id);
                bool fileExists = Managers.CategoryCloudManager.Instance.IsCategoryDownloaded(cloudCat.id);
        
                // --- LOG THE COMPARISON ---
                Debug.Log($"Category: {cloudCat.categoryName} | Cloud Ver: {cloudCat.version} | Local Ver: {localVer}");

                // If local is -1 (no file) or local is less than cloud, it is NOT up to date
                bool isUpToDate = fileExists && (localVer >= cloudCat.version);

                bool isDownloaded = isUpToDate; 
                bool isUnlocked = isDownloaded || unlockedIds.Contains(cloudCat.id);
                bool isAlreadySelected = temporarySelections.Exists(x => x.id == cloudCat.id);

                itemUI.SetupWithState(cloudCat, this, isUnlocked, isDownloaded, isAlreadySelected);
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