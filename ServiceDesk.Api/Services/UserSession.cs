using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServiceDesk.Maui.Services
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
}