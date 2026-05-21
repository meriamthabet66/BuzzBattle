using System.Collections.Generic;
using Data.Data;
using GamePlay.Questions;
using Managers;
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
        [SerializeField] private GameObject backButton;

        private void OnEnable()
        {
            // Instead of clearing the list, we LOAD the saved choices!
            // If it's Round 1, it will be empty. If it's Round 2, it will remember!
            localCategories = new List<Category>(MatchSetupData.SelectedCategories);
            
            // Also remember the Question Type they used last round
            SelectedQuestionType = MatchSetupData.QType;

            UpdateGridDisplay();
            UpdateModeVisuals();
            
            // --- NEW LOGIC: Hide the back button if we are mid-match! ---
            if (backButton != null)
            {
                // If IsMatchActive is TRUE, it hides the button. If FALSE, it shows it!
                bool isMidGame = Managers.MatchManager.Instance != null && Managers.MatchManager.Instance.IsMatchActive;
                backButton.SetActive(!isMidGame);
            }
            
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
        // --- UPDATED: Now ASYNC to handle the download wait ---
        public async void OnClickNext()
        {
            if (localCategories.Count == 0) return; 

            foreach (Category cat in localCategories) {
                if (CategoryCloudManager.Instance.IsCategoryDownloaded(cat.id)) {
                    CategoryCloudManager.Instance.LoadCategoryFromDisk(cat);
                } else {
                    await CategoryCloudManager.Instance.DownloadQuestions(cat);
                    CategoryCloudManager.Instance.SaveCategoryLocally(cat);
                }
            }

            MatchSetupData.SelectedCategories = new List<Category>(localCategories);
            MatchSetupData.QType = SelectedQuestionType; 

            

            // --- THEN START THE MATCH ---
            if (!Managers.MatchManager.Instance.IsMatchActive)
            {
                Managers.MatchManager.Instance.StartMatch(MatchSetupData.Mode, MatchSetupData.Rounds, MatchSetupData.QuestionsPerRound);
            }

            Managers.MatchManager.Instance.StartRound(MatchSetupData.SelectedCategories, MatchSetupData.QType);
            
            
            // --- THE FIX: CHANGE STATE FIRST ---
            // This turns on the Gameplay Canvas so it's ready to receive data
            GameManager.Instance.ChangeState(GameState.Gameplay);
        }
    }
}