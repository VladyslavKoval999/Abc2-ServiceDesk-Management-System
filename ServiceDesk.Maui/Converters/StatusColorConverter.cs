using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace ServiceDesk.Maui.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string status = value as string ?? string.Empty;
            bool isBackground = parameter?.ToString() == "Background";

            return status switch
            {
                "Completed" or "Paid" or "Accepted" =>
                    isBackground ? Color.FromArgb("#D1FAE5") : Color.FromArgb("#065F46"),
                "Pending Start" or "Pending" or "Unpaid" =>
                    isBackground ? Color.FromArgb("#FEF3C7") : Color.FromArgb("#92400E"),
                "In Progress" =>
                    isBackground ? Color.FromArgb("#DBEAFE") : Color.FromArgb("#1E40AF"),
                "Waiting for Parts" =>
                    isBackground ? Color.FromArgb("#F3E8FF") : Color.FromArgb("#6B21A8"),
                "Rejected" =>
                    isBackground ? Color.FromArgb("#FEE2E2") : Color.FromArgb("#991B1B"),
                _ =>
                    isBackground ? Color.FromArgb("#F3F4F6") : Color.FromArgb("#374151")
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}