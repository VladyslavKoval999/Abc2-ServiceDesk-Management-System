using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ServiceDesk.Maui.Views
{
    public partial class AddPartPopup : Popup
    {
        public InventoryItem? SelectedPart { get; private set; }
        public int Quantity { get; private set; } = 1;
        private Page? _parentPage;

        public AddPartPopup(List<InventoryItem> items)
        {
            InitializeComponent();
            PartPicker.ItemsSource = items;

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
                Resources["PopupWidth"] = 400.0;
                Resources["PopupPadding"] = new Thickness(24);
                ApplyFonts(20, 13, 14, 14);
                ApplyButtonHeights(44);
            }
            else if (width >= 900)
            {
                Resources["PopupWidth"] = 380.0;
                Resources["PopupPadding"] = new Thickness(20);
                ApplyFonts(19, 12, 14, 14);
                ApplyButtonHeights(44);
            }
            else if (width >= 650)
            {
                Resources["PopupWidth"] = 350.0;
                Resources["PopupPadding"] = new Thickness(20);
                ApplyFonts(18, 12, 13, 13);
                ApplyButtonHeights(42);
            }
            else
            {
                Resources["PopupWidth"] = 320.0;
                Resources["PopupPadding"] = new Thickness(16);
                ApplyFonts(17, 11, 12, 12);
                ApplyButtonHeights(40);
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
            AddBtn.HeightRequest = height;
        }

        private void OnAddClicked(object? sender, EventArgs e)
        {
            SelectedPart = PartPicker.SelectedItem as InventoryItem;
            if (int.TryParse(QtyEntry.Text, out int qty) && qty > 0) Quantity = qty;
            Close(true);
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(false);
    }
}