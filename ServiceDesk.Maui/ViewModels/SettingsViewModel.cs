using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ServiceDesk.Maui.Services;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDesk.Maui.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _username;
        private string _newPassword;
        private string _confirmPassword;
        private string _selectedLanguage;
        private bool _notificationsEnabled;

        private bool _isNewPasswordHidden = true;
        private bool _isConfirmPasswordHidden = true;

        private bool _isInitializing = true;

        public ObservableCollection<string> Languages { get; } = new ObservableCollection<string> { "English", "Portuguese" };

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public bool IsNewPasswordHidden
        {
            get => _isNewPasswordHidden;
            set { _isNewPasswordHidden = value; OnPropertyChanged(); }
        }

        public bool IsConfirmPasswordHidden
        {
            get => _isConfirmPasswordHidden;
            set { _isConfirmPasswordHidden = value; OnPropertyChanged(); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();

                    if (!_isInitializing)
                        ApplyLanguageChange(value);
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (_notificationsEnabled != value)
                {
                    _notificationsEnabled = value;
                    OnPropertyChanged();

                    if (!_isInitializing)
                        Preferences.Set("NotificationsEnabled", value);
                }
            }
        }

        public ICommand SaveChangesCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ToggleNewPasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

        public SettingsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            SaveChangesCommand = new Command(async () => await OnSaveChanges());
            LogoutCommand = new Command(async () => await OnLogout());

            ToggleNewPasswordVisibilityCommand = new Command(() => IsNewPasswordHidden = !IsNewPasswordHidden);
            ToggleConfirmPasswordVisibilityCommand = new Command(() => IsConfirmPasswordHidden = !IsConfirmPasswordHidden);

            LoadSettings();
        }

        private void LoadSettings()
        {
            _isInitializing = true;

            Username = UserSession.Instance?.Username ?? Preferences.Get("Username", "Admin");

            string savedLang = Preferences.Get("AppLanguage", "en");
            SelectedLanguage = savedLang == "pt" ? "Portuguese" : "English";

            NotificationsEnabled = Preferences.Get("NotificationsEnabled", true);

            _isInitializing = false;
        }

        private void ApplyLanguageChange(string lang)
        {
            string langCode = lang == "Portuguese" ? "pt" : "en";
            Preferences.Set("AppLanguage", langCode);
            App.ChangeLanguage(langCode);
        }

        private async Task OnSaveChanges()
        {
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmPassword)
            {
                string errorTitle = "Error";
                string errorMsg = "Passwords do not match.";

                if (Application.Current.Resources.TryGetValue("LblError", out var errObj)) errorTitle = errObj.ToString();
                if (Application.Current.Resources.TryGetValue("LblPasswordsDoNotMatch", out var msgObj)) errorMsg = msgObj.ToString();

                await Application.Current.MainPage.DisplayAlert(errorTitle, errorMsg, "OK");
                return;
            }

            try
            {
                string oldUsername = UserSession.Instance?.Username ?? Preferences.Get("Username", "Admin");

                var users = await _apiService.GetUsersAsync();
                var currentUser = users.FirstOrDefault(u => string.Equals(u.Username, oldUsername, StringComparison.OrdinalIgnoreCase));

                if (currentUser == null && users.Any())
                {
                    currentUser = users.FirstOrDefault();
                }

                if (currentUser != null)
                {
                    currentUser.Username = Username;
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        currentUser.PasswordHash = NewPassword;
                    }

                    var updateResult = await _apiService.UpdateUserAsync(currentUser);
                    if (!updateResult.IsSuccess)
                    {
                        await Application.Current.MainPage.DisplayAlert("Database Error", $"Failed to update user.\nReason: {updateResult.ErrorMessage}", "OK");
                        return;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No users found in the database to update.", "OK");
                    return;
                }

                if (UserSession.Instance != null)
                {
                    UserSession.Instance.Username = Username;
                }
                Preferences.Set("Username", Username);

                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;

                IsNewPasswordHidden = true;
                IsConfirmPasswordHidden = true;

                string successTitle = "Success";
                string successMsg = "Settings saved successfully.";

                if (Application.Current.Resources.TryGetValue("LblSuccess", out var sucObj)) successTitle = sucObj.ToString();
                if (Application.Current.Resources.TryGetValue("LblSettingsSaved", out var sMsgObj)) successMsg = sMsgObj.ToString();

                await Application.Current.MainPage.DisplayAlert(successTitle, successMsg, "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task OnLogout()
        {
            string logoutTitle = "Logout";
            string logoutMsg = "Are you sure you want to log out?";
            string yesStr = "Yes";
            string noStr = "No";

            if (Application.Current.Resources.TryGetValue("LblLogout", out var logObj)) logoutTitle = logObj.ToString();
            if (Application.Current.Resources.TryGetValue("LblLogoutMessage", out var lMsgObj)) logoutMsg = lMsgObj.ToString();
            if (Application.Current.Resources.TryGetValue("LblYes", out var yObj)) yesStr = yObj.ToString();
            if (Application.Current.Resources.TryGetValue("LblNo", out var nObj)) noStr = nObj.ToString();

            bool confirm = await Application.Current.MainPage.DisplayAlert(logoutTitle, logoutMsg, yesStr, noStr);
            if (confirm)
            {
                Preferences.Remove("AuthToken");

                if (UserSession.Instance != null)
                {
                    UserSession.Instance.Username = string.Empty;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.Quit();
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}