using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Maui.Services;
using ServiceDesk.Maui.Views;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace ServiceDesk.Maui.ViewModels
{
    public class TechnicianWorkloadItem
    {
        public string TechnicianName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int ActiveTasksCount { get; set; }
    }

    public class DashboardChartItem
    {
        public string Label { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public Color SegmentColor { get; set; } = Colors.Gray;
    }

    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] public partial int TotalCustomersCount { get; set; }
        [ObservableProperty] public partial string TotalRevenue { get; set; } = "€ 0.00";
        [ObservableProperty] public partial int PendingOrdersCount { get; set; }
        [ObservableProperty] public partial int LowStockCount { get; set; }
        [ObservableProperty] public partial bool IsRefreshing { get; set; }

        public ObservableCollection<WorkOrder> RecentOrders { get; } = new();
        public ObservableCollection<InventoryItem> LowStockItems { get; } = new();
        public ObservableCollection<TechnicianWorkloadItem> TechnicianWorkloads { get; } = new();
        public ObservableCollection<DashboardChartItem> ChartData { get; } = new();

        public Action? OnDataLoaded;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        private Color GetColorForStatus(string status)
        {
            return status switch
            {
                "Completed" => Color.FromArgb("#198754"),
                "In Progress" => Color.FromArgb("#0DCAF0"),
                "Pending Start" => Color.FromArgb("#FFC107"),
                "Waiting for Parts" => Color.FromArgb("#DC3545"),
                _ => Color.FromArgb("#6C757D")
            };
        }

        [RelayCommand]
        public async Task LoadDashboardDataAsync()
        {
            if (IsRefreshing) return;
            IsRefreshing = true;

            try
            {
                var customers = await _apiService.GetCustomersAsync();
                var workOrders = await _apiService.GetWorkOrdersAsync();
                var inventory = await _apiService.GetInventoryAsync();
                var invoices = await _apiService.GetInvoicesAsync();

                TotalCustomersCount = customers.Count;
                TotalRevenue = $"€ {invoices.Sum(i => i.TotalAmount):F2}";
                PendingOrdersCount = workOrders.Count(w => w.Status == "Pending Start");
                LowStockCount = inventory.Count(i => i.QuantityInStock < 5);

                RecentOrders.Clear();
                var recent = workOrders.OrderByDescending(w => w.CreatedAt).Take(5);
                foreach (var order in recent)
                {
                    if (order.Customer == null && order.CustomerId != 0)
                        order.Customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
                    RecentOrders.Add(order);
                }

                LowStockItems.Clear();
                var lowStock = inventory.Where(i => i.QuantityInStock < 5).OrderBy(i => i.QuantityInStock).Take(5);
                foreach (var item in lowStock) LowStockItems.Add(item);

                TechnicianWorkloads.Clear();
                var activeOrders = workOrders.Where(w => w.Status != "Completed").ToList();
                var workload = activeOrders
                    .Where(w => w.Technician != null)
                    .GroupBy(w => w.Technician!.FullName)
                    .Select(g => new TechnicianWorkloadItem
                    {
                        TechnicianName = g.Key,
                        Initials = string.Join("", g.Key.Split(' ').Take(2).Select(s => s[0])).ToUpper(),
                        ActiveTasksCount = g.Count()
                    })
                    .OrderByDescending(x => x.ActiveTasksCount)
                    .Take(4);

                foreach (var item in workload) TechnicianWorkloads.Add(item);

                ChartData.Clear();
                if (workOrders.Any())
                {
                    var grouped = workOrders.GroupBy(t => t.Status ?? "Unknown").ToList();
                    foreach (var group in grouped)
                    {
                        ChartData.Add(new DashboardChartItem
                        {
                            Label = group.Key,
                            DisplayValue = $"{group.Count()} Tasks",
                            Percentage = (double)group.Count() / workOrders.Count,
                            SegmentColor = GetColorForStatus(group.Key)
                        });
                    }
                }

                OnDataLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard Load Error: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToNewTaskAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
            var vm = new WorkOrderPopupViewModel(_apiService);
            if (await Shell.Current.CurrentPage.ShowPopupAsync(new WorkOrderPopup(vm, null)) is WorkOrder) await LoadDashboardDataAsync();
        }

        [RelayCommand]
        public async Task GoToNewInvoiceAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
            var wos = await _apiService.GetWorkOrdersAsync();
            var workOrdersList = new ObservableCollection<WorkOrder>(wos);
            if (await Shell.Current.CurrentPage.ShowPopupAsync(new InvoicePopup(workOrdersList)) is Invoice) await LoadDashboardDataAsync();
        }

        [RelayCommand]
        public async Task GoToInventoryAsync()
        {
            if (Shell.Current != null) await Shell.Current.GoToAsync("//InventoryPage");
        }

        [RelayCommand]
        public async Task GoToReportsAsync()
        {
            if (Shell.Current != null) await Shell.Current.GoToAsync("//ReportsRoute");
        }
    }
}