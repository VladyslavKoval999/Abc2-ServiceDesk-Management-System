using CommunityToolkit.Maui.Views;
using System;
using Microsoft.Maui.Controls;

namespace ServiceDesk.Maui.Views
{
    public partial class DateTimePickerPopup : Popup
    {
        private Page? _parentPage;

        public DateTimePickerPopup(DateTime initialDate, TimeSpan initialTime)
        {
            InitializeComponent();

            PopupDatePicker.Date = initialDate;
            PopupTimePicker.Time = initialTime;

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
                MainBorder.WidthRequest = 400;
                MainBorder.Padding = new Thickness(32);
                ApplyFonts(24, 14, 15, 14);
                ApplyButtonHeights(48);
            }
            else if (width >= 900)
            {
                MainBorder.WidthRequest = 360;
                MainBorder.Padding = new Thickness(24);
                ApplyFonts(18, 13, 15, 14);
                ApplyButtonHeights(44);
            }
            else if (width >= 650)
            {
                MainBorder.WidthRequest = 340;
                MainBorder.Padding = new Thickness(20);
                ApplyFonts(17, 12, 14, 13);
                ApplyButtonHeights(42);
            }
            else
            {
                MainBorder.WidthRequest = 300;
                MainBorder.Padding = new Thickness(18);
                ApplyFonts(16, 11, 13, 12);
                ApplyButtonHeights(40);
            }
        }

        private void ApplyFonts(double titleSize, double labelSize, double pickerSize, double btnSize)
        {
            TitleLabel.FontSize = titleSize;
            DateTitleLabel.FontSize = labelSize;
            TimeTitleLabel.FontSize = labelSize;
            PopupDatePicker.FontSize = pickerSize;
            PopupTimePicker.FontSize = pickerSize;
            CancelBtn.FontSize = btnSize;
            SaveBtn.FontSize = btnSize;
        }

        private void ApplyButtonHeights(double height)
        {
            CancelBtn.HeightRequest = height;
            SaveBtn.HeightRequest = height;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            DateTime safeDate = PopupDatePicker.Date is DateTime d ? d : DateTime.Today;
            TimeSpan safeTime = PopupTimePicker.Time is TimeSpan t ? t : TimeSpan.Zero;
            Close(safeDate.Add(safeTime));
        }

        private void OnCancelClicked(object sender, EventArgs e) => Close(null);
    }
}