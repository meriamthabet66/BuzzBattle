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
        
        [Header("Warning Popups")]
        [SerializeField] private GameObject starWarningPopup;
        
        
        private void OnDisable()
        {
            // If the popup was left open, force it to close!
            if (starWarningPopup != null && starWarningPopup.activeSelf)
            {
                starWarningPopup.SetActive(false);
            }
        }
        

        // Update the OpenPopup method in CategoryPopupUI.cs

        public async void OpenPopup(CategoryPanelUI parent, List<Category> currentlySelected) {
    parentPanel = parent;
    temporarySelections = new List<Category>(currentlySelected);
    gameObject.SetActive(true);

    // 1. Clear current UI
    foreach (Transform child in contentContainer) Destroy(child.gameObject);

    // 2. Start with Local Categories (Offline Support)
    List<Category> displayedCategories = Managers.CategoryCloudManager.Instance.GetLocalDownloadedCategories();
    Debug.Log($"<color=cyan>Popup: Found {displayedCategories.Count} categories locally.</color>");

    // 3. Check for Cloud Data
    if (Application.internetReachability != NetworkReachability.NotReachable) 
    {
        Debug.Log("Lobby: Internet detected. Fetching cloud categories...");
        
        // Try to get from Cache first
        List<Category> cloudCategories = Managers.SupabaseManager.Instance.CachedCloudCategories;

        // If Cache is empty, do a forced live fetch
        if (cloudCategories == null || cloudCategories.Count == 0)
        {
            Debug.Log("Lobby: Cache empty, performing live fetch...");
            cloudCategories = await Managers.CategoryCloudManager.Instance.GetCategoryList();
        }

        List<long> unlockedIds = Managers.SupabaseManager.Instance.CachedUnlockedIds;

        // Merge cloud categories into the list
        if (cloudCategories != null)
        {
            foreach (var cloudCat in cloudCategories) 
            {
                if (!displayedCategories.Exists(x => x.id == cloudCat.id)) 
                {
                    displayedCategories.Add(cloudCat);
                }
            }
        }
        else
        {
            Debug.LogError("Lobby: Cloud fetch returned NULL. Check Supabase 'categories' table.");
        }
    }

    // 4. Final Safety Check: If the list is STILL empty
    if (displayedCategories.Count == 0)
    {
        Debug.LogWarning("Popup: No categories found either locally or in cloud.");
        return;
    }

    // 5. Spawn the items
    foreach (Category cat in displayedCategories) {
        GameObject newObj = Instantiate(categoryItemPrefab, contentContainer);
        CategoryItemUI itemUI = newObj.GetComponent<CategoryItemUI>();

        bool isDownloaded = Managers.CategoryCloudManager.Instance.IsCategoryDownloaded(cat.id);
        
        // Use the Cache for unlocked status if available
        bool isUnlocked = isDownloaded || (Managers.SupabaseManager.Instance.CachedUnlockedIds != null && 
                          Managers.SupabaseManager.Instance.CachedUnlockedIds.Contains(cat.id));
        
        bool isAlreadySelected = temporarySelections.Exists(x => x.id == cat.id);

        itemUI.SetupWithState(cat, this, isUnlocked, isDownloaded, isAlreadySelected, starWarningPopup);
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