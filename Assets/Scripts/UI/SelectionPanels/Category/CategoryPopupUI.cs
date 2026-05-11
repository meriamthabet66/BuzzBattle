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

    // --- NEW LOGIC: MERGE LOCAL AND CLOUD ---
    
    // 1. Always load what is on the phone first (Works Offline!)
    List<Category> displayedCategories = Managers.CategoryCloudManager.Instance.GetLocalDownloadedCategories();
    Debug.Log($"Found {displayedCategories.Count} categories offline.");

    // 2. If we have internet, fetch new ones from Supabase
    if (Application.internetReachability != NetworkReachability.NotReachable) 
    {
        List<Category> cloudCategories = await Managers.CategoryCloudManager.Instance.GetCategoryList();
        
        // Add cloud categories to the list, but avoid duplicates
        foreach (var cloudCat in cloudCategories) 
        {
            if (!displayedCategories.Exists(x => x.id == cloudCat.id)) 
            {
                displayedCategories.Add(cloudCat);
            }
        }
    }

    // 3. Fetch Unlocks (only if online)
    List<long> unlockedIds = new List<long>();
    if (Application.internetReachability != NetworkReachability.NotReachable) 
    {
        unlockedIds = await Managers.SupabaseManager.Instance.GetUnlockedCategoryIds();
    }

    // 4. Fill the UI
    foreach (Category cat in displayedCategories) {
        GameObject newObj = Instantiate(categoryItemPrefab, contentContainer);
        CategoryItemUI itemUI = newObj.GetComponent<CategoryItemUI>();

        // Logic: If it's on the disk, it's definitely unlocked!
        bool isDownloaded = Managers.CategoryCloudManager.Instance.IsCategoryDownloaded(cat.id);
        bool isUnlocked = isDownloaded || unlockedIds.Contains(cat.id);
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