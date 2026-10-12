using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ServiceDesk.Maui.Views
{
    public partial class ProductPopup : Popup
    {
        private int _editingProductId;
        private Page? _parentPage;

        public ProductPopup(InventoryItem? product = null)
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

            if (product != null)
            {
                TitleLabel.Text = GetLoc("LblEditProductTitle");
                SaveButton.Text = GetLoc("LblSave");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");

                _editingProductId = product.Id;
                SkuEntry.Text = product.PartNumber;
                NameEntry.Text = product.Name;
                PriceEntry.Text = product.UnitPrice.ToString();
                QtyEntry.Text = product.QuantityInStock.ToString();
            }
            else
            {
                TitleLabel.Text = GetLoc("LblNewProductTitle");
                SaveButton.Text = GetLoc("LblSaveProduct");
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
                Resources["PopupWidth"] = 420.0;
                Resources["PopupPadding"] = new Thickness(32, 32);
                ApplyFonts(24, 15, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                Resources["PopupWidth"] = 400.0;
                Resources["PopupPadding"] = new Thickness(24, 32);
                ApplyFonts(22, 14, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 650)
            {
                Resources["PopupWidth"] = 380.0;
                Resources["PopupPadding"] = new Thickness(24, 24);
                ApplyFonts(20, 13, 14, 14);
                ApplyButtonHeights(44);
            }
            else
            {
                Resources["PopupWidth"] = 320.0;
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
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(SkuEntry.Text))
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert(GetLoc("LblValidationError"), GetLoc("LblValidationFailed"), "OK");
                return;
            }

            if (!decimal.TryParse(PriceEntry.Text, out decimal price) || !int.TryParse(QtyEntry.Text, out int qty))
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert(GetLoc("LblValidationError"), GetLoc("LblInvalidNumeric"), "OK");
                return;
            }

            var result = new InventoryItem
            {
                Id = _editingProductId,
                PartNumber = SkuEntry.Text,
                Name = NameEntry.Text,
                UnitPrice = price,
                QuantityInStock = qty
            };

            Close(result);
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            Close(null);
        }
    }
}