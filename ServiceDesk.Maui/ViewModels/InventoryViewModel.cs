using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDesk.Maui.Services;
using ServiceDesk.Maui.Views;
using ServiceDesk.Shared.Entities;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<InventoryItem> _allProducts = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial InventoryItem? SelectedProduct { get; set; }

        public ObservableCollection<InventoryItem> Products { get; } = new();

        public InventoryViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadProductsAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            Task.Delay(300, _searchCts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled) FilterData();
            });
        }

        private void FilterData()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allProducts.ToList() : _allProducts.Where(p =>
                (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.PartNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Products.Clear();
                foreach (var item in filtered) Products.Add(item);
            });
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                var items = await _apiService.GetInventoryAsync();

                _allProducts.Clear();
                if (items != null)
                {
                    foreach (var item in items) _allProducts.Add(item);
                }
                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddProductAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new ProductPopup();
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is InventoryItem newItem)
            {
                try
                {
                    bool isSuccess = await _apiService.AddInventoryItemAsync(newItem);
                    if (isSuccess) await LoadProductsAsync();
                    else await Shell.Current.DisplayAlert("Error", "Failed to add product.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditProductAsync(InventoryItem selectedProduct)
        {
            if (selectedProduct == null || Shell.Current?.CurrentPage == null) return;

            var popup = new ProductPopup(selectedProduct);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is InventoryItem updatedProduct)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateInventoryItemAsync(updatedProduct);
                    if (isSuccess) await LoadProductsAsync();
                    else await Shell.Current.DisplayAlert("Error", "Failed to update product.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteProductAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedProduct == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a product from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to permanently delete product: {SelectedProduct.Name}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteInventoryItemAsync(SelectedProduct.Id);

                if (isSuccess)
                {
                    _allProducts.Remove(SelectedProduct);
                    FilterData();
                    SelectedProduct = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete product.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}