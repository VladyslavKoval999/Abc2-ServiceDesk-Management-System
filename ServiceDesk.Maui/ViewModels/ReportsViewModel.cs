using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Text;
using System.Globalization;

using Color = Microsoft.Maui.Graphics.Color;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace ServiceDesk.Maui.ViewModels
{
    public class ReportChartItem
    {
        public string Label { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public Color SegmentColor { get; set; } = Colors.Gray;
    }

    public class CustomerReportItem
    {
        public string CustomerName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class EquipmentReportItem
    {
        public string Brand { get; set; } = string.Empty;
        public int TaskCount { get; set; }
    }

    public partial class ReportsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public List<string> ReportTypes { get; } = new()
        {
            "General Dashboard",
            "Financial Overview",
            "Task Status Analytics",
            "Top Technicians Workload",
            "Inventory Health",
            "Customer Insights",
            "Equipment Analytics"
        };

        [ObservableProperty] public partial string SelectedReportType { get; set; } = "General Dashboard";
        [ObservableProperty] public partial bool IsRefreshing { get; set; }

        [ObservableProperty] public partial string TotalRevenueAmount { get; set; } = "€ 0.00";
        [ObservableProperty] public partial string ActiveTasksCount { get; set; } = "0";
        [ObservableProperty] public partial string TotalCustomersCount { get; set; } = "0";
        [ObservableProperty] public partial string LowStockCount { get; set; } = "0";

        public ObservableCollection<Invoice> FinancialRecords { get; } = new();
        public ObservableCollection<WorkOrder> TaskRecords { get; } = new();
        public ObservableCollection<InventoryItem> InventoryRecords { get; } = new();
        public ObservableCollection<CustomerReportItem> CustomerRecords { get; } = new();
        public ObservableCollection<EquipmentReportItem> EquipmentRecords { get; } = new();
        public ObservableCollection<ReportChartItem> ChartData { get; } = new();

        public Action? OnDataLoaded;

        public ReportsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        partial void OnSelectedReportTypeChanged(string value)
        {
            if (!IsRefreshing) _ = LoadReportsDataAsync();
        }

        private string GetLoc(string key)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val))
                return val?.ToString() ?? key;
            return key;
        }

        [RelayCommand]
        public async Task LoadReportsDataAsync()
        {
            if (IsRefreshing) return;
            IsRefreshing = true;

            try
            {
                if (SelectedReportType == "General Dashboard") await LoadDashboardReport();
                else if (SelectedReportType == "Financial Overview") await LoadFinancialReport();
                else if (SelectedReportType == "Task Status Analytics") await LoadTasksReport();
                else if (SelectedReportType == "Top Technicians Workload") await LoadTechniciansReport();
                else if (SelectedReportType == "Inventory Health") await LoadInventoryReport();
                else if (SelectedReportType == "Customer Insights") await LoadCustomerInsights();
                else if (SelectedReportType == "Equipment Analytics") await LoadEquipmentAnalytics();

                OnDataLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reports Load Error: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadDashboardReport()
        {
            var invoices = await _apiService.GetInvoicesAsync();
            var tasks = await _apiService.GetWorkOrdersAsync();
            var customers = await _apiService.GetCustomersAsync();
            var inventory = await _apiService.GetInventoryAsync();

            TotalRevenueAmount = $"€ {invoices.Sum(i => i.TotalAmount):F2}";
            ActiveTasksCount = tasks.Count(t => t.Status != "Completed").ToString();
            TotalCustomersCount = customers.Count.ToString();
            LowStockCount = inventory.Count(i => i.QuantityInStock <= 5).ToString();

            ChartData.Clear();
            if (tasks.Any())
            {
                var grouped = tasks.GroupBy(t => t.Status ?? "Unknown").ToList();
                var colors = new[] { Color.FromArgb("#0DCAF0"), Color.FromArgb("#FFC107"), Color.FromArgb("#198754"), Color.FromArgb("#6C757D") };
                int c = 0;

                foreach (var group in grouped)
                {
                    ChartData.Add(new ReportChartItem
                    {
                        Label = group.Key,
                        DisplayValue = $"{group.Count()} Tasks",
                        Percentage = (double)group.Count() / tasks.Count,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadCustomerInsights()
        {
            var invoices = await _apiService.GetInvoicesAsync();
            CustomerRecords.Clear();

            var grouped = invoices.Where(i => i.WorkOrder != null && i.WorkOrder.Customer != null)
                                  .GroupBy(i => i.WorkOrder!.Customer!.FullName)
                                  .Select(g => new CustomerReportItem
                                  {
                                      CustomerName = g.Key,
                                      TotalTasks = g.Select(x => x.WorkOrderId).Distinct().Count(),
                                      TotalRevenue = g.Sum(x => x.TotalAmount)
                                  })
                                  .OrderByDescending(x => x.TotalRevenue).Take(15).ToList();

            foreach (var c in grouped) CustomerRecords.Add(c);

            ChartData.Clear();
            if (grouped.Any())
            {
                decimal topTotal = grouped.Sum(x => x.TotalRevenue);
                var colors = new[] { Color.FromArgb("#6F42C1"), Color.FromArgb("#E83E8C"), Color.FromArgb("#FD7E14"), Color.FromArgb("#20C997") };
                int c = 0;
                foreach (var item in grouped.Take(10))
                {
                    ChartData.Add(new ReportChartItem
                    {
                        Label = item.CustomerName,
                        DisplayValue = $"€ {item.TotalRevenue:F2}",
                        Percentage = topTotal > 0 ? (double)(item.TotalRevenue / topTotal) : 0,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadEquipmentAnalytics()
        {
            var tasks = await _apiService.GetWorkOrdersAsync();
            EquipmentRecords.Clear();

            var grouped = tasks.Where(t => t.Equipment != null && !string.IsNullOrWhiteSpace(t.Equipment.Brand))
                               .GroupBy(t => t.Equipment!.Brand)
                               .Select(g => new EquipmentReportItem
                               {
                                   Brand = g.Key,
                                   TaskCount = g.Count()
                               })
                               .OrderByDescending(x => x.TaskCount).ToList();

            foreach (var e in grouped) EquipmentRecords.Add(e);

            ChartData.Clear();
            if (grouped.Any())
            {
                int totalTop = grouped.Sum(x => x.TaskCount);
                var colors = new[] { Color.FromArgb("#0DCAF0"), Color.FromArgb("#FFC107"), Color.FromArgb("#198754"), Color.FromArgb("#DC3545") };
                int c = 0;
                foreach (var item in grouped.Take(10))
                {
                    ChartData.Add(new ReportChartItem
                    {
                        Label = item.Brand,
                        DisplayValue = $"{item.TaskCount} Repairs",
                        Percentage = totalTop > 0 ? (double)item.TaskCount / totalTop : 0,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadFinancialReport()
        {
            var invoices = await _apiService.GetInvoicesAsync();
            FinancialRecords.Clear();
            foreach (var inv in invoices.OrderByDescending(i => i.Id)) FinancialRecords.Add(inv);

            ChartData.Clear();
            if (invoices.Any())
            {
                decimal totalRev = invoices.Sum(i => i.TotalAmount);
                var grouped = invoices.GroupBy(i => i.Status ?? "Unknown").ToList();
                var colors = new[] { Color.FromArgb("#198754"), Color.FromArgb("#DC3545"), Color.FromArgb("#FFC107") };
                int c = 0;
                foreach (var group in grouped)
                {
                    decimal groupSum = group.Sum(i => i.TotalAmount);
                    ChartData.Add(new ReportChartItem
                    {
                        Label = group.Key,
                        DisplayValue = $"€ {groupSum:F2}",
                        Percentage = totalRev > 0 ? (double)(groupSum / totalRev) : 0,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadTasksReport()
        {
            var tasks = await _apiService.GetWorkOrdersAsync();
            TaskRecords.Clear();
            foreach (var t in tasks.OrderByDescending(t => t.Id)) TaskRecords.Add(t);

            ChartData.Clear();
            if (tasks.Any())
            {
                var grouped = tasks.GroupBy(t => t.Status ?? "Unknown").ToList();
                var colors = new[] { Color.FromArgb("#0DCAF0"), Color.FromArgb("#FFC107"), Color.FromArgb("#198754"), Color.FromArgb("#6C757D") };
                int c = 0;
                foreach (var group in grouped)
                {
                    ChartData.Add(new ReportChartItem
                    {
                        Label = group.Key,
                        DisplayValue = $"{group.Count()} Tasks",
                        Percentage = (double)group.Count() / tasks.Count,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadTechniciansReport()
        {
            var tasks = await _apiService.GetWorkOrdersAsync();
            TaskRecords.Clear();
            foreach (var t in tasks.Where(x => x.Technician != null)) TaskRecords.Add(t);

            ChartData.Clear();
            var activeTasks = tasks.Where(t => t.Technician != null).ToList();
            if (activeTasks.Any())
            {
                var grouped = activeTasks.GroupBy(t => t.Technician!.FullName).Take(10).ToList();
                var colors = new[] { Color.FromArgb("#6F42C1"), Color.FromArgb("#E83E8C"), Color.FromArgb("#FD7E14"), Color.FromArgb("#20C997") };
                int c = 0;
                foreach (var group in grouped)
                {
                    ChartData.Add(new ReportChartItem
                    {
                        Label = group.Key,
                        DisplayValue = $"{group.Count()} Tasks",
                        Percentage = (double)group.Count() / activeTasks.Count,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        private async Task LoadInventoryReport()
        {
            var inventory = await _apiService.GetInventoryAsync();
            InventoryRecords.Clear();
            foreach (var i in inventory.OrderByDescending(x => x.QuantityInStock * x.UnitPrice)) InventoryRecords.Add(i);

            ChartData.Clear();
            if (inventory.Any())
            {
                var top = inventory.OrderByDescending(i => i.UnitPrice * i.QuantityInStock).Take(10).ToList();
                decimal totalValue = top.Sum(i => i.UnitPrice * i.QuantityInStock);
                var colors = new[] { Color.FromArgb("#0A58CA"), Color.FromArgb("#0DCAF0"), Color.FromArgb("#FFC107"), Color.FromArgb("#DC3545") };
                int c = 0;
                foreach (var item in top)
                {
                    decimal val = item.UnitPrice * item.QuantityInStock;
                    ChartData.Add(new ReportChartItem
                    {
                        Label = item.Name,
                        DisplayValue = $"€ {val:F2}",
                        Percentage = totalValue > 0 ? (double)(val / totalValue) : 0,
                        SegmentColor = colors[c % colors.Length]
                    });
                    c++;
                }
            }
        }

        [RelayCommand]
        public async Task ExportToPdfAsync()
        {
            if (IsRefreshing) return;
            IsRefreshing = true;

            try
            {
                string safeName = SelectedReportType.Replace(" ", "_");
                string filePath = Path.Combine(FileSystem.CacheDirectory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.html");

                await Task.Run(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html><head><meta charset='utf-8'>");
                    sb.AppendLine($"<title>{SelectedReportType}</title>");
                    sb.AppendLine("<style>");
                    sb.AppendLine("@page { size: A4; margin: 1.5cm; }");
                    sb.AppendLine("body { font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; font-size: 10pt; color: #000; margin: 0; padding: 20px; }");
                    sb.AppendLine("h1 { font-size: 22pt; font-weight: 600; color: #1976D2; margin: 0 0 20px 0; }");
                    sb.AppendLine(".date { font-size: 12pt; font-style: italic; margin-bottom: 20px; }");

                    sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; table-layout: fixed; page-break-inside: auto; }");
                    sb.AppendLine("thead { display: table-header-group; }");
                    sb.AppendLine("tr { page-break-inside: avoid; break-inside: avoid; }");

                    sb.AppendLine("th, td { border-bottom: 1px solid #E0E0E0; padding: 6px 2px; word-wrap: break-word; }");
                    sb.AppendLine("th { font-weight: 600; text-align: left; }");
                    sb.AppendLine(".text-center { text-align: center; }");
                    sb.AppendLine(".text-right { text-align: right; }");
                    sb.AppendLine(".text-left { text-align: left; }");
                    sb.AppendLine(".chart-section { page-break-before: always; margin-top: 20px; }");
                    sb.AppendLine(".chart-title { font-size: 18pt; font-weight: 600; color: #1976D2; margin-bottom: 20px; }");
                    sb.AppendLine(".chart-item { margin-bottom: 15px; page-break-inside: avoid; }");
                    sb.AppendLine(".chart-label { font-size: 12pt; font-weight: 600; margin-bottom: 5px; }");
                    sb.AppendLine("@media print { * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; color-adjust: exact !important; } }");
                    sb.AppendLine("</style></head>");
                    sb.AppendLine("<body onload='setTimeout(function() { window.print(); }, 500);'>");
                    sb.AppendLine($"<h1>{SelectedReportType}</h1>");
                    sb.AppendLine($"<div class='date'>Generated on: {DateTime.Now:MMMM dd, yyyy}</div>");
                    sb.AppendLine("<table>");

                    if (SelectedReportType == "Financial Overview")
                    {
                        sb.AppendLine("<colgroup><col style='width:18%'><col style='width:28%'><col style='width:18%'><col style='width:18%'><col style='width:18%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th class='text-left'>{GetLoc("LblColInvoiceNum")}</th><th class='text-left'>{GetLoc("LblColCustomer")}</th><th class='text-left'>{GetLoc("LblColDate")}</th><th class='text-center'>{GetLoc("LblColStatus")}</th><th class='text-right'>{GetLoc("LblColAmount")}</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var inv in FinancialRecords)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>INV-{inv.Id:D4}</td><td class='text-left'>{inv.WorkOrder?.Customer?.FullName ?? "N/A"}</td><td class='text-left'>{inv.IssueDate:MM/dd/yyyy}</td><td class='text-center'>{inv.Status}</td><td class='text-right'>€ {inv.TotalAmount:F2}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }
                    else if (SelectedReportType == "Customer Insights")
                    {
                        sb.AppendLine("<colgroup><col style='width:50%'><col style='width:25%'><col style='width:25%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th class='text-left'>{GetLoc("LblCustomerName")}</th><th class='text-center'>{GetLoc("LblTotalTasks")}</th><th class='text-right'>{GetLoc("LblTotalRevenueCards")}</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var cust in CustomerRecords)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>{cust.CustomerName}</td><td class='text-center'>{cust.TotalTasks}</td><td class='text-right'>€ {cust.TotalRevenue:F2}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }
                    else if (SelectedReportType == "Inventory Health")
                    {
                        sb.AppendLine("<colgroup><col style='width:16%'><col style='width:16%'><col style='width:36%'><col style='width:16%'><col style='width:16%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th class='text-left'>{GetLoc("LblColId")}</th><th class='text-left'>{GetLoc("LblColSKU")}</th><th class='text-left'>{GetLoc("LblColItemName")}</th><th class='text-center'>{GetLoc("LblColQuantity")}</th><th class='text-right'>{GetLoc("LblColPrice")}</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var i in InventoryRecords)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>INV-{i.Id:D4}</td><td class='text-left'>{i.PartNumber ?? "N/A"}</td><td class='text-left'>{i.Name}</td><td class='text-center'>{i.QuantityInStock}</td><td class='text-right'>€ {i.UnitPrice:F2}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }
                    else if (SelectedReportType == "Equipment Analytics")
                    {
                        sb.AppendLine("<colgroup><col style='width:67%'><col style='width:33%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th class='text-left'>{GetLoc("LblEquipmentBrand")}</th><th class='text-center'>{GetLoc("LblRepairsCount")}</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var eq in EquipmentRecords)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>{eq.Brand}</td><td class='text-center'>{eq.TaskCount}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }
                    else if (SelectedReportType == "Task Status Analytics" || SelectedReportType == "Top Technicians Workload")
                    {
                        sb.AppendLine("<colgroup><col style='width:16%'><col style='width:26%'><col style='width:26%'><col style='width:16%'><col style='width:16%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th class='text-left'>{GetLoc("LblColTaskNum")}</th><th class='text-left'>{GetLoc("LblColCustomer")}</th><th class='text-left'>{GetLoc("LblColTechnician")}</th><th class='text-center'>{GetLoc("LblColStatus")}</th><th class='text-right'>{GetLoc("LblColTotalCost")}</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var task in TaskRecords)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>TSK-{task.Id:D4}</td><td class='text-left'>{task.Customer?.FullName ?? "N/A"}</td><td class='text-left'>{task.Technician?.FullName ?? "N/A"}</td><td class='text-center'>{task.Status ?? ""}</td><td class='text-right'>€ {task.TotalCost:F2}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }
                    else
                    {
                        sb.AppendLine("<colgroup><col style='width:34%'><col style='width:33%'><col style='width:33%'></colgroup>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine("<tr><th class='text-left'>Metric</th><th class='text-left'>Value</th><th class='text-left'>Percentage</th></tr>");
                        sb.AppendLine("</thead><tbody>");
                        foreach (var item in ChartData)
                        {
                            sb.AppendLine($"<tr><td class='text-left'>{item.Label}</td><td class='text-left'>{item.DisplayValue}</td><td class='text-left'>{item.Percentage:P1}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                    }

                    sb.AppendLine("</table>");

                    if (ChartData.Any())
                    {
                        sb.AppendLine("<div class='chart-section'>");
                        sb.AppendLine($"<div class='chart-title'>{GetLoc("LblVisualBreakdown")}</div>");

                        foreach (var chartItem in ChartData)
                        {
                            sb.AppendLine("<div class='chart-item'>");
                            sb.AppendLine($"<div class='chart-label'>{chartItem.Label} - {chartItem.DisplayValue} ({chartItem.Percentage:P1})</div>");
                            float perc = (float)chartItem.Percentage;
                            string hexColor = chartItem.SegmentColor.ToHex();
                            string widthPerc = (perc * 100f).ToString(CultureInfo.InvariantCulture);

                            sb.AppendLine("<svg width='100%' height='20' style='display:block; margin-top:5px;'>");
                            sb.AppendLine("  <rect width='100%' height='20' fill='#EEEEEE' rx='4' ry='4' />");
                            if (perc > 0)
                            {
                                sb.AppendLine($"  <rect width='{widthPerc}%' height='20' fill='{hexColor}' rx='4' ry='4' />");
                            }
                            sb.AppendLine("</svg>");

                            sb.AppendLine("</div>");
                        }
                        sb.AppendLine("</div>");
                    }

                    sb.AppendLine("</body></html>");

                    File.WriteAllText(filePath, sb.ToString());
                });

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        Title = "View Report",
                        File = new ReadOnlyFile(filePath)
                    });
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(GetLoc("LblError"), $"Could not generate report: {ex.Message}", "OK");
                    }
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}