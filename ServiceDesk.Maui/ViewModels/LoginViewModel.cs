using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ServiceDesk.Maui.Services;

namespace ServiceDesk.Maui.ViewModels
{
    public class UserSession : INotifyPropertyChanged
    {
        private static UserSession? _instance;
        public static UserSession Instance => _instance ??= new UserSession();

        private string _username = "Guest";
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Initials));
                }
            }
        }

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Username)) return "U";
                var parts = Username.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1)
                    return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();

                return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsPasswordHidden { get; set; } = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        public partial string ErrorMessage { get; set; } = string.Empty;

        public bool IsNotBusy => !IsBusy;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Username cannot be empty.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password cannot be empty.";
                return;
            }

            IsBusy = true;

            try
            {
                var users = await _apiService.GetUsersAsync();

                if (users == null || !users.Any())
                {
                    ErrorMessage = "Database connected, but no users found! Please add a user to the database.";
                    return;
                }

                string inputUser = Email.Trim();
                string inputPass = Password.Trim();

                var validUser = users.FirstOrDefault(u =>
                    u.Username.Equals(inputUser, StringComparison.OrdinalIgnoreCase) &&
                    u.PasswordHash == inputPass);

                if (validUser != null)
                {
                    var namePart = validUser.Username;
                    if (namePart.Length > 0)
                    {
                        namePart = char.ToUpper(namePart[0]) + namePart.Substring(1);
                    }

                    UserSession.Instance.Username = namePart;

                    if (Application.Current != null && Application.Current.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                    }
                    else if (Application.Current != null)
                    {
#pragma warning disable CS0618
                        Application.Current.MainPage = new AppShell();
#pragma warning restore CS0618
                    }
                }
                else
                {
                    ErrorMessage = GetLoc("LblLoginError");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"API Connection Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetLoc(string key)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val))
                return val?.ToString() ?? key;
            return key;
        }
    }
}