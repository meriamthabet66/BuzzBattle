using Data.DTO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Managers;

namespace UI {
    public class AccountUIController : MonoBehaviour {
        
        [Header("Authentication Panels")]
        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private GameObject signInPanel;
        [SerializeField] private GameObject signUpPanel;

        [Header("Welcome Panel Buttons")]
        [SerializeField] private Button welcomeToSignInBtn; 
        [SerializeField] private Button welcomeToSignUpBtn; 

        [Header("Direct Switch Buttons")]
        [SerializeField] private Button signInToSignUpBtn; 
        [SerializeField] private Button signUpToSignInBtn; 

        [Header("Signup Fields")]
        [SerializeField] private TMP_InputField signupEmailInput;
        [SerializeField] private TMP_InputField signupPasswordInput;
        [SerializeField] private TMP_InputField signupUsernameInput;
        [SerializeField] private Button signupSubmit;
        [SerializeField] private Button signupHideBtn; // The eye button inside Signup
        [SerializeField] private Image signupEyeIcon;   // The icon to swap

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginSubmit;
        [SerializeField] private Button loginHideBtn;  // The eye button inside Login
        [SerializeField] private Image loginEyeIcon;    // The icon to swap

        [Header("Eye Sprites")]
        [SerializeField] private Sprite eyeOpenSprite;
        [SerializeField] private Sprite eyeClosedSprite;

        [Header("Status Feedback")]
        [SerializeField] private TMP_Text statusText;

        private bool isLoginPasswordVisible = false;
        private bool isSignupPasswordVisible = false;
        
        private void OnEnable()
        {
            // --- THE CLEANUP FIX ---
            // Clear all text fields so the next person (or you) sees a fresh screen
            if (loginEmailInput != null) loginEmailInput.text = "";
            if (loginPasswordInput != null) loginPasswordInput.text = "";
            if (signupEmailInput != null) signupEmailInput.text = "";
            if (signupPasswordInput != null) signupPasswordInput.text = "";
            if (signupUsernameInput != null) signupUsernameInput.text = "";

            // Reset the status message
            if (statusText != null) statusText.text = "";

            // Make sure the buttons are clickable again
            if (loginSubmit != null) loginSubmit.interactable = true;
            if (signupSubmit != null) signupSubmit.interactable = true;

            // Default to Welcome Screen
            ShowPanel(welcomePanel);
        }

        private void Start() {
            ShowPanel(welcomePanel);

            // Navigation
            if (welcomeToSignInBtn != null) welcomeToSignInBtn.onClick.AddListener(() => ShowPanel(signInPanel));
            if (welcomeToSignUpBtn != null) welcomeToSignUpBtn.onClick.AddListener(() => ShowPanel(signUpPanel));
            if (signInToSignUpBtn != null) signInToSignUpBtn.onClick.AddListener(() => ShowPanel(signUpPanel));
            if (signUpToSignInBtn != null) signUpToSignInBtn.onClick.AddListener(() => ShowPanel(signInPanel));

            // Submit Buttons
            signupSubmit.onClick.AddListener(OnSignupClicked);
            loginSubmit.onClick.AddListener(OnLoginClicked);

            // --- PASSWORD HIDE/SHOW LOGIC ---
            if (loginHideBtn != null) loginHideBtn.onClick.AddListener(ToggleLoginPassword);
            if (signupHideBtn != null) signupHideBtn.onClick.AddListener(ToggleSignupPassword);
            
            // Set initial state (Hidden)
            ApplyPasswordState(loginPasswordInput, loginEyeIcon, false);
            ApplyPasswordState(signupPasswordInput, signupEyeIcon, false);
        }
        
        

        // --- PASSWORD TOGGLE LOGIC ---

        private void ToggleLoginPassword() {
            isLoginPasswordVisible = !isLoginPasswordVisible;
            ApplyPasswordState(loginPasswordInput, loginEyeIcon, isLoginPasswordVisible);
        }

        private void ToggleSignupPassword() {
            isSignupPasswordVisible = !isSignupPasswordVisible;
            ApplyPasswordState(signupPasswordInput, signupEyeIcon, isSignupPasswordVisible);
        }

        private void ApplyPasswordState(TMP_InputField input, Image icon, bool isVisible) {
            if (input == null || icon == null) return;

            // standard = visible, password = dots
            input.contentType = isVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            
            // Swap the sprite
            icon.sprite = isVisible ? eyeOpenSprite : eyeClosedSprite;

            // Force the InputField to refresh its visuals instantly
            input.ForceLabelUpdate();
        }

        // --- AUTH LOGIC (Signup/Login) ---

        private void ShowPanel(GameObject targetPanel) {
            welcomePanel.SetActive(targetPanel == welcomePanel);
            signInPanel.SetActive(targetPanel == signInPanel);
            signUpPanel.SetActive(targetPanel == signUpPanel);
            if (statusText != null) statusText.text = "";
        }

        private async void OnSignupClicked() {
            string email = signupEmailInput.text.Trim();
            string password = signupPasswordInput.text;
            string username = signupUsernameInput.text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
                ShowStatus("الرجاء إدخال البريد الإلكتروني وكلمة المرور");
                return;
            }

            signupSubmit.interactable = false;
            ShowStatus("جاري إنشاء الحساب...");

            bool success = await SupabaseManager.Instance.SignUp(email, password, username);
            
            if (success) {
                ShowStatus("تم إنشاء الحساب! جاري التحويل لتسجيل الدخول...");
                Invoke(nameof(SwitchToLoginAfterSignup), 2.0f);
            } else {
                ShowStatus("فشل إنشاء الحساب. حاول مرة أخرى.");
                signupSubmit.interactable = true;
            }
        }

        private void SwitchToLoginAfterSignup() {
            ShowPanel(signInPanel);
        }

        private async void OnLoginClicked() {
            string email = loginEmailInput.text.Trim();
            string password = loginPasswordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
                ShowStatus("الرجاء إدخال البيانات");
                return;
            }

            loginSubmit.interactable = false;
            ShowStatus("جاري تسجيل الدخول...");

            // --- THE FIX: Change from ProfileDTO to bool ---
            bool success = await SupabaseManager.Instance.Login(email, password);
            
            if (success) {
                // The SupabaseManager already told LocalAccountManager to save the data.
                // We grab the name from the local save to show the message.
                var host = LocalAccountManager.Instance.SavedAccount;
                string name = host != null ? host.Username : "";

                ShowStatus($"مرحباً بعودتك {name}!");
                
                // DELETE: PlayerManager.Instance.SetMainAccount(profile); 
                // Why? Because LocalAccountManager already did this!

                Invoke(nameof(GoToMainMenu), 1.5f);
            } else {
                ShowStatus("خطأ في تسجيل الدخول. تأكد من البيانات.");
                loginSubmit.interactable = true;
            }
        }

        private void GoToMainMenu() => GameManager.Instance.ChangeState(GameState.Menu);

        private void ShowStatus(string message) {
            if (statusText != null) statusText.text = ArabicFixer.Fix(message);
        }
    }
}