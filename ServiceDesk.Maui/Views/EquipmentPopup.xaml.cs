using CommunityToolkit.Maui.Views;
using ServiceDesk.Shared.Entities;
using ServiceDesk.Maui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public partial class EquipmentPopup : Popup
    {
        private int _editingEquipmentId;
        private readonly ApiService _apiService;
        private List<Customer> _customersList;
        private Page? _parentPage;

        public EquipmentPopup(ApiService apiService, IEnumerable<Customer> customers, Equipment? equipment = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _customersList = customers?.ToList() ?? new List<Customer>();
            CustomerPicker.ItemsSource = _customersList;

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

            if (equipment != null)
            {
                TitleLabel.Text = GetLoc("LblEditEquipmentTitle");
                SaveButton.Text = GetLoc("LblSave");
                SaveButton.BackgroundColor = Color.FromArgb("#0DCAF0");
                AddCustomerBtn.IsVisible = false;

                _editingEquipmentId = equipment.Id;
                BrandEntry.Text = equipment.Brand;
                ModelEntry.Text = equipment.Model;
                SerialEntry.Text = equipment.SerialNumber;

                if (equipment.CustomerId != 0)
                {
                    CustomerPicker.SelectedItem = _customersList.FirstOrDefault(c => c.Id == equipment.CustomerId);
                }
            }
            else
            {
                TitleLabel.Text = GetLoc("LblNewEquipmentTitle");
                SaveButton.Text = GetLoc("LblAddEquipmentBtn");
                SaveButton.BackgroundColor = Color.FromArgb("#198754");
                AddCustomerBtn.IsVisible = true;
            }
        }

        private void OnParentPageSizeChanged(object? sender, EventArgs e)
        {
            if (sender is Page page && page.Width > 0)
            {
                ApplyResponsiveStyles(page.Width);
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

        private void ApplyResponsiveStyles(double width)
        {
            if (width <= 0) return;

            if (width >= 1200)
            {
                Resources["PopupWidth"] = 440.0;
                Resources["PopupPadding"] = new Thickness(32, 32);
                ApplyFonts(24, 15, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                Resources["PopupWidth"] = 420.0;
                Resources["PopupPadding"] = new Thickness(24, 32);
                ApplyFonts(22, 14, 15, 15);
                ApplyButtonHeights(48);
            }
            else if (width >= 650)
            {
                Resources["PopupWidth"] = 400.0;
                Resources["PopupPadding"] = new Thickness(24, 24);
                ApplyFonts(20, 13, 14, 14);
                ApplyButtonHeights(44);
            }
            else
            {
                Resources["PopupWidth"] = 340.0;
                Resources["PopupPadding"] = new Thickness(20, 20);
                ApplyFonts(18, 12, 13, 13);
                ApplyButtonHeights(42);
            }
        }

        private void ApplyFonts(double titleSize, double labelSize, double entrySize, double btnSize)
        {
            Resources["PopupTitleSize"] = titleSize;
            Resources["PopupLabelSize"] = labelSize;
            Resources["PopupEntrySize"] = entrySize;
            Resources["PopupButtonSize"] = btnSize;
        }

        private void ApplyButtonHeights(double height)
        {
            CancelBtn.HeightRequest = height;
            SaveButton.HeightRequest = height;
            AddCustomerBtn.HeightRequest = height;
        }

        private string GetLoc(string key)
        {
            if (Application.Current != null && Application.Current.Resources.TryGetValue(key, out var val))
                return val?.ToString() ?? key;
            return key;
        }

        private async void OnAddCustomerClicked(object sender, EventArgs e)
        {
            var popup = new CustomerPopup(null);
            var result = await Shell.Current!.ShowPopupAsync(popup);
            if (result is Customer newCust)
            {
                await _apiService.AddCustomerAsync(newCust);
                var updated = await _apiService.GetCustomersAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (updated != null)
                    {
                        _customersList = updated.ToList();
                        CustomerPicker.ItemsSource = null;
                        CustomerPicker.ItemsSource = _customersList;
                        CustomerPicker.SelectedItem = _customersList.FirstOrDefault(c => c.FullName == newCust.FullName);
                    }
                });
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BrandEntry.Text) || string.IsNullOrWhiteSpace(ModelEntry.Text))
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync(GetLoc("LblValidationError"), GetLoc("LblEquipmentValidationMsg"), "OK");
                }
                return;
            }

            var selectedCustomer = CustomerPicker.SelectedItem as Customer;
            var result = new Equipment
            {
                Id = _editingEquipmentId,
                Brand = BrandEntry.Text,
                Model = ModelEntry.Text,
                SerialNumber = SerialEntry.Text,
                CustomerId = selectedCustomer?.Id ?? 0,
                Customer = null
            };
            Close(result);
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(null);
    }
}