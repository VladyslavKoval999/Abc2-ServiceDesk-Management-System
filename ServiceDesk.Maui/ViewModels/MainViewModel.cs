using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDesk.Maui.Services;
using ServiceDesk.Maui.Views;
using ServiceDesk.Shared.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.Threading;
using Microsoft.Maui.ApplicationModel;
namespace ServiceDesk.Maui.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<WorkOrder> _allTasks = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial WorkOrder? SelectedOrder { get; set; }

        public ObservableCollection<WorkOrder> WorkOrders { get; } = new();

        public MainViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadWorkOrdersAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            Task.Delay(300, _searchCts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled) MainThread.BeginInvokeOnMainThread(FilterTasks);
            });
        }

        [RelayCommand]
        public void PerformSearch() => FilterTasks();

        private void FilterTasks()
        {
            WorkOrders.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allTasks
                : _allTasks.Where(t =>
                    (t.Id.ToString().Contains(SearchText)) ||
                    (t.IssueDescription != null && t.IssueDescription.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Status != null && t.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Customer?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (t.Equipment?.Brand?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (t.Technician?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) WorkOrders.Add(item);
        }

        [RelayCommand]
        public async Task LoadWorkOrdersAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var customers = await _apiService.GetCustomersAsync();
                var equipments = await _apiService.GetEquipmentAsync();
                var technicians = await _apiService.GetTechniciansAsync();

                var items = await _apiService.GetWorkOrdersAsync();
                _allTasks.Clear();
                foreach (var task in items)
                {
                    if (task.Customer == null && task.CustomerId != 0)
                        task.Customer = customers.FirstOrDefault(c => c.Id == task.CustomerId);

                    if (task.Equipment == null && task.EquipmentId != 0)
                        task.Equipment = equipments.FirstOrDefault(e => e.Id == task.EquipmentId);

                    if (task.Technician == null && task.TechnicianId != null)
                        task.Technician = technicians.FirstOrDefault(t => t.Id == task.TechnicianId);

                    _allTasks.Add(task);
                }

                FilterTasks();
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Connection Failed", $"Cannot load Tasks. Reason: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddWorkOrderAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var vm = new WorkOrderPopupViewModel(_apiService);
            var popup = new WorkOrderPopup(vm, null);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is WorkOrder) await LoadWorkOrdersAsync();
        }

        [RelayCommand]
        public async Task EditWorkOrderAsync(WorkOrder selectedOrder)
        {
            if (selectedOrder == null || Shell.Current?.CurrentPage == null) return;

            var vm = new WorkOrderPopupViewModel(_apiService);
            var popup = new WorkOrderPopup(vm, selectedOrder);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is WorkOrder) await LoadWorkOrdersAsync();
        }

        [RelayCommand]
        public async Task DeleteWorkOrderAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedOrder == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a task from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Confirm Deletion", $"Are you sure you want to permanently delete Task #{SelectedOrder.Id}?", "Yes", "No");
            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteWorkOrderAsync(SelectedOrder.Id);

                if (isSuccess)
                {
                    _allTasks.Remove(SelectedOrder);
                    FilterTasks();
                    SelectedOrder = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete task on the server.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}