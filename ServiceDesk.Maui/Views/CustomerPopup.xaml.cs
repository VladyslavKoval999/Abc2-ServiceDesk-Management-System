using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq;

namespace ServiceDesk.Maui.Views
{
    public partial class CustomerPopup : Popup
    {
        private int _editingCustomerId;
        private Page? _parentPage;

        public CustomerPopup(Customer? customer = null)
        {
            InitializeComponent();

            _parentPage = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (_parentPage != null)
            {
                ApplyResponsiveStyles(_parentPage.Width);
                _parentPage.SizeChanged += OnParentPageSizeChanged;
            }
            else
            {
                ApplyResponsiveStyles(Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density);
            }

            this.Closed += OnPopupClosed;

            if (customer != null)
            {
                TitleLabel.Text = GetLoc("LblEditCustomerTitle");
                SaveButton.Text = GetLoc("LblSave");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");

                _editingCustomerId = customer.Id;
                NameEntry.Text = customer.FullName;
                PhoneEntry.Text = customer.Phone;
                EmailEntry.Text = customer.Email;
                AddressEntry.Text = customer.Address;
            }
            else
            {
                TitleLabel.Text = GetLoc("LblNewCustomerTitle");
                SaveButton.Text = GetLoc("LblAddCustomerBtn");
                SaveButton.BackgroundColor = Color.FromArgb("#198754");
            }
        }

        private void OnParentPageSizeChanged(object? sender, EventArgs e)
        {
            if (sender is Page page && page.Width > 0)
            {
                ApplyResponsiveStyles(page.Width);
            }
        }

        private void OnPopupClosed(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
        {
            if (_parentPage != null)
            {
                _parentPage.SizeChanged -= OnParentPageSizeChanged;
            }
            this.Closed -= OnPopupClosed;
        }

        private void ApplyResponsiveStyles(double width)
        {
            if (width <= 0) return;

            if (width >= 1200) 
            {
                MainBorder.WidthRequest = 420;
                MainBorder.Padding = new Thickness(32, 32);
                ApplyFonts(24, 15, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                MainBorder.WidthRequest = 400;
                MainBorder.Padding = new Thickness(24, 32);
                ApplyFonts(22, 14, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 650)
            {
                MainBorder.WidthRequest = 380;
                MainBorder.Padding = new Thickness(24, 24);
                ApplyFonts(20, 13, 14, 14);
                ApplyButtonHeights(44);
            }
            else
            {
                MainBorder.WidthRequest = 320;
                MainBorder.Padding = new Thickness(20, 20);
                ApplyFonts(18, 12, 13, 13);
                ApplyButtonHeights(42);
            }
        }

        private void ApplyFonts(double titleSize, double labelSize, double entrySize, double btnSize)
        {
            TitleLabel.FontSize = titleSize;

            LblNameTitle.FontSize = labelSize;
            LblPhoneTitle.FontSize = labelSize;
            LblEmailTitle.FontSize = labelSize;
            LblAddressTitle.FontSize = labelSize;

            NameEntry.FontSize = entrySize;
            PhoneEntry.FontSize = entrySize;
            EmailEntry.FontSize = entrySize;
            AddressEntry.FontSize = entrySize;

            CancelBtn.FontSize = btnSize;
            SaveButton.FontSize = btnSize;
        }

        private void ApplyButtonHeights(double height)
        {
            CancelBtn.HeightRequest = height;
            SaveButton.HeightRequest = height;
        }

        private string GetLoc(string key)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val))
                return val?.ToString() ?? key;
            return key;
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            string phoneStr = PhoneEntry.Text?.Replace("+", "").Replace(" ", "") ?? "";

            if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(phoneStr))
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblCustomerValidationMsg"), "OK");
                return;
            }

            if (!phoneStr.All(char.IsDigit))
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblInvalidPhone"), "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(EmailEntry.Text) && !EmailEntry.Text.Contains("@"))
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblInvalidEmail"), "OK");
                return;
            }

            var result = new Customer
            {
                Id = _editingCustomerId,
                FullName = NameEntry.Text,
                Phone = PhoneEntry.Text,
                Email = EmailEntry.Text,
                Address = AddressEntry.Text
            };

            Close(result);
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(null);
    }
}