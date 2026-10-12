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
using System.IO;
using System.Text;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace ServiceDesk.Maui.ViewModels
{
    public partial class QuotationsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Quotation> _allQuotations = new();
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Quotation? SelectedQuotation { get; set; }

        public ObservableCollection<Quotation> Quotations { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<Equipment> Equipments { get; } = new();

        public QuotationsViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _ = LoadQuotationsAsync();
        }

        [RelayCommand]
        public void SelectQuotation(Quotation quotation)
        {
            SelectedQuotation = quotation;
        }

        private string GetLoc(string key, string fallback = "")
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val) && val != null)
            {
                string result = val.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
            return !string.IsNullOrWhiteSpace(fallback) ? fallback : key;
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
            var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allQuotations.ToList() : _allQuotations.Where(q =>
                (q.Customer?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (q.Status?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Quotations.Clear();
                foreach (var item in filtered) Quotations.Add(item);
            });
        }

        [RelayCommand]
        public async Task LoadQuotationsAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var customers = await _apiService.GetCustomersAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Customers.Clear();
                    if (customers != null) foreach (var c in customers) Customers.Add(c);
                });

                var equipments = await _apiService.GetEquipmentAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Equipments.Clear();
                    if (equipments != null) foreach (var e in equipments) Equipments.Add(e);
                });

                var items = await _apiService.GetQuotationsAsync();
                _allQuotations.Clear();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item.Customer == null && item.CustomerId != 0 && customers != null)
                        {
                            item.Customer = customers.FirstOrDefault(c => c.Id == item.CustomerId);
                        }
                        _allQuotations.Add(item);
                    }
                }

                FilterData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading quotations: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToAddQuotationAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            var popup = new QuotationPopup(_apiService, Customers, Equipments, null);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Quotation)
            {
                await LoadQuotationsAsync();
            }
        }

        [RelayCommand]
        public async Task EditQuotationAsync(Quotation selectedQuotation)
        {
            if (selectedQuotation == null || Shell.Current?.CurrentPage == null) return;

            var popup = new QuotationPopup(_apiService, Customers, Equipments, selectedQuotation);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            if (result is Quotation updatedQuotation)
            {
                await LoadQuotationsAsync();
                SelectedQuotation = _allQuotations.FirstOrDefault(q => q.Id == updatedQuotation.Id);
            }
        }

        [RelayCommand]
        public async Task DeleteQuotationAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedQuotation == null)
            {
                await Shell.Current.DisplayAlert(GetLoc("LblWarning", "Warning"), "Please select a quotation from the list to delete.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Deletion",
                $"Are you sure you want to delete Quotation #{SelectedQuotation.Id}?",
                "Yes, Delete", "Cancel");

            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteQuotationAsync(SelectedQuotation.Id);

                if (isSuccess)
                {
                    _allQuotations.Remove(SelectedQuotation);
                    FilterData();
                    SelectedQuotation = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert(GetLoc("LblError", "Error"), "Failed to delete quotation.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(GetLoc("LblError", "Error"), $"Could not delete: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task PrintQuotationAsync()
        {
            if (Shell.Current == null) return;

            if (SelectedQuotation == null)
            {
                await Shell.Current.DisplayAlert(GetLoc("LblWarning", "Warning"), "Please select a quotation to print.", "OK");
                return;
            }

            try
            {
                string safeName = $"Quotation_{SelectedQuotation.Id:D3}";
                string filePath = Path.Combine(FileSystem.CacheDirectory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.html");

                string titleClient = GetLoc("LblClientData", "Client Information");
                string titleDetails = GetLoc("LblQuotationDetails", "Quotation Details");
                string titleEquipment = GetLoc("LblEquipmentDetails", "Equipment Details");
                string titleProblem = GetLoc("LblProblemDescription", "Problem Description");

                string lblClient = GetLoc("LblClient", "Client:");
                string lblPhone = GetLoc("LblColPhone", "Phone:");
                string lblDate = GetLoc("LblColDate", "Created On:");
                string lblStatus = GetLoc("LblColStatus", "Status:");
                string lblCost = GetLoc("LblEstimatedCost", "Estimated Total Cost");
                string lblSignCompany = GetLoc("LblSignCompany", "Company Signature");
                string lblSignClient = GetLoc("LblSignClient", "Client Approval");

                await Task.Run(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html><head><meta charset='utf-8'>");
                    sb.AppendLine($"<title>Quotation QUO-{SelectedQuotation.Id:D3}</title>");
                    sb.AppendLine("<style>");

                    sb.AppendLine("@page { size: A4; margin: 15mm; }");
                    sb.AppendLine("body { font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #333; margin: 0; padding: 0; background-color: #fff; }");
                    sb.AppendLine(".a4-container { width: 100%; box-sizing: border-box; }");

                    sb.AppendLine(".header { display: flex; justify-content: space-between; align-items: center; border-bottom: 3px solid #0DCAF0; padding-bottom: 15px; margin-bottom: 30px; }");
                    sb.AppendLine(".header h1 { color: #1A2837; font-size: 24pt; font-weight: 800; margin: 0; text-transform: uppercase; letter-spacing: 1px; }");
                    sb.AppendLine(".header-info { text-align: right; }");
                    sb.AppendLine(".header-info h2 { margin: 0; color: #1976D2; font-size: 18pt; }");
                    sb.AppendLine(".header-info p { margin: 5px 0 0 0; color: #6C757D; font-size: 11pt; }");

                    sb.AppendLine(".section { margin-bottom: 25px; page-break-inside: avoid; }");
                    sb.AppendLine(".section-title { font-size: 13pt; font-weight: bold; color: #fff; background-color: #1A2837; padding: 8px 12px; border-radius: 4px; margin-bottom: 12px; display: block; }");
                    sb.AppendLine(".info-text { font-size: 11pt; padding: 0 12px; line-height: 1.5; color: #1A2837; word-wrap: break-word; overflow-wrap: break-word; word-break: break-word; }");

                    sb.AppendLine(".info-grid { display: table; width: 100%; border-collapse: collapse; table-layout: fixed; }");
                    sb.AppendLine(".info-row { display: table-row; }");
                    sb.AppendLine(".info-cell { display: table-cell; padding: 8px 12px; border-bottom: 1px solid #eee; font-size: 11pt; word-wrap: break-word; overflow-wrap: break-word; word-break: break-word; }");
                    sb.AppendLine(".info-cell.label { font-weight: bold; color: #6C757D; width: 30%; }");

                    sb.AppendLine(".price-box { border: 2px solid #198754; border-radius: 8px; padding: 20px; text-align: right; margin-top: 40px; background-color: #f8fff9; page-break-inside: avoid; }");
                    sb.AppendLine(".price-label { font-size: 12pt; font-weight: bold; color: #6C757D; text-transform: uppercase; }");
                    sb.AppendLine(".price-value { font-size: 26pt; font-weight: 900; color: #198754; margin-top: 5px; }");

                    sb.AppendLine(".disclaimer { margin-top: 20px; font-size: 10pt; color: #8A97A8; font-style: italic; page-break-inside: avoid; }");

                    sb.AppendLine(".signatures { display: flex; justify-content: space-between; margin-top: 60px; padding: 0 20px; page-break-inside: avoid; }");
                    sb.AppendLine(".sig-box { width: 40%; text-align: center; font-size: 11pt; font-weight: bold; color: #1A2837; }");
                    sb.AppendLine(".sig-line { border-top: 1px solid #1A2837; padding-top: 5px; margin-top: 5px; }");
                    sb.AppendLine(".sig-img { max-width: 100%; height: 60px; object-fit: contain; display: block; margin: 0 auto; background-color: #ffffff; }");

                    sb.AppendLine("@media print { ");
                    sb.AppendLine("  @page { margin: 15mm; }");
                    sb.AppendLine("  body { background-color: transparent; }");
                    sb.AppendLine("  * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }");
                    sb.AppendLine("}");
                    sb.AppendLine("</style></head>");

                    sb.AppendLine("<body>");

                    sb.AppendLine("<div class='a4-container'>");

                    sb.AppendLine("<div class='header'>");
                    sb.AppendLine("  <div>");
                    sb.AppendLine($"    <h1>{GetLoc("LblCompanyTitle", "Abc2 ServiceDesk")}</h1>");
                    sb.AppendLine("  </div>");
                    sb.AppendLine("  <div class='header-info'>");
                    sb.AppendLine($"    <h2>QUOTATION</h2>");
                    sb.AppendLine($"    <p><b>ID:</b> QUO-{SelectedQuotation.Id:D3}</p>");
                    sb.AppendLine($"    <p><b>{GetLoc("LblDate", "Date:")}</b> {DateTime.Now:dd-MM-yyyy}</p>");
                    sb.AppendLine("  </div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{titleClient}</div>");
                    sb.AppendLine("<div class='info-grid'>");
                    sb.AppendLine($"<div class='info-row'><div class='info-cell label'>{lblClient}</div><div class='info-cell'>{SelectedQuotation.Customer?.FullName ?? "Unknown"}</div></div>");

                    if (!string.IsNullOrEmpty(SelectedQuotation.Customer?.Phone))
                        sb.AppendLine($"<div class='info-row'><div class='info-cell label'>{lblPhone}</div><div class='info-cell'>{SelectedQuotation.Customer.Phone}</div></div>");

                    if (!string.IsNullOrEmpty(SelectedQuotation.Customer?.Email))
                        sb.AppendLine($"<div class='info-row'><div class='info-cell label'>Email:</div><div class='info-cell'>{SelectedQuotation.Customer.Email}</div></div>");
                    sb.AppendLine("</div></div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{titleEquipment}</div>");
                    string equipValue = string.IsNullOrWhiteSpace(SelectedQuotation.EquipmentDetails) ? "N/A" : SelectedQuotation.EquipmentDetails;
                    sb.AppendLine($"<div class='info-text'>{equipValue}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{titleProblem}</div>");
                    string problemValue = string.IsNullOrWhiteSpace(SelectedQuotation.ProblemDescription) ? "N/A" : SelectedQuotation.ProblemDescription;
                    sb.AppendLine($"<div class='info-text'>{problemValue}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{titleDetails}</div>");
                    sb.AppendLine("<div class='info-grid'>");
                    sb.AppendLine($"<div class='info-row'><div class='info-cell label'>{lblDate}</div><div class='info-cell'>{SelectedQuotation.CreatedAt:yyyy-MM-dd HH:mm}</div></div>");
                    sb.AppendLine($"<div class='info-row'><div class='info-cell label'>{lblStatus}</div><div class='info-cell'><b>{SelectedQuotation.Status}</b></div></div>");
                    sb.AppendLine("</div></div>");

                    sb.AppendLine("<div class='price-box'>");
                    sb.AppendLine($"<div class='price-label'>{lblCost}</div>");
                    sb.AppendLine($"<div class='price-value'>€ {SelectedQuotation.EstimatedCost:F2}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='disclaimer'>* This is a price estimate based on initial diagnostics. Final costs may vary if additional required parts or labor are discovered during the service process. Estimate valid for 30 days.</div>");

                    sb.AppendLine("<div class='signatures'>");

                    sb.AppendLine("<div class='sig-box'>");
                    sb.AppendLine("<div style='height: 60px;'></div>");
                    sb.AppendLine($"<div class='sig-line'>{lblSignCompany}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='sig-box'>");
                    if (!string.IsNullOrEmpty(SelectedQuotation.ClientSignatureBase64))
                    {
                        sb.AppendLine($"<img id='raw-sig' src='data:image/png;base64,{SelectedQuotation.ClientSignatureBase64}' style='visibility: hidden; position: absolute; width: 0; height: 0;' />");
                        sb.AppendLine($"<canvas id='render-sig' class='sig-img' width='300' height='150'></canvas>");
                    }
                    else
                    {
                        sb.AppendLine("<div style='height: 60px;'></div>");
                    }
                    sb.AppendLine($"<div class='sig-line'>{lblSignClient}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<script>");
                    sb.AppendLine("window.onload = function() {");
                    sb.AppendLine("  var img = document.getElementById('raw-sig');");
                    sb.AppendLine("  var canvas = document.getElementById('render-sig');");
                    sb.AppendLine("  if (img && canvas) {");
                    sb.AppendLine("    var ctx = canvas.getContext('2d');");
                    sb.AppendLine("    ctx.fillStyle = '#ffffff';");
                    sb.AppendLine("    ctx.fillRect(0, 0, canvas.width, canvas.height);");
                    sb.AppendLine("    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);");
                    sb.AppendLine("  }");
                    sb.AppendLine("  setTimeout(function() { window.print(); }, 500);");
                    sb.AppendLine("};");
                    sb.AppendLine("</script>");

                    sb.AppendLine("</body></html>");

                    File.WriteAllText(filePath, sb.ToString());
                });

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await Launcher.Default.OpenAsync(new OpenFileRequest
                        {
                            Title = GetLoc("LblPrintDocument", "Print Document"),
                            File = new ReadOnlyFile(filePath)
                        });
                    }
                    catch (Exception)
                    {
                        await Share.Default.RequestAsync(new ShareFileRequest
                        {
                            Title = "Save or View Quotation",
                            File = new ShareFile(filePath)
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(GetLoc("LblError", "Error"), $"Could not generate document.\nError: {ex.Message}", "OK");
                    }
                });
            }
        }
    }
}