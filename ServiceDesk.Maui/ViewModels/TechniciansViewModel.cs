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
    public partial class TechniciansViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Technician> _allTechnicians = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Technician? SelectedTechnician { get; set; }

        public ObservableCollection<Technician> Technicians { get; } = new();

        public TechniciansViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadTechniciansAsync();
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
            Technicians.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allTechnicians : _allTechnicians.Where(t =>
                (t.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (t.Phone?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));

            foreach (var item in filtered) Technicians.Add(item);
        }

        [RelayCommand]
        public async Task LoadTechniciansAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                var items = await _apiService.GetTechniciansAsync();

                _allTechnicians.Clear();
                if (items != null)
                {
                    foreach (var item in items) _allTechnicians.Add(item);
                }
                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading technicians: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddTechnicianAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new TechnicianPopup();
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Technician newTech)
            {
                try
                {
                    bool isSuccess = await _apiService.AddTechnicianAsync(newTech);
                    if (isSuccess) await LoadTechniciansAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to add technician.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not add: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task EditTechnicianAsync(Technician selectedTech)
        {
            if (selectedTech == null || Shell.Current?.CurrentPage == null) return;

            var popup = new TechnicianPopup(selectedTech);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Technician updatedTech)
            {
                try
                {
                    bool isSuccess = await _apiService.UpdateTechnicianAsync(updatedTech);
                    if (isSuccess) await LoadTechniciansAsync();
                    else if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", "Failed to update technician.", "OK");
                }
                catch (Exception ex)
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert("Error", $"Could not update: {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task DeleteTechnicianAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedTechnician == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Please select a technician from the list to delete.", "OK");
                return;
            }

            if (SelectedTechnician.Id == 0)
            {
                await Shell.Current.DisplayAlert("Warning", "Cannot delete a technician without a valid ID. Please refresh the data.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Confirm Deletion", $"Are you sure you want to permanently delete technician: {SelectedTechnician.FullName}? (ID: {SelectedTechnician.Id})", "Yes", "Cancel");
            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteTechnicianAsync(SelectedTechnician.Id);

                if (isSuccess)
                {
                    _allTechnicians.Remove(SelectedTechnician);
                    FilterData();
                    SelectedTechnician = null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    await Shell.Current.DisplayAlert("Backend Error (404)", "The server could not find the DELETE method for Technicians, or the Technician was already deleted in the database. Please check your API Controller.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Backend Error", $"The server rejected the deletion.\n\nReason: {ex.Message}", "OK");
                }
            }
        }
    }
}