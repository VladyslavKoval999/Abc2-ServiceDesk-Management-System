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
    public partial class EquipmentViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Equipment> _allEquipment = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Equipment? SelectedEquipment { get; set; }

        public ObservableCollection<Equipment> EquipmentList { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();

        public EquipmentViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadEquipmentAsync();
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
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allEquipment.ToList() : _allEquipment.Where(e =>
                (e.Brand?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.Model?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.SerialNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.Customer?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                EquipmentList.Clear();
                foreach (var item in filtered) EquipmentList.Add(item);
            });
        }

        [RelayCommand]
        public async Task LoadEquipmentAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var customers = await _apiService.GetCustomersAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Customers.Clear();
                    if (customers != null)
                    {
                        foreach (var c in customers) Customers.Add(c);
                    }
                });

                var items = await _apiService.GetEquipmentAsync();
                _allEquipment.Clear();

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item.Customer == null && item.CustomerId != 0 && customers != null)
                        {
                            item.Customer = customers.FirstOrDefault(c => c.Id == item.CustomerId);
                        }
                        _allEquipment.Add(item);
                    }
                }

                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading equipment: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddEquipmentAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new EquipmentPopup(_apiService, Customers, null);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Equipment newEquipment)
            {
                try
                {
                    bool isSuccess = await _apiService.AddEquipmentAsync(newEquipment);
                    if (isSuccess) await LoadEquipmentAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to add equipment.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditEquipmentAsync(Equipment selectedEquipment)
        {
            if (selectedEquipment == null || Shell.Current?.CurrentPage == null) return;

            var popup = new EquipmentPopup(_apiService, Customers, selectedEquipment);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Equipment updatedEquipment)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateEquipmentAsync(updatedEquipment);
                    if (isSuccess) await LoadEquipmentAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to update equipment.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteEquipmentAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedEquipment == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select equipment from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to permanently delete equipment: {SelectedEquipment.Brand} {SelectedEquipment.Model}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteEquipmentAsync(SelectedEquipment.Id);

                if (isSuccess)
                {
                    _allEquipment.Remove(SelectedEquipment);
                    FilterData();
                    SelectedEquipment = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete equipment.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}