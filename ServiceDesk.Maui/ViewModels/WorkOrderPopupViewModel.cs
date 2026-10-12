using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using ServiceDesk.Maui.Services;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Text;

namespace ServiceDesk.Maui.ViewModels
{
    public partial class WorkOrderPartItem : ObservableObject
    {
        public int InventoryItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        [ObservableProperty] public partial int Quantity { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(TotalPrice));
    }

    public partial class WorkOrderPopupViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<InventoryItem> _partsToRestore = new();

        [ObservableProperty] public partial WorkOrder Order { get; set; } = new();
        [ObservableProperty] public partial bool IsNewOrder { get; set; } = true;

        public ObservableCollection<string> Statuses { get; } = new() { "Pending Start", "In Progress", "Waiting for Parts", "Completed", "Cancelled" };
        public ObservableCollection<string> ContactMethods { get; } = new();

        public ObservableCollection<Technician> Technicians { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();
        public ObservableCollection<WorkOrderPartItem> AddedParts { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<Equipment> Equipments { get; } = new();

        [ObservableProperty] public partial Technician? SelectedTechnician { get; set; }
        [ObservableProperty] public partial Customer? SelectedCustomer { get; set; }
        [ObservableProperty] public partial Equipment? SelectedEquipment { get; set; }
        [ObservableProperty] public partial InventoryItem? SelectedPart { get; set; }

        [ObservableProperty] public partial DateTime? StartDate { get; set; }
        [ObservableProperty] public partial DateTime? EndDate { get; set; }
        [ObservableProperty] public partial TimeSpan? StartTime { get; set; }
        [ObservableProperty] public partial TimeSpan? EndTime { get; set; }

        public DateTime? StartDateTime => (StartDate.HasValue && StartTime.HasValue) ? StartDate.Value.Add(StartTime.Value) : null;
        public DateTime? EndDateTime => (EndDate.HasValue && EndTime.HasValue) ? EndDate.Value.Add(EndTime.Value) : null;

        [ObservableProperty] public partial decimal HourlyRate { get; set; }
        [ObservableProperty] public partial decimal LaborCost { get; set; }
        [ObservableProperty] public partial decimal PartsCost { get; set; }
        [ObservableProperty] public partial decimal TotalCost { get; set; }

        [ObservableProperty] public partial string SelectedContactMethod { get; set; } = string.Empty;
        [ObservableProperty] public partial string CustomContactMethod { get; set; } = string.Empty;
        [ObservableProperty] public partial bool IsCustomContactMethod { get; set; }
        [ObservableProperty] public partial bool IsUnderWarranty { get; set; }

        public WorkOrderPopupViewModel(ApiService apiService)
        {
            _apiService = apiService;

            ContactMethods.Add("Phone");
            ContactMethods.Add("WhatsApp");
            ContactMethods.Add("Email");
            ContactMethods.Add("In Person");
            ContactMethods.Add(GetLoc("LblOther", "Other"));

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(StartDate) or nameof(StartTime) or nameof(EndDate) or nameof(EndTime))
                {
                    CalculateLaborCost();
                    OnPropertyChanged(nameof(StartDateTime));
                    OnPropertyChanged(nameof(EndDateTime));
                }
            };
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

        partial void OnSelectedTechnicianChanged(Technician? value)
        {
            if (value != null)
            {
                HourlyRate = value.HourlyRate;
                CalculateLaborCost();
            }
        }

        partial void OnSelectedContactMethodChanged(string value)
        {
            IsCustomContactMethod = (value == GetLoc("LblOther", "Other") || value == "Other" || value == "Email");
        }

        partial void OnSelectedPartChanged(InventoryItem? value)
        {
            if (value != null)
            {
                AddPartToGrid(value);
                MainThread.BeginInvokeOnMainThread(() => SelectedPart = null);
            }
        }

        private async void AddPartToGrid(InventoryItem part)
        {
            if (part.QuantityInStock < 1)
            {
                if (Shell.Current != null) await Shell.Current.DisplayAlert(GetLoc("LblAlert", "Alert"), GetLoc("LblNotEnoughStock", "Not enough stock"), "OK");
                return;
            }

            var existingItem = AddedParts.FirstOrDefault(p => p.InventoryItemId == part.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new WorkOrderPartItem { InventoryItemId = part.Id, Name = part.Name, UnitPrice = part.UnitPrice, Quantity = 1 };
                newItem.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(WorkOrderPartItem.Quantity)) RecalculateTotals(); };
                AddedParts.Add(newItem);
            }
            part.QuantityInStock--;
            RecalculateTotals();
        }

        [RelayCommand]
        public async Task CreateNewPartAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
        }

        [RelayCommand]
        public async Task IncreaseQtyAsync(WorkOrderPartItem item)
        {
            var invItem = InventoryItems.FirstOrDefault(i => i.Id == item.InventoryItemId);
            if (invItem != null && invItem.QuantityInStock > 0)
            {
                item.Quantity++;
                invItem.QuantityInStock--;
                RecalculateTotals();
            }
            else if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(GetLoc("LblAlert", "Alert"), GetLoc("LblNoMoreStock", "No more stock available"), "OK");
            }
        }

        [RelayCommand]
        public void DecreaseQtyAsync(WorkOrderPartItem item)
        {
            var invItem = InventoryItems.FirstOrDefault(i => i.Id == item.InventoryItemId);
            if (item.Quantity > 1)
            {
                item.Quantity--;
                if (invItem != null) invItem.QuantityInStock++;
            }
            else
            {
                AddedParts.Remove(item);
                if (invItem != null)
                {
                    invItem.QuantityInStock++;
                    _partsToRestore.Add(invItem);
                }
            }
            RecalculateTotals();
        }

        public ICommand RemovePartCommand => new Command<WorkOrderPartItem>(item =>
        {
            if (item == null) return;
            var invItem = InventoryItems.FirstOrDefault(i => i.Id == item.InventoryItemId);
            AddedParts.Remove(item);
            if (invItem != null)
            {
                invItem.QuantityInStock += item.Quantity;
                _partsToRestore.Add(invItem);
            }
            RecalculateTotals();
        });

        public async Task InitializeAsync(WorkOrder? order)
        {
            if (order != null && order.Id != 0)
            {
                IsNewOrder = false;
                Order = order;

                if (!string.IsNullOrEmpty(Order.ContactMethod))
                {
                    if (ContactMethods.Contains(Order.ContactMethod)) SelectedContactMethod = Order.ContactMethod;
                    else
                    {
                        SelectedContactMethod = GetLoc("LblOther", "Other");
                        CustomContactMethod = Order.ContactMethod;
                    }
                }

                if (Order.WorkStartTime.HasValue)
                {
                    StartDate = Order.WorkStartTime.Value.Date;
                    StartTime = Order.WorkStartTime.Value.TimeOfDay;
                }
                if (Order.WorkEndTime.HasValue)
                {
                    EndDate = Order.WorkEndTime.Value.Date;
                    EndTime = Order.WorkEndTime.Value.TimeOfDay;
                }

                HourlyRate = Order.HourlyRate;
                LaborCost = Math.Round(Order.LaborCost, 2);
                PartsCost = Order.PartsCost;
                IsUnderWarranty = Order.IsWarranty;

                if (!string.IsNullOrWhiteSpace(Order.UsedPartsText))
                {
                    try
                    {
                        var savedParts = JsonSerializer.Deserialize<WorkOrderPartItem[]>(Order.UsedPartsText);
                        if (savedParts != null)
                        {
                            foreach (var p in savedParts)
                            {
                                p.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(WorkOrderPartItem.Quantity)) RecalculateTotals(); };
                                AddedParts.Add(p);
                            }
                        }
                    }
                    catch { }
                }
            }
            else
            {
                IsNewOrder = true;
                Order = new WorkOrder { CreatedAt = DateTime.Now, Status = "Pending Start" };
                SelectedContactMethod = "Phone";
                StartDate = DateTime.Today;
                StartTime = DateTime.Now.TimeOfDay;
                EndDate = DateTime.Today;
                EndTime = DateTime.Now.AddHours(1).TimeOfDay;
            }

            await LoadDropdownsAsync();
            RecalculateTotals();
        }

        private async Task LoadDropdownsAsync()
        {
            try
            {
                var techs = await _apiService.GetTechniciansAsync();
                Technicians.Clear(); foreach (var t in techs) Technicians.Add(t);

                var items = await _apiService.GetInventoryAsync();
                InventoryItems.Clear(); foreach (var i in items) InventoryItems.Add(i);

                var customers = await _apiService.GetCustomersAsync();
                Customers.Clear(); foreach (var c in customers) Customers.Add(c);

                var equips = await _apiService.GetEquipmentAsync();
                Equipments.Clear(); foreach (var e in equips) Equipments.Add(e);

                if (Order.TechnicianId != null) SelectedTechnician = Technicians.FirstOrDefault(t => t.Id == Order.TechnicianId);
                if (Order.CustomerId != 0) SelectedCustomer = Customers.FirstOrDefault(c => c.Id == Order.CustomerId);
                if (Order.EquipmentId != 0) SelectedEquipment = Equipments.FirstOrDefault(e => e.Id == Order.EquipmentId);
            }
            catch { }
        }

        [RelayCommand]
        public async Task AddCustomerAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
        }

        [RelayCommand]
        public async Task AddEquipmentAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
        }

        [RelayCommand]
        public async Task AddTechnicianAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;
        }

        [RelayCommand]
        public async Task OpenStartDateTimePickerAsync()
        {
        }

        [RelayCommand]
        public async Task OpenEndDateTimePickerAsync()
        {
        }

        partial void OnHourlyRateChanged(decimal value) => CalculateLaborCost();

        private void CalculateLaborCost()
        {
            if (StartDate.HasValue && StartTime.HasValue && EndDate.HasValue && EndTime.HasValue)
            {
                var start = StartDate.Value.Add(StartTime.Value);
                var end = EndDate.Value.Add(EndTime.Value);
                if (end > start)
                {
                    double totalHours = (end - start).TotalHours;
                    LaborCost = Math.Round(HourlyRate * (decimal)totalHours, 2);
                }
                else LaborCost = 0;
            }
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            PartsCost = Math.Round(AddedParts.Sum(p => p.TotalPrice), 2);
            if (IsUnderWarranty) TotalCost = 0;
            else TotalCost = LaborCost + PartsCost;
        }

        partial void OnIsUnderWarrantyChanged(bool value)
        {
            Order.IsWarranty = value;
            RecalculateTotals();
        }

        private bool IsValid()
        {
            string actualMethod = IsCustomContactMethod ? CustomContactMethod : SelectedContactMethod;

            return SelectedCustomer != null &&
                   SelectedEquipment != null &&
                   !string.IsNullOrWhiteSpace(actualMethod) &&
                   !string.IsNullOrWhiteSpace(Order.RequesterName) &&
                   !string.IsNullOrWhiteSpace(Order.RequesterContact) &&
                   !string.IsNullOrWhiteSpace(Order.EquipmentState) &&
                   !string.IsNullOrWhiteSpace(Order.IssueDescription) &&
                   !string.IsNullOrWhiteSpace(Order.Status);
        }

        [RelayCommand]
        public async Task PrintTaskAsync()
        {
            if (Shell.Current?.CurrentPage == null) return;

            if (!IsValid())
            {
                await Shell.Current.DisplayAlert(GetLoc("LblValidationError", "Validation Error"), GetLoc("LblValidationFailed", "Please fill all required fields."), "OK");
                return;
            }

            if (IsNewOrder || Order.Id == 0)
            {
                await Shell.Current.DisplayAlert(GetLoc("LblWarning", "Warning"), GetLoc("LblSaveTaskFirst", "Please save the task before printing."), "OK");
                return;
            }

            string cancel = GetLoc("LblCancel", "Cancel");
            string barcodeStr = GetLoc("LblBarcode", "Barcode");
            string qrStr = GetLoc("LblQRCode", "QR Code");

            string action = await Shell.Current.DisplayActionSheet(GetLoc("LblSelectFormat", "Select format"), cancel, null, qrStr, barcodeStr);
            if (action == cancel || string.IsNullOrEmpty(action)) return;

            try
            {
                byte[]? imageBytes = null;
                using (var client = new HttpClient())
                {
                    string data = $"TSK-{Order.Id:D4}";
                    if (action == qrStr)
                        imageBytes = await client.GetByteArrayAsync($"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={data}");
                    else
                        imageBytes = await client.GetByteArrayAsync($"https://bwipjs-api.metafloor.com/?bcid=code128&text={data}");
                }

                string base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : "";
                string filePath = Path.Combine(FileSystem.CacheDirectory, $"Task_{Order.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.html");

                await Task.Run(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html><head><meta charset='utf-8'>");
                    sb.AppendLine($"<title>Task TSK-{Order.Id:D4}</title>");
                    sb.AppendLine("<style>");

                    sb.AppendLine("@page { size: auto; margin: 10mm; }");
                    sb.AppendLine("body { font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #000; margin: 0; padding: 0; background-color: #fff; }");

                    sb.AppendLine(".label-wrapper { width: 100%; text-align: center; padding-top: 80mm; page-break-after: always; }");
                    sb.AppendLine(".label-container { width: 80mm !important; height: 50mm !important; border: 2px dashed #000 !important; padding: 4mm !important; box-sizing: border-box !important; margin: 0 auto !important; text-align: center !important; background-color: #fff !important; }");
                    sb.AppendLine(".label-title { font-size: 14pt; font-weight: bold; margin-bottom: 2mm; color: #000; }");
                    sb.AppendLine(".label-text { font-size: 10pt; margin-bottom: 1mm; text-align: left; color: #000; word-wrap: break-word; overflow-wrap: break-word; word-break: break-word; }");
                    sb.AppendLine(".label-img { display: block; margin: 3mm auto 0 auto; max-width: 100%; height: 35px; background-color: #ffffff !important; }");

                    sb.AppendLine(".a4-container { width: 100%; box-sizing: border-box; }");
                    sb.AppendLine(".header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 15px; border-bottom: 2px solid #1976D2; padding-bottom: 10px; }");
                    sb.AppendLine(".header-title { color: #1976D2; font-size: 20pt; font-weight: bold; margin: 0; }");
                    sb.AppendLine(".header-date { font-size: 11pt; font-style: italic; color: #555; margin-top: 5px; }");
                    sb.AppendLine(".header-task { font-size: 16pt; font-weight: bold; }");

                    sb.AppendLine(".section { border: 1px solid #E0E0E0; border-radius: 4px; padding: 12px; margin-bottom: 15px; page-break-inside: avoid; }");
                    sb.AppendLine(".section-title { font-size: 12pt; font-weight: bold; color: #1976D2; margin-bottom: 8px; }");
                    sb.AppendLine(".info-line { font-size: 11pt; margin-bottom: 5px; word-wrap: break-word; overflow-wrap: break-word; word-break: break-word; }");

                    sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; margin-bottom: 15px; table-layout: fixed; }");
                    sb.AppendLine("thead { display: table-header-group; }");
                    sb.AppendLine("tr { page-break-inside: avoid; }");
                    sb.AppendLine("th, td { border: 1px solid #E0E0E0; padding: 8px; text-align: left; font-size: 11pt; word-wrap: break-word; overflow-wrap: break-word; word-break: break-word; }");
                    sb.AppendLine("th { background-color: #f8f9fa; font-weight: bold; }");

                    sb.AppendLine(".text-right { text-align: right; }");
                    sb.AppendLine(".text-center { text-align: center; }");

                    sb.AppendLine(".checkout-section { page-break-inside: avoid; margin-top: 20px; }");
                    sb.AppendLine(".totals-container { text-align: right; }");
                    sb.AppendLine(".total-line { font-size: 11pt; margin-bottom: 5px; }");
                    sb.AppendLine(".total-final { font-size: 14pt; font-weight: bold; color: #198754; margin-top: 8px; }");

                    sb.AppendLine(".signatures { display: flex; justify-content: space-between; margin-top: 30px; padding: 0 20px; }");
                    sb.AppendLine(".sig-box { width: 40%; text-align: center; font-size: 11pt; font-weight: bold; color: #1A2837; }");
                    sb.AppendLine(".sig-line { border-top: 1px solid #1A2837; padding-top: 5px; margin-top: 5px; }");

                    sb.AppendLine(".sig-img { max-width: 100%; height: 60px; object-fit: contain; display: block; margin: 0 auto; background-color: #ffffff; }");

                    sb.AppendLine("@media print { ");
                    sb.AppendLine("  @page { margin: 10mm; }");
                    sb.AppendLine("  body { background-color: transparent; }");
                    sb.AppendLine("  * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }");
                    sb.AppendLine("}");
                    sb.AppendLine("</style></head>");

                    sb.AppendLine("<body>");

                    sb.AppendLine("<div class='label-wrapper'>");
                    sb.AppendLine("<div class='label-container'>");
                    sb.AppendLine($"<div class='label-title'>{GetLoc("LblTask", "Task")} TSK-{Order.Id:D4}</div>");
                    sb.AppendLine($"<div class='label-text'><b>{GetLoc("LblClient", "Client:")}</b> {SelectedCustomer?.FullName ?? Order.RequesterName}</div>");
                    sb.AppendLine($"<div class='label-text'><b>{GetLoc("LblEquip", "Equipment:")}</b> {SelectedEquipment?.Brand} {SelectedEquipment?.Model}</div>");
                    sb.AppendLine($"<div class='label-text'><b>{GetLoc("LblSN", "S/N:")}</b> {SelectedEquipment?.SerialNumber ?? "N/A"}</div>");
                    if (!string.IsNullOrEmpty(base64Image))
                    {
                        sb.AppendLine($"<img class='label-img' src='data:image/png;base64,{base64Image}' alt='Barcode/QR' />");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='a4-container'>");

                    sb.AppendLine("<div class='header'>");
                    sb.AppendLine("  <div>");
                    sb.AppendLine($"    <div class='header-title'>{GetLoc("LblCompanyTitle", "Abc2 ServiceDesk")} - {GetLoc("LblServiceSheet", "Service Sheet")}</div>");
                    sb.AppendLine($"    <div class='header-date'>{GetLoc("LblDate", "Date:")} {DateTime.Now:dd-MM-yyyy HH:mm}</div>");
                    sb.AppendLine("  </div>");
                    sb.AppendLine($"  <div class='header-task'>TSK-{Order.Id:D4}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{GetLoc("LblClientData", "Client Data")}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblClient", "Client:")}</b> {SelectedCustomer?.FullName ?? Order.RequesterName}</div>");
                    string contactStr = IsCustomContactMethod ? CustomContactMethod : SelectedContactMethod;
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblContactMethod", "Contact Method:")}</b> {contactStr}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblContactName", "Contact Name:")}</b> {Order.RequesterName}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblColPhone", "Phone:")}</b> {Order.RequesterContact}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{GetLoc("LblEquipState", "Equipment State")}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblEquip", "Equipment:")}</b> {SelectedEquipment?.Brand} {SelectedEquipment?.Model}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblSN", "S/N:")}</b> {SelectedEquipment?.SerialNumber ?? "N/A"}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblState", "State:")}</b> {Order.EquipmentState}</div>");
                    sb.AppendLine($"<div class='info-line'><b>{GetLoc("LblCables", "Cables:")}</b> {Order.Cables}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{GetLoc("LblProblemDescTitle", "Problem Description")}</div>");
                    sb.AppendLine($"<div class='info-line'>{Order.IssueDescription}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='section'>");
                    sb.AppendLine($"<div class='section-title'>{GetLoc("LblWorkPerformedUpper", "WORK PERFORMED")}</div>");
                    sb.AppendLine($"<div class='info-line'>{Order.WorkPerformed ?? "N/A"}</div>");
                    sb.AppendLine("</div>");

                    if (AddedParts.Any())
                    {
                        sb.AppendLine($"<div class='section-title' style='border:none; margin-top:20px; padding-left:5px; page-break-after: avoid;'>{GetLoc("LblPartsUsedUpper", "PARTS USED")}</div>");
                        sb.AppendLine("<table>");
                        sb.AppendLine("<thead>");
                        sb.AppendLine($"<tr><th style='width: 50%;'>{GetLoc("LblPart", "Part")}</th><th class='text-center'>{GetLoc("LblQty", "Qty")}</th><th class='text-right'>{GetLoc("LblPrice", "Price")}</th><th class='text-right'>{GetLoc("LblTotal", "Total")}</th></tr>");
                        sb.AppendLine("</thead>");
                        sb.AppendLine("<tbody>");
                        foreach (var p in AddedParts)
                        {
                            sb.AppendLine($"<tr><td>{p.Name}</td><td class='text-center'>{p.Quantity}</td><td class='text-right'>€{p.UnitPrice:F2}</td><td class='text-right'>€{p.TotalPrice:F2}</td></tr>");
                        }
                        sb.AppendLine("</tbody>");
                        sb.AppendLine("</table>");
                    }

                    sb.AppendLine("<div class='checkout-section'>");
                    sb.AppendLine("<div class='totals-container'>");
                    sb.AppendLine($"<div class='total-line'>{GetLoc("LblLaborCost", "Labor Cost")}: €{LaborCost:F2}</div>");
                    sb.AppendLine($"<div class='total-line'>{GetLoc("LblPartsCost", "Parts Cost")}: €{PartsCost:F2}</div>");

                    if (IsUnderWarranty)
                        sb.AppendLine($"<div class='total-final'>{GetLoc("LblTotalCostUpper", "TOTAL COST:")} €0.00 {GetLoc("LblWarrantySuffix", "(WARRANTY)")}</div>");
                    else
                        sb.AppendLine($"<div class='total-final'>{GetLoc("LblTotalCostUpper", "TOTAL COST:")} €{TotalCost:F2}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='signatures'>");

                    sb.AppendLine("<div class='sig-box'>");
                    sb.AppendLine("<div style='height: 60px;'></div>");
                    sb.AppendLine($"<div class='sig-line'>{GetLoc("LblSignTech", "Technician Signature")}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<div class='sig-box'>");
                    if (!string.IsNullOrEmpty(Order.ClientSignatureBase64))
                    {
                        sb.AppendLine($"<img class='sig-img' src='data:image/png;base64,{Order.ClientSignatureBase64}' />");
                    }
                    else
                    {
                        sb.AppendLine("<div style='height: 60px;'></div>");
                    }
                    sb.AppendLine($"<div class='sig-line'>{GetLoc("LblSignClient", "Client Signature")}</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");

                    sb.AppendLine("<script>");
                    sb.AppendLine("window.onload = function() {");
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
                            Title = "Save or View Document",
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

        public async Task<WorkOrder?> SaveAsync()
        {
            string contactMethod = SelectedContactMethod;
            string contactValue = Order.RequesterContact?.Replace("+", "").Replace(" ", "") ?? "";

            if (contactMethod == "Phone" || contactMethod == "WhatsApp")
            {
                if (!string.IsNullOrWhiteSpace(contactValue) && !contactValue.All(char.IsDigit))
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert(GetLoc("LblValidationError", "Validation Error"), GetLoc("LblInvalidPhone", "Invalid Phone"), "OK");
                    return null;
                }
            }
            else if (contactMethod == "Email" || contactMethod == "E-mail")
            {
                if (!string.IsNullOrWhiteSpace(contactValue) && !contactValue.Contains("@"))
                {
                    if (Shell.Current != null) await Shell.Current.DisplayAlert(GetLoc("LblValidationError", "Validation Error"), GetLoc("LblInvalidEmail", "Invalid Email"), "OK");
                    return null;
                }
            }

            if (!IsValid())
            {
                if (Shell.Current != null) await Shell.Current.DisplayAlert(GetLoc("LblValidationError", "Validation Error"), GetLoc("LblValidationFailed", "Validation Failed"), "OK");
                return null;
            }

            Order.ContactMethod = IsCustomContactMethod ? CustomContactMethod : SelectedContactMethod;
            if (SelectedCustomer != null) Order.CustomerId = SelectedCustomer.Id;
            if (SelectedEquipment != null) Order.EquipmentId = SelectedEquipment.Id;
            if (SelectedTechnician != null) Order.TechnicianId = SelectedTechnician.Id;

            if (StartDate.HasValue && StartTime.HasValue) Order.WorkStartTime = StartDate.Value.Add(StartTime.Value);
            else Order.WorkStartTime = null;

            if (EndDate.HasValue && EndTime.HasValue) Order.WorkEndTime = EndDate.Value.Add(EndTime.Value);
            else Order.WorkEndTime = null;

            Order.HourlyRate = HourlyRate;
            Order.LaborCost = LaborCost;
            Order.PartsCost = PartsCost;
            Order.IsWarranty = IsUnderWarranty;
            Order.UsedPartsText = JsonSerializer.Serialize(AddedParts);

            Order.Customer = null;
            Order.Equipment = null;
            Order.Technician = null;

            bool success;
            if (Order.Id == 0) success = await _apiService.AddWorkOrderAsync(Order);
            else success = await _apiService.UpdateWorkOrderAsync(Order);

            if (success)
            {
                foreach (var part in AddedParts)
                {
                    var invItem = InventoryItems.FirstOrDefault(i => i.Id == part.InventoryItemId);
                    if (invItem != null) await _apiService.UpdateInventoryItemAsync(invItem);
                }

                foreach (var restored in _partsToRestore)
                {
                    await _apiService.UpdateInventoryItemAsync(restored);
                }
                _partsToRestore.Clear();

                return Order;
            }
            return null;
        }
    }
}