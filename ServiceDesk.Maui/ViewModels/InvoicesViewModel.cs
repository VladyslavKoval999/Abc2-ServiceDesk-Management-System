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
    public partial class InvoicesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Invoice> _allInvoices = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Invoice? SelectedInvoice { get; set; }

        public ObservableCollection<Invoice> Invoices { get; } = new();
        public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

        public InvoicesViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadInvoicesAsync();
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
            Invoices.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allInvoices : _allInvoices.Where(i =>
                (i.Id.ToString().Contains(SearchText)) ||
                (i.WorkOrderId.ToString().Contains(SearchText)) ||
                (i.Status?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) Invoices.Add(item);
        }

        [RelayCommand]
        public async Task LoadInvoicesAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var workOrders = await _apiService.GetWorkOrdersAsync();
                WorkOrders.Clear();
                foreach (var w in workOrders) WorkOrders.Add(w);

                var items = await _apiService.GetInvoicesAsync();
                _allInvoices.Clear();
                foreach (var item in items)
                {
                    if (item.WorkOrder == null && item.WorkOrderId != 0)
                    {
                        item.WorkOrder = WorkOrders.FirstOrDefault(w => w.Id == item.WorkOrderId);
                    }
                    _allInvoices.Add(item);
                }

                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading invoices: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddInvoiceAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new InvoicePopup(WorkOrders);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Invoice newInvoice)
            {
                try
                {
                    bool isSuccess = await _apiService.AddInvoiceAsync(newInvoice);
                    if (isSuccess) await LoadInvoicesAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to add invoice.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditInvoiceAsync(Invoice selectedInvoice)
        {
            if (selectedInvoice == null || Shell.Current?.CurrentPage == null) return;

            var popup = new InvoicePopup(WorkOrders, selectedInvoice);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Invoice updatedInvoice)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateInvoiceAsync(updatedInvoice);
                    if (isSuccess) await LoadInvoicesAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to update invoice.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteInvoiceAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedInvoice == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select an invoice from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to delete Invoice #{SelectedInvoice.Id}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteInvoiceAsync(SelectedInvoice.Id);

                if (isSuccess)
                {
                    _allInvoices.Remove(SelectedInvoice);
                    FilterData();
                    SelectedInvoice = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete invoice.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}