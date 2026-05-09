using Data.DTO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Managers;
using RTLTMPro;

namespace UI {
    public class AccountUIController : MonoBehaviour {
        
        [Header("Authentication Panels")]
        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private GameObject signInPanel;
        [SerializeField] private GameObject signUpPanel;

        [Header("Welcome Panel Buttons")]
        [SerializeField] private Button goToSignInBtn; // Button on Welcome screen
        [SerializeField] private Button goToSignUpBtn; // Button on Welcome screen

        [Header("Signup Fields")]
        [SerializeField] private TMP_InputField signupEmailInput;
        [SerializeField] private TMP_InputField signupPasswordInput;
        [SerializeField] private TMP_InputField signupUsernameInput;
        [SerializeField] private Button signupSubmit;

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Button loginSubmit;

        [Header("Status Feedback")]
        [SerializeField] private TMP_Text statusText;

        private void Start() {
            // 1. Initial State: Show only Welcome
            ShowPanel(welcomePanel);

            // 2. Navigation Buttons
            if (goToSignInBtn != null) goToSignInBtn.onClick.AddListener(() => ShowPanel(signInPanel));
            if (goToSignUpBtn != null) goToSignUpBtn.onClick.AddListener(() => ShowPanel(signUpPanel));

            // 3. Submit Buttons
            signupSubmit.onClick.AddListener(OnSignupClicked);
            loginSubmit.onClick.AddListener(OnLoginClicked);
            
            // Clear status at start
            if (statusText != null) statusText.text = "";
        }

        // --- PANEL NAVIGATION HELPER ---
        private void ShowPanel(GameObject targetPanel) {
            welcomePanel.SetActive(targetPanel == welcomePanel);
            signInPanel.SetActive(targetPanel == signInPanel);
            signUpPanel.SetActive(targetPanel == signUpPanel);
            
            // Clear status message when switching screens
            if (statusText != null) statusText.text = "";
        }

        // --- AUTHENTICATION LOGIC ---

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
                
                // --- THE FLOW: Move to Login page after 2 seconds ---
                Invoke(nameof(SwitchToLoginAfterSignup), 2.0f);
            } else {
                ShowStatus("فشل إنشاء الحساب. حاول مرة أخرى.");
                signupSubmit.interactable = true;
            }
        }

        private void SwitchToLoginAfterSignup() {
            ShowPanel(signInPanel);
            loginSubmit.interactable = true;
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

            ProfileDTO profile = await SupabaseManager.Instance.Login(email, password);
            
            if (profile != null) {
                ShowStatus($"مرحباً بعودتك {profile.username}!");
                PlayerManager.Instance.SetMainAccount(profile); 

                // --- THE FLOW: Move to Main Menu ---
                Invoke(nameof(GoToMainMenu), 1.5f);
            } else {
                ShowStatus("خطأ في تسجيل الدخول. تأكد من البيانات.");
                loginSubmit.interactable = true;
            }
        }

        private void GoToMainMenu() {
            GameManager.Instance.ChangeState(GameState.Menu);
        }

        private void ShowStatus(string message) {
            if (statusText != null) statusText.text = ArabicFixer.Fix(message);
        }
    }
}