using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Maui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public partial class PurchasePopup : Popup
    {
        private int _editingPurchaseId;
        private DateTime _purchaseDate;
        private readonly ApiService _apiService;
        private List<Supplier> _suppliersList;
        private List<InventoryItem> _invList;
        private Page? _parentPage;

        public PurchasePopup(ApiService apiService, IEnumerable<Supplier> suppliers, IEnumerable<InventoryItem> inventoryItems, Purchase? purchase = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _suppliersList = suppliers.ToList();
            _invList = inventoryItems.ToList();

            SupplierPicker.ItemsSource = _suppliersList;
            InventoryPicker.ItemsSource = _invList;

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

            if (purchase != null)
            {
                TitleLabel.Text = GetLoc("LblEditPurchaseTitle");
                SaveButton.Text = GetLoc("LblSave");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");

                AddSupplierBtn.IsVisible = false;
                AddInvBtn.IsVisible = false;

                _editingPurchaseId = purchase.Id;
                _purchaseDate = purchase.PurchaseDate;

                QuantityEntry.Text = purchase.Quantity.ToString();
                UnitPriceEntry.Text = purchase.UnitPrice.ToString("F2");
                TotalCostLabel.Text = purchase.TotalCost.ToString("F2");

                if (purchase.SupplierId != 0) SupplierPicker.SelectedItem = _suppliersList.FirstOrDefault(s => s.Id == purchase.SupplierId);
                if (purchase.InventoryItemId != 0) InventoryPicker.SelectedItem = _invList.FirstOrDefault(i => i.Id == purchase.InventoryItemId);
            }
            else
            {
                TitleLabel.Text = GetLoc("LblCreatePurchaseTitle");
                SaveButton.Text = GetLoc("LblAddPurchaseBtn");
                SaveButton.BackgroundColor = Color.FromArgb("#198754");

                AddSupplierBtn.IsVisible = true;
                AddInvBtn.IsVisible = true;
                _purchaseDate = DateTime.Now;
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
                Resources["PopupWidth"] = 480.0;
                Resources["PopupPadding"] = new Thickness(32, 32);
                ApplyFonts(24, 15, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                Resources["PopupWidth"] = 440.0;
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
            AddSupplierBtn.HeightRequest = height;
            AddInvBtn.HeightRequest = height;
        }

        private string GetLoc(string key)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val))
                return val?.ToString() ?? key;
            return key;
        }

        private void OnCalculateTotal(object? sender, TextChangedEventArgs e)
        {
            if (int.TryParse(QuantityEntry.Text, out int qty) && decimal.TryParse(UnitPriceEntry.Text, out decimal price))
            {
                TotalCostLabel.Text = (qty * price).ToString("F2");
            }
            else
            {
                TotalCostLabel.Text = "0.00";
            }
        }

        private async void OnAddSupplierClicked(object sender, EventArgs e)
        {
            var popup = new SupplierPopup(null);
            var result = await Shell.Current!.ShowPopupAsync(popup);
            if (result is Supplier newSupp)
            {
                await _apiService.AddSupplierAsync(newSupp);
                var updated = await _apiService.GetSuppliersAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _suppliersList = updated.ToList();
                    SupplierPicker.ItemsSource = null;
                    SupplierPicker.ItemsSource = _suppliersList;
                    SupplierPicker.SelectedItem = _suppliersList.FirstOrDefault(s => s.CompanyName == newSupp.CompanyName);
                });
            }
        }

        private async void OnAddInvClicked(object sender, EventArgs e)
        {
            var popup = new ProductPopup(null);
            var result = await Shell.Current!.ShowPopupAsync(popup);
            if (result is InventoryItem newItem)
            {
                await _apiService.AddInventoryItemAsync(newItem);
                var updated = await _apiService.GetInventoryAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _invList = updated.ToList();
                    InventoryPicker.ItemsSource = null;
                    InventoryPicker.ItemsSource = _invList;
                    InventoryPicker.SelectedItem = _invList.FirstOrDefault(i => i.Name == newItem.Name);
                });
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            var selectedSupplier = SupplierPicker.SelectedItem as Supplier;
            var selectedInventory = InventoryPicker.SelectedItem as InventoryItem;

            if (selectedSupplier == null || selectedInventory == null ||
                !int.TryParse(QuantityEntry.Text, out int qty) || !decimal.TryParse(UnitPriceEntry.Text, out decimal unitPrice))
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(GetLoc("LblValidationError"), GetLoc("LblPurchaseValidationMsg"), "OK");
                }
                return;
            }

            decimal totalCost = qty * unitPrice;

            var result = new Purchase
            {
                Id = _editingPurchaseId,
                SupplierId = selectedSupplier.Id,
                InventoryItemId = selectedInventory.Id,
                Quantity = qty,
                UnitPrice = unitPrice,
                TotalCost = totalCost,
                PurchaseDate = _purchaseDate,
                Supplier = null,
                InventoryItem = null
            };
            Close(result);
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(null);
    }
}