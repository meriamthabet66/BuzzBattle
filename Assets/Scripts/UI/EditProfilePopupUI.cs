using Managers;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class EditProfilePopupUI : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Buttons")]
        [SerializeField] private Button saveChangesBtn;
        [SerializeField] private Button deleteAccountBtn; // This now opens the warning
        [SerializeField] private Button closePopupBtn;

        // --- NEW: Delete Warning UI ---
        [Header("Delete Confirmation Popup")]
        [SerializeField] private GameObject deleteWarningPanel; // Drag the warning panel here
        [SerializeField] private Button confirmDeleteBtn;      // The "Yes, Delete" button
        [SerializeField] private Button cancelDeleteBtn;       // The "No, Cancel" button
        // ------------------------------

        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro statusText;

        private string originalUsername;
        private string originalEmail;

        private void OnEnable()
        {
            if (statusText != null) statusText.text = "";
            if (deleteWarningPanel != null) deleteWarningPanel.SetActive(false); // Ensure warning is hidden
            
            if (LocalAccountManager.Instance.SavedAccount != null)
            {
                originalUsername = LocalAccountManager.Instance.SavedAccount.Username;
                originalEmail = LocalAccountManager.Instance.SavedAccount.email;

                if (usernameInput != null) usernameInput.text = originalUsername;
                if (emailInput != null) emailInput.text = originalEmail;
            }
            if (passwordInput != null) passwordInput.text = "";
            if (saveChangesBtn != null) saveChangesBtn.interactable = false;
        }
        private void OnDisable()
        {
            // If the popup was left open, force it to close!
            if (deleteWarningPanel != null && deleteWarningPanel.activeSelf)
            {
                deleteWarningPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (usernameInput != null) usernameInput.onValueChanged.AddListener(CheckForChanges);
            if (emailInput != null) emailInput.onValueChanged.AddListener(CheckForChanges);
            if (passwordInput != null) passwordInput.onValueChanged.AddListener(CheckForChanges);

            if (saveChangesBtn != null) saveChangesBtn.onClick.AddListener(OnSaveChangesClicked);
            if (closePopupBtn != null) closePopupBtn.onClick.AddListener(() => gameObject.SetActive(false));

            // --- THE NEW BUTTON HOOKS ---
            if (deleteAccountBtn != null) 
                deleteAccountBtn.onClick.AddListener(() => deleteWarningPanel.SetActive(true));

            if (cancelDeleteBtn != null) 
                cancelDeleteBtn.onClick.AddListener(() => deleteWarningPanel.SetActive(false));

            if (confirmDeleteBtn != null) 
                confirmDeleteBtn.onClick.AddListener(ExecuteFinalAccountDeletion);
        }
        
        
        
        
        private async void OnSaveChangesClicked()
        {
            saveChangesBtn.interactable = false;
            if (statusText != null) statusText.text = "جاري الحفظ..."; // "Saving..."

            bool allSuccess = true;

            // 1. Did they change their Username?
            if (usernameInput.text.Trim() != originalUsername)
            {
                bool success = await SupabaseManager.Instance.UpdateUsername(usernameInput.text.Trim());
                if (success) originalUsername = usernameInput.text.Trim();
                else allSuccess = false;
            }

            // 2. Did they change their Email?
            string cleanedEmail = Utilities.InputSanitizer.CleanEmail(emailInput.text);

            if (cleanedEmail != originalEmail)
            {
                if (!Utilities.InputSanitizer.IsValidEmail(cleanedEmail))
                {
                    if (statusText != null)
                        statusText.text = "البريد الإلكتروني غير صالح";

                    saveChangesBtn.interactable = true;
                    return;
                }

                Debug.Log("EMAIL = [" + cleanedEmail + "]");
                Debug.Log("EMAIL LENGTH = " + cleanedEmail.Length);

                bool success = await SupabaseManager.Instance.UpdateEmail(cleanedEmail);

                if (success)
                    originalEmail = cleanedEmail;
                else
                    allSuccess = false;
            }

            // 3. Did they type a new Password?
            if (!string.IsNullOrEmpty(passwordInput.text))
            {
                bool success = await SupabaseManager.Instance.UpdatePassword(passwordInput.text);
                if (success) passwordInput.text = ""; // Clear it after saving for safety
               
                
                else allSuccess = false;
            }
            
            

            // 4. Final Feedback
            if (statusText != null)
            {
                if (allSuccess) statusText.text = "تم حفظ التغييرات بنجاح!"; // "Changes saved successfully!"
                else statusText.text = "حدث خطأ أثناء حفظ بعض التغييرات."; // "An error occurred while saving."
            }

            // Re-check the button state (it should turn off now since originals match inputs)
            CheckForChanges("");
        }

        private void CheckForChanges(string dummyText)
        {
            if (saveChangesBtn == null) return;
            bool nameChanged = (usernameInput.text.Trim() != originalUsername);
            bool emailChanged = (emailInput.text.Trim() != originalEmail);
            bool passwordChanged = (!string.IsNullOrEmpty(passwordInput.text));
            saveChangesBtn.interactable = (nameChanged || emailChanged || passwordChanged);
        }

        // ... (Keep your OnSaveChangesClicked as it is) ...

        // --- THE ACTUAL DELETION METHOD ---
        private async void ExecuteFinalAccountDeletion()
        {
            // Hide the warning and show status
            deleteWarningPanel.SetActive(false);
            if (statusText != null) statusText.text = "جاري حذف الحساب نهائياً..."; 

            bool success = await SupabaseManager.Instance.DeleteMyAccount();
            
            if (!success)
            {
                if (statusText != null) statusText.text = "فشل حذف الحساب. حاول لاحقاً.";
            }
            // If success, SupabaseManager calls Logout() which takes the user to Auth screen.
        }
    }
}