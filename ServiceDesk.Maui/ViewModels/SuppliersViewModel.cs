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
    public partial class SuppliersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Supplier> _allSuppliers = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Supplier? SelectedSupplier { get; set; }

        public ObservableCollection<Supplier> Suppliers { get; } = new();

        public SuppliersViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadSuppliersAsync();
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
            Suppliers.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allSuppliers : _allSuppliers.Where(s =>
                (s.CompanyName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (s.ContactPerson?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (s.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) Suppliers.Add(item);
        }

        [RelayCommand]
        public async Task LoadSuppliersAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                var items = await _apiService.GetSuppliersAsync();

                _allSuppliers.Clear();
                if (items != null)
                {
                    foreach (var item in items) _allSuppliers.Add(item);
                }
                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suppliers: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddSupplierAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new SupplierPopup();
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Supplier newSupplier)
            {
                try
                {
                    bool isSuccess = await _apiService.AddSupplierAsync(newSupplier);
                    if (isSuccess) await LoadSuppliersAsync();
                    else await Shell.Current.DisplayAlert("Error", "Failed to add supplier.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditSupplierAsync(Supplier selectedSupplier)
        {
            if (selectedSupplier == null || Shell.Current?.CurrentPage == null) return;

            var popup = new SupplierPopup(selectedSupplier);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Supplier updatedSupplier)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateSupplierAsync(updatedSupplier);
                    if (isSuccess) await LoadSuppliersAsync();
                    else await Shell.Current.DisplayAlert("Error", "Failed to update supplier.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteSupplierAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedSupplier == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a supplier from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to permanently delete supplier: {SelectedSupplier.CompanyName}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteSupplierAsync(SelectedSupplier.Id);

                if (isSuccess)
                {
                    _allSuppliers.Remove(SelectedSupplier);
                    FilterData();
                    SelectedSupplier = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete supplier on the server.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}