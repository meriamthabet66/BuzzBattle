using System.Collections.Generic;
using Data.Data;
using GamePlay.Questions;
using UnityEngine;

namespace UI {
     public class CategoryPanelUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private GameObject nextPanel; 

        [Header("Popup Reference")]
        [SerializeField] private CategoryPopupUI popupUI;

        [Header("Dynamic Scroll View Settings")]
        [SerializeField] private Transform contentContainer; // The Content object of the Main Panel's Scroll View
        [SerializeField] private GameObject selectedCategoryPrefab; // The solid box prefab (needs SelectedCategoryItemUI)
        [SerializeField] private GameObject emptyPlaceholderPrefab; // The dashed box prefab

        private List<Category> localCategories = new List<Category>();
        private QuestionType SelectedQuestionType = QuestionType.MultipleChoice;
        
        [Header("Game Mode UI Buttons")]
        [SerializeField] private ModeBtn MCQBtn;
        [SerializeField] private ModeBtn VerbalBtn;

        private void OnEnable()
        {
            localCategories.Clear(); 
            UpdateGridDisplay();
        }

        public void OnClickAddCategory()
        {
            popupUI.OpenPopup(this, localCategories);
        }

        public void ReceiveSelectedCategories(List<Category> newSelections)
        {
            localCategories = new List<Category>(newSelections);
            UpdateGridDisplay();
        }

        private void UpdateGridDisplay()
        {
            // 1. Delete everything currently inside the scroll view
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Spawn the real, selected categories
            foreach (Category cat in localCategories)
            {
                GameObject newObj = Instantiate(selectedCategoryPrefab, contentContainer);
                newObj.GetComponent<SelectedCategoryItemUI>().Setup(cat);
            }

            // 3. Spawn the dashed placeholders! 
            // If they picked 1 category, we need 3 placeholders. If they picked 4 or more, we need 0.
            int placeholdersNeeded = Mathf.Max(0, 4 - localCategories.Count);
            
            for (int i = 0; i < placeholdersNeeded; i++)
            {
                Instantiate(emptyPlaceholderPrefab, contentContainer);
            }
        }
        
        // --- QUESTION TYPE SELECTION ---
        public void SelectMCQ() {
            SelectedQuestionType = QuestionType.MultipleChoice;
            UpdateModeVisuals();
        }
        
        public void SelectVerbal() {
            SelectedQuestionType = QuestionType.Verbal;
            UpdateModeVisuals();
        }
        
        private void UpdateModeVisuals()
        {
            // SetActive evaluates the condition. If localSelectedMode is Normal, it returns true!
            if (MCQBtn != null && MCQBtn.SelectedOutline != null) 
                MCQBtn.SelectedOutline.SetActive(SelectedQuestionType == QuestionType.MultipleChoice);
            
            if (VerbalBtn != null && VerbalBtn.SelectedOutline != null) 
                VerbalBtn.SelectedOutline.SetActive(SelectedQuestionType == QuestionType.Verbal);
            

        }

        // --- CONTINUE BUTTON ---
        public void OnClickNext()
        {
            if (localCategories.Count == 0)
            {
                Debug.LogWarning("You must select at least 1 category before continuing!");
                return; 
            }

            MatchSetupData.SelectedCategories = new List<Category>(localCategories);
            MatchSetupData.QType = SelectedQuestionType; 
            
            StartGame();
        }

        private void StartGame()
        {
            Managers.MatchManager.Instance.StartMatch(MatchSetupData.Mode, MatchSetupData.Rounds, MatchSetupData.QuestionsPerRound);
            Managers.MatchManager.Instance.StartRound(MatchSetupData.SelectedCategories, MatchSetupData.QType);

            MenuController.Instance.gameObject.SetActive(false);
            MenuController.Instance.OpenPanel(nextPanel);
            GameManager.Instance.ChangeState(GameState.Gameplay);
        }
    }
}