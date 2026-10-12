using CommunityToolkit.Maui.Views;
using ServiceDesk.Maui.ViewModels;
using ServiceDesk.Shared.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public partial class WorkOrderPopup : Popup
    {
        private Page? _parentPage;
        private string? _existingSignature;
        private bool _isClosing = false;
        private WorkOrder? _lastSavedOrder = null;

        public WorkOrderPopup(WorkOrderPopupViewModel vm, WorkOrder? order = null)
        {
            InitializeComponent();
            BindingContext = vm;

            _parentPage = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (_parentPage != null)
            {
                ApplyResponsiveStyles(_parentPage.Width, _parentPage.Height);
                _parentPage.SizeChanged += OnParentPageSizeChanged;
            }
            else
            {
                double w = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width / Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density;
                double h = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height / Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density;
                ApplyResponsiveStyles(w, h);
            }

            this.Closed += OnPopupClosed;

            if (order != null && order.Id != 0)
            {
                TitleLabel.Text = GetLoc("LblEditWorkOrder", "Edit Work Order");
                SaveButton.Text = GetLoc("LblSave", "Save");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");
                _existingSignature = order.ClientSignatureBase64;

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
            }
            else
            {
                TitleLabel.Text = GetLoc("LblNewTaskTitle", "New Work Order");
                SaveButton.Text = GetLoc("LblSave", "Save");
                SaveButton.BackgroundColor = Color.FromArgb("#198754");

                SavedSignatureImage.IsVisible = false;
                SignatureView.IsVisible = true;
            }

            _ = vm.InitializeAsync(order);
        }

        private void OnParentPageSizeChanged(object? sender, EventArgs e)
        {
            if (sender is Page page && page.Width > 0 && page.Height > 0)
            {
                ApplyResponsiveStyles(page.Width, page.Height);
            }
        }

        private void OnPopupClosed(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
        {
            if (_parentPage != null)
            {
                _parentPage.SizeChanged -= OnParentPageSizeChanged;
            }
            this.Closed -= OnPopupClosed;
        }

        private void ApplyResponsiveStyles(double width, double height)
        {
            if (width <= 0) return;

            MainBorder.HeightRequest = Math.Min(height * 0.95, 850);

            if (width >= 1200)
            {
                MainBorder.WidthRequest = 1100;
                MainContentLayout.Padding = new Thickness(24);
                SetTwoColumnsMode();
                ApplyFonts(24, 14, 14, 14, 28);
            }
            else if (width >= 900)
            {
                MainBorder.WidthRequest = 850;
                MainContentLayout.Padding = new Thickness(20);
                SetTwoColumnsMode();
                ApplyFonts(22, 13, 13, 13, 24);
            }
            else if (width >= 650)
            {
                MainBorder.WidthRequest = width * 0.95;
                MainContentLayout.Padding = new Thickness(15);
                SetOneColumnMode();
                ApplyFonts(20, 12, 12, 12, 22);
            }
            else
            {
                MainBorder.WidthRequest = width * 0.95;
                MainContentLayout.Padding = new Thickness(15);
                SetOneColumnMode();
                ApplyFonts(18, 11, 11, 11, 20);
            }
        }

        private void ApplyFonts(double titleSize, double labelSize, double entrySize, double btnSize, double totalSize)
        {
            Resources["PopupTitleSize"] = titleSize;
            Resources["PopupLabelSize"] = labelSize;
            Resources["PopupEntrySize"] = entrySize;
            Resources["PopupButtonSize"] = btnSize;
            Resources["PopupTotalSize"] = totalSize;
        }

        private void SetTwoColumnsMode()
        {
            SplitContentGrid.ColumnDefinitions.Clear();
            SplitContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.2, GridUnitType.Star)));
            SplitContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            SplitContentGrid.RowDefinitions.Clear();
            SplitContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftColumnLayout, 0); Grid.SetRow(LeftColumnLayout, 0);
            Grid.SetColumn(RightColumnLayout, 1); Grid.SetRow(RightColumnLayout, 0);

            SetGridColumns(CustomerEquipmentGrid, "1.5*, 1.5*, 1*");
            SetGridColumns(ContactMethodGrid, "1*, 1.5*, 1.2*");
            SetGridColumns(EquipStateGrid, "*, *");
            SetGridColumns(StartEndGrid, "*, *");
            SetGridColumns(StatusTechGrid, "*, *");
        }

        private void SetOneColumnMode()
        {
            SplitContentGrid.ColumnDefinitions.Clear();
            SplitContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SplitContentGrid.RowDefinitions.Clear();
            SplitContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            SplitContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftColumnLayout, 0); Grid.SetRow(LeftColumnLayout, 0);
            Grid.SetColumn(RightColumnLayout, 0); Grid.SetRow(RightColumnLayout, 1);

            SetGridColumns(CustomerEquipmentGrid, "*");
            SetGridColumns(ContactMethodGrid, "*");
            SetGridColumns(EquipStateGrid, "*");
            SetGridColumns(StartEndGrid, "*");
            SetGridColumns(StatusTechGrid, "*");
        }

        private void SetGridColumns(Grid grid, string columns)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            if (columns == "*")
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                int childrenCount = grid.Children.Count;
                for (int i = 0; i < childrenCount; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    Grid.SetColumn((BindableObject)grid.Children[i], 0);
                    Grid.SetRow((BindableObject)grid.Children[i], i);
                }
            }
            else
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                var parts = columns.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    string p = parts[i].Trim();
                    double val = p.Replace("*", "") == "" ? 1 : double.Parse(p.Replace("*", ""), System.Globalization.CultureInfo.InvariantCulture);
                    grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(val, GridUnitType.Star)));

                    if (i < grid.Children.Count)
                    {
                        Grid.SetColumn((BindableObject)grid.Children[i], i);
                        Grid.SetRow((BindableObject)grid.Children[i], 0);
                    }
                }
            }
        }

        private string GetLoc(string key, string fallback = "")
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val) && val != null)
            {
                string result = val.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(result)) return result;
            }
            return !string.IsNullOrWhiteSpace(fallback) ? fallback : key;
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
                var vm = (WorkOrderPopupViewModel)BindingContext;
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

                vm.Order.ClientSignatureBase64 = finalSignature;
                var result = await vm.SaveAsync();

                if (result != null)
                {
                    _lastSavedOrder = result;

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

                    TitleLabel.Text = GetLoc("LblEditWorkOrder", "Edit Work Order");
                    SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");
                    vm.IsNewOrder = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveError]: {ex}");
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

        private async void OnCancelClicked(object? sender, EventArgs e) => await SafeCloseAsync(_lastSavedOrder);

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