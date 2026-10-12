using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Maui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public class EquipmentWrapper
    {
        public Equipment Equipment { get; set; } = null!;
        public string DisplayText
        {
            get
            {
                string sn = string.IsNullOrWhiteSpace(Equipment.SerialNumber) ? "N/A" : Equipment.SerialNumber;
                return $"{Equipment.Brand} {Equipment.Model} (S/N: {sn})";
            }
        }
    }

    public partial class QuotationPopup : Popup
    {
        private Quotation _currentQuotation;
        private readonly ApiService _apiService;
        private List<Customer> _customersList;
        private List<EquipmentWrapper> _equipmentsList;
        private Page? _parentPage;
        private string? _existingSignature;
        private bool _isClosing = false;

        private Quotation? _lastSavedQuotation = null;

        public QuotationPopup(ApiService apiService, IEnumerable<Customer> customers, IEnumerable<Equipment> equipments, Quotation? quotation = null)
        {
            InitializeComponent();
            _apiService = apiService;

            _parentPage = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (_parentPage != null)
            {
                ApplyResponsiveStyles(_parentPage.Width);
                _parentPage.SizeChanged += OnParentPageSizeChanged;
            }
            else
            {
                ApplyResponsiveStyles(Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density);
            }

            this.Closed += OnPopupClosed;

            _customersList = customers?.ToList() ?? new List<Customer>();
            _equipmentsList = equipments?.Select(e => new EquipmentWrapper { Equipment = e }).ToList() ?? new List<EquipmentWrapper>();

            CustomerPicker.ItemsSource = _customersList;
            EquipmentPicker.ItemsSource = _equipmentsList;

            if (quotation != null)
            {
                _currentQuotation = quotation;

                TitleLabel.Text = GetLoc("LblEditQuotationTitle", "Edit Quotation");
                SaveButton.Text = GetLoc("LblSave", "Save");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");
                AddEquipmentBtn.IsVisible = false;

                CostEntry.Text = quotation.EstimatedCost.ToString(System.Globalization.CultureInfo.InvariantCulture);
                ProblemEntry.Text = quotation.ProblemDescription;
                StatusPicker.SelectedItem = quotation.Status;

                _existingSignature = quotation.ClientSignatureBase64;
                if (!string.IsNullOrEmpty(_existingSignature))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(_existingSignature);
                        SavedSignatureImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        SavedSignatureImage.IsVisible = true;
                        SignatureView.IsVisible = false;
                    }
                    catch
                    {
                        SavedSignatureImage.IsVisible = false;
                        SignatureView.IsVisible = true;
                    }
                }
                else
                {
                    SavedSignatureImage.IsVisible = false;
                    SignatureView.IsVisible = true;
                }

                if (quotation.CustomerId != 0)
                    CustomerPicker.SelectedItem = _customersList.FirstOrDefault(c => c.Id == quotation.CustomerId);

                if (!string.IsNullOrEmpty(quotation.EquipmentDetails))
                {
                    var matchedEquip = _equipmentsList.FirstOrDefault(w =>
                        quotation.EquipmentDetails.Contains(w.Equipment.Brand) ||
                        quotation.EquipmentDetails.Contains(w.Equipment.SerialNumber ?? ""));

                    if (matchedEquip != null) EquipmentPicker.SelectedItem = matchedEquip;
                }
            }
            else
            {
                _currentQuotation = new Quotation
                {
                    CreatedAt = DateTime.Now,
                    Status = "Pending Start"
                };

                TitleLabel.Text = GetLoc("LblCreateQuotationTitle", "Create Quotation");
                SaveButton.Text = GetLoc("LblAddQuotationBtn", "Save Details");
                SaveButton.BackgroundColor = Color.FromArgb("#198754");
                StatusPicker.SelectedItem = "Pending Start";
                AddEquipmentBtn.IsVisible = true;

                SavedSignatureImage.IsVisible = false;
                SignatureView.IsVisible = true;
            }
        }

        private void OnParentPageSizeChanged(object? sender, EventArgs e)
        {
            if (sender is Page page && page.Width > 0)
                ApplyResponsiveStyles(page.Width);
        }

        private void OnPopupClosed(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
        {
            if (_parentPage != null) _parentPage.SizeChanged -= OnParentPageSizeChanged;
            this.Closed -= OnPopupClosed;
        }

        private void ApplyResponsiveStyles(double width)
        {
            if (width <= 0) return;
            MainBorder.MaximumHeightRequest = 850;
            if (width >= 1200) { Resources["PopupWidth"] = 450.0; Resources["PopupPadding"] = new Thickness(32, 32); ApplyFonts(24, 15, 15, 15); ApplyButtonHeights(48); }
            else if (width >= 900) { Resources["PopupWidth"] = 420.0; Resources["PopupPadding"] = new Thickness(24, 32); ApplyFonts(22, 14, 15, 15); ApplyButtonHeights(48); }
            else if (width >= 650) { Resources["PopupWidth"] = 400.0; Resources["PopupPadding"] = new Thickness(24, 24); ApplyFonts(20, 13, 14, 14); ApplyButtonHeights(44); }
            else { Resources["PopupWidth"] = 340.0; Resources["PopupPadding"] = new Thickness(20, 20); ApplyFonts(18, 12, 13, 13); ApplyButtonHeights(42); }
        }

        private void ApplyFonts(double tSize, double lSize, double eSize, double bSize) { Resources["PopupTitleSize"] = tSize; Resources["PopupLabelSize"] = lSize; Resources["PopupEntrySize"] = eSize; Resources["PopupButtonSize"] = bSize; }
        private void ApplyButtonHeights(double height) { CancelBtn.HeightRequest = height; SaveButton.HeightRequest = height; AddEquipmentBtn.HeightRequest = height; }

        private string GetLoc(string key, string fallback = "")
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val) && val != null)
            {
                string result = val.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(result)) return result;
            }
            return !string.IsNullOrWhiteSpace(fallback) ? fallback : key;
        }

        private async void OnAddEquipmentClicked(object sender, EventArgs e)
        {
            var popup = new EquipmentPopup(_apiService, _customersList, null);
            var result = await Shell.Current!.ShowPopupAsync(popup);
            if (result is Equipment newEquip)
            {
                await _apiService.AddEquipmentAsync(newEquip);
                var updated = await _apiService.GetEquipmentAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (updated != null)
                    {
                        _equipmentsList = updated.Select(eq => new EquipmentWrapper { Equipment = eq }).ToList();
                        EquipmentPicker.ItemsSource = null; EquipmentPicker.ItemsSource = _equipmentsList;
                        EquipmentPicker.SelectedItem = _equipmentsList.FirstOrDefault(w => w.Equipment.SerialNumber == newEquip.SerialNumber || w.Equipment.Brand == newEquip.Brand);
                    }
                });
            }
        }

        private void OnClearSignatureClicked(object sender, EventArgs e)
        {
            SignatureView.Clear();
            _existingSignature = null;
            SavedSignatureImage.IsVisible = false;
            SignatureView.IsVisible = true;
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (_isClosing || !SaveButton.IsEnabled) return;

            SaveButton.IsEnabled = false;
            CancelBtn.IsEnabled = false;

            try
            {
                var selectedCustomer = CustomerPicker.SelectedItem as Customer;
                var selectedEquipmentWrapper = EquipmentPicker.SelectedItem as EquipmentWrapper;

                string safeCostStr = string.IsNullOrWhiteSpace(CostEntry.Text) ? "0" : CostEntry.Text.Replace(",", ".");
                if (selectedCustomer == null || !decimal.TryParse(safeCostStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cost))
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblQuotationValidationMsg", "Please check your inputs and try again."), "OK");

                    SaveButton.IsEnabled = true;
                    CancelBtn.IsEnabled = true;
                    return;
                }

                string? finalSignature = _existingSignature;

                if (SignatureView.IsVisible && SignatureView.Lines.Any())
                {
                    try
                    {
                        var stream = await DrawingView.GetImageStream(
                            SignatureView.Lines,
                            new Size(300, 150),
                            new SolidColorBrush(Colors.White));

                        if (stream != null)
                        {
                            using var ms = new MemoryStream();
                            await stream.CopyToAsync(ms);
                            finalSignature = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    catch
                    {
                        var fallbackStream = await SignatureView.GetImageStream(300, 150);
                        if (fallbackStream != null)
                        {
                            using var ms = new MemoryStream();
                            await fallbackStream.CopyToAsync(ms);
                            finalSignature = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }

                _currentQuotation.CustomerId = selectedCustomer.Id;
                _currentQuotation.EstimatedCost = cost;
                _currentQuotation.EquipmentDetails = selectedEquipmentWrapper?.DisplayText ?? "";
                _currentQuotation.ProblemDescription = ProblemEntry.Text ?? "";
                _currentQuotation.Status = StatusPicker.SelectedItem?.ToString() ?? "Pending Start";
                _currentQuotation.ClientSignatureBase64 = finalSignature;

                _currentQuotation.Customer = null;

                bool isSuccess = false;
                if (_currentQuotation.Id == 0)
                    isSuccess = await _apiService.AddQuotationAsync(_currentQuotation);
                else
                    isSuccess = await _apiService.UpdateQuotationAsync(_currentQuotation);

                if (isSuccess)
                {
                    _lastSavedQuotation = _currentQuotation;

                    if (Application.Current?.Windows.Count > 0)
                    {
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync(
                            GetLoc("LblSuccess", "Success"),
                            GetLoc("LblSavedSuccessfully", "Saved successfully."),
                            "OK");
                    }

                    _existingSignature = finalSignature;
                    if (!string.IsNullOrEmpty(_existingSignature))
                    {
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(_existingSignature);
                            SavedSignatureImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                            SavedSignatureImage.IsVisible = true;
                            SignatureView.IsVisible = false;
                            SignatureView.Clear();
                        }
                        catch
                        {
                            SavedSignatureImage.IsVisible = false;
                            SignatureView.IsVisible = true;
                        }
                    }

                    TitleLabel.Text = GetLoc("LblEditQuotationTitle", "Edit Quotation");
                    SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");
                    AddEquipmentBtn.IsVisible = false;
                }
                else
                {
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Failed to save quotation on server.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", $"Could not save: {ex.Message}", "OK");
            }
            finally
            {
                if (!_isClosing)
                {
                    SaveButton.IsEnabled = true;
                    CancelBtn.IsEnabled = true;
                }
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e) => await SafeCloseAsync(_lastSavedQuotation);

        private async Task SafeCloseAsync(object? result)
        {
            if (_isClosing) return;
            _isClosing = true;

            try
            {
                if (SignatureView != null) SignatureView.Clear();
                if (SignatureContainer != null) SignatureContainer.Content = null;

                await Task.Delay(250);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try { Close(result); } catch { }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SafeCloseError]: {ex}");
                MainThread.BeginInvokeOnMainThread(() => { try { Close(result); } catch { } });
            }
        }

        private async void OnAskAiClicked(object sender, EventArgs e)
        {
            string text = ProblemEntry.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                if (Application.Current?.Windows.Count > 0)
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("AI", GetLoc("LblAiEmptyPrompt", "Please describe the problem first."), "OK");
                return;
            }

            var btn = sender as Button;
            string originalText = btn?.Text ?? "Ask AI ✨";
            if (btn != null) { btn.IsEnabled = false; btn.Text = GetLoc("LblAiThinking", "Thinking..."); }

            try
            {
                var aiService = Application.Current?.Handler?.MauiContext?.Services.GetService<ServiceDesk.Maui.Services.GeminiAiService>();
                if (aiService != null)
                {
                    var aiResult = await aiService.AskAiAsync(text, isContextAnalysis: true);
                    if (Application.Current?.Windows.Count > 0)
                        await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblAiDiagnostic", "AI Diagnostic"), aiResult.Message, "OK");
                }
            }
            finally
            {
                if (btn != null) { btn.IsEnabled = true; btn.Text = originalText; }
            }
        }
    }
}