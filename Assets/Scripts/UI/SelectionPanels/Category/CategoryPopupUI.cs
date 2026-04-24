using System.Collections.Generic;
using GamePlay.Questions;
using UnityEngine;

namespace UI {
    public class CategoryPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentContainer; 
        [SerializeField] private GameObject categoryItemPrefab; 
        [SerializeField] private CategoryDatabase database; 

        private CategoryPanelUI parentPanel;
        private List<Category> temporarySelections = new List<Category>();

        public void OpenPopup(CategoryPanelUI parent, List<Category> currentlySelected)
        {
            parentPanel = parent;
            temporarySelections = new List<Category>(currentlySelected);
            PopulateList();
            gameObject.SetActive(true);
        }

        private void PopulateList()
        {
            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            foreach (Category cat in database.categories)
            {
                GameObject newObj = Instantiate(categoryItemPrefab, contentContainer);
                CategoryItemUI itemUI = newObj.GetComponent<CategoryItemUI>();

                bool isAlreadySelected = temporarySelections.Contains(cat);
                itemUI.Setup(cat, this, isAlreadySelected);
            }
        }

        public void OnCategoryToggled(Category cat, bool isSelected)
        {
            // NO LIMITS! Just add or remove from the list
            if (isSelected && !temporarySelections.Contains(cat))
            {
                temporarySelections.Add(cat);
            }
            else if (!isSelected && temporarySelections.Contains(cat))
            {
                temporarySelections.Remove(cat);
            }
        }

        public void OnConfirmClicked()
        {
            parentPanel.ReceiveSelectedCategories(temporarySelections);
            gameObject.SetActive(false); 
        }
    }
}