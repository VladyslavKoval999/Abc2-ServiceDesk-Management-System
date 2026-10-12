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
    public partial class CustomersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Customer> _allCustomers = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string PageTitle { get; set; } = "ServiceDesk - Customers Management";

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Customer? SelectedCustomer { get; set; }

        public ObservableCollection<Customer> Customers { get; } = new();

        public CustomersViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadCustomersAsync();
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
            Customers.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allCustomers : _allCustomers.Where(c =>
                (c.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (c.Phone?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (c.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) Customers.Add(item);
        }

        [RelayCommand]
        public async Task LoadCustomersAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                var items = await _apiService.GetCustomersAsync();

                _allCustomers.Clear();
                if (items != null)
                {
                    foreach (var item in items) _allCustomers.Add(item);
                }
                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddCustomerAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new CustomerPopup();
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Customer newCustomer)
            {
                try
                {
                    bool isSuccess = await _apiService.AddCustomerAsync(newCustomer);
                    if (isSuccess) await LoadCustomersAsync();
                    else await 
                            Shell.Current.DisplayAlert("Error", "Failed to add customer to the server.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditCustomerAsync(Customer selectedCustomer)
        {
            if (selectedCustomer == null || Shell.Current?.CurrentPage == null) return;

            var popup = new CustomerPopup(selectedCustomer);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Customer updatedCustomer)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateCustomerAsync(updatedCustomer);
                    if (isSuccess) await LoadCustomersAsync();
                    else await Shell.Current.DisplayAlert("Error", "Failed to update customer on the server.", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteCustomerAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedCustomer == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a customer from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to permanently delete customer: {SelectedCustomer.FullName}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteCustomerAsync(SelectedCustomer.Id);

                if (isSuccess)
                {
                    _allCustomers.Remove(SelectedCustomer);
                    FilterData();
                    SelectedCustomer = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete customer on the server.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}