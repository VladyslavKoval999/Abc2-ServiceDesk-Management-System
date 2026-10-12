using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq;

namespace ServiceDesk.Maui.Views
{
    public partial class SupplierPopup : Popup
    {
        private int _editingSupplierId;
        private Page? _parentPage;

        public SupplierPopup(Supplier? supplier = null)
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

            if (supplier != null)
            {
                TitleLabel.Text = GetLoc("LblEditSupplierTitle");
                SaveButton.Text = GetLoc("LblSave");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");

                _editingSupplierId = supplier.Id;
                CompanyNameEntry.Text = supplier.CompanyName;
                ContactEntry.Text = supplier.ContactPerson;
                PhoneEntry.Text = supplier.Phone;
                EmailEntry.Text = supplier.Email;
            }
            else
            {
                TitleLabel.Text = GetLoc("LblNewSupplierTitle");
                SaveButton.Text = GetLoc("LblAddSupplierBtn");
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
                Resources["PopupWidth"] = 440.0;
                Resources["PopupPadding"] = new Thickness(32, 32);
                ApplyFonts(24, 15, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                Resources["PopupWidth"] = 420.0;
                Resources["PopupPadding"] = new Thickness(24, 32);
                ApplyFonts(22, 14, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 650)
            {
                Resources["PopupWidth"] = 400.0;
                Resources["PopupPadding"] = new Thickness(24, 24);
                ApplyFonts(20, 13, 14, 14);
                ApplyButtonHeights(44);
            }
            else
            {
                Resources["PopupWidth"] = 340.0;
                Resources["PopupPadding"] = new Thickness(20, 20);
                ApplyFonts(18, 12, 13, 13);
                ApplyButtonHeights(42);
            }
        }

        private void ApplyFonts(double titleSize, double labelSize, double entrySize, double btnSize)
        {
            Resources["PopupTitleSize"] = titleSize;
            Resources["PopupLabelSize"] = labelSize;
            Resources["PopupEntrySize"] = entrySize;
            Resources["PopupButtonSize"] = btnSize;
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

            if (string.IsNullOrWhiteSpace(CompanyNameEntry.Text))
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblSupplierValidationMsg"), "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(phoneStr) && !phoneStr.All(char.IsDigit))
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

            var result = new Supplier { Id = _editingSupplierId, CompanyName = CompanyNameEntry.Text, ContactPerson = ContactEntry.Text ?? "", Phone = PhoneEntry.Text ?? "", Email = EmailEntry.Text ?? "" };
            Close(result);
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(null);
    }
}