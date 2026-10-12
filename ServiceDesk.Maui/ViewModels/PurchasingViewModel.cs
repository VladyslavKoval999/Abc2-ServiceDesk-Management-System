using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDesk.Maui.Services;
using ServiceDesk.Maui.Views;
using ServiceDesk.Shared.Entities;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.ViewModels
{
    public partial class PurchasingViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Purchase> _allPurchases = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Purchase? SelectedPurchase { get; set; }

        public ObservableCollection<Purchase> Purchases { get; } = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public PurchasingViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadPurchasesAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            Task.Delay(300, _searchCts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled) MainThread.BeginInvokeOnMainThread(FilterData);
            });
        }

        private void FilterData()
        {
            Purchases.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allPurchases : _allPurchases.Where(p =>
                (p.Id.ToString().Contains(SearchText)) ||
                (p.Supplier?.CompanyName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.InventoryItem?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) Purchases.Add(item);
        }

        [RelayCommand]
        public async Task LoadPurchasesAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var suppliers = await _apiService.GetSuppliersAsync();
                Suppliers.Clear();
                foreach (var s in suppliers) Suppliers.Add(s);

                var inventoryItems = await _apiService.GetInventoryAsync();
                InventoryItems.Clear();
                foreach (var i in inventoryItems) InventoryItems.Add(i);

                var items = await _apiService.GetPurchasesAsync();
                _allPurchases.Clear();
                foreach (var item in items)
                {
                    if (item.Supplier == null && item.SupplierId != 0)
                        item.Supplier = Suppliers.FirstOrDefault(s => s.Id == item.SupplierId);

                    if (item.InventoryItem == null && item.InventoryItemId != 0)
                        item.InventoryItem = InventoryItems.FirstOrDefault(i => i.Id == item.InventoryItemId);

                    _allPurchases.Add(item);
                }

                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading purchases: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddPurchaseAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new PurchasePopup(_apiService, Suppliers, InventoryItems, null);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Purchase newPurchase)
            {
                try
                {
                    bool isSuccess = await _apiService.AddPurchaseAsync(newPurchase);
                    if (isSuccess) await LoadPurchasesAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to add purchase.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditPurchaseAsync(Purchase selectedPurchase)
        {
            if (selectedPurchase == null || Shell.Current?.CurrentPage == null) return;

            var popup = new PurchasePopup(_apiService, Suppliers, InventoryItems, selectedPurchase);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Purchase updatedPurchase)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdatePurchaseAsync(updatedPurchase);
                    if (isSuccess) await LoadPurchasesAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to update purchase.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeletePurchaseAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedPurchase == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a purchase from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to delete Purchase #{SelectedPurchase.Id}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeletePurchaseAsync(SelectedPurchase.Id);

                if (isSuccess)
                {
                    _allPurchases.Remove(SelectedPurchase);
                    FilterData();
                    SelectedPurchase = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete purchase.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}