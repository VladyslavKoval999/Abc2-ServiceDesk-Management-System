using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ServiceDesk.Maui.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public partial class ReportsPage : ContentPage
    {
        private readonly ReportsViewModel _viewModel;
        private double _lastWidth = -1;

        public ReportsPage(ReportsViewModel viewModel)
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = _viewModel = viewModel;

            _viewModel.OnDataLoaded += () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_viewModel.SelectedReportType == "Financial Overview" || _viewModel.SelectedReportType == "Top Technicians Workload")
                    {
                        ChartGraphicsView.Drawable = new ReportsBarChartDrawable { ViewModel = _viewModel };
                    }
                    else if (_viewModel.SelectedReportType == "Customer Insights" || _viewModel.SelectedReportType == "Inventory Health")
                    {
                        ChartGraphicsView.Drawable = new ReportsHorizontalBarChartDrawable { ViewModel = _viewModel };
                    }
                    else
                    {
                        ChartGraphicsView.Drawable = new ReportsPieChartDrawable { ViewModel = _viewModel };
                    }

                    ChartGraphicsView.Invalidate();
                });
            };

            AppSidebar.SidebarStateChanged += (s, e) =>
            {
                if (AppSidebar.IsDesktopMode)
                {
                    SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
                }
                UpdateResponsiveLayout(Width);
            };

            SizeChanged += OnReportsPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateResponsiveLayout(Width);
            if (_viewModel.LoadReportsDataCommand.CanExecute(null))
            {
                _viewModel.LoadReportsDataCommand.Execute(null);
            }
        }

        private void OnReportsPageSizeChanged(object? sender, EventArgs e)
        {
            if (Width <= 0 || Math.Abs(_lastWidth - Width) < 3) return;
            _lastWidth = Width;
            UpdateResponsiveLayout(Width);
        }

        private void UpdateResponsiveLayout(double width)
        {
            if (width <= 0) return;
            if (width >= 1200) ApplyDesktopLayout(width);
            else if (width >= 900) ApplyLargeTabletLayout(width);
            else if (width >= 650) ApplyTabletLayout(width);
            else ApplyPhoneLayout(width);
        }

        private void ApplyDesktopLayout(double screenWidth)
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();

            MainPageGrid.Padding = 25;
            MainPageGrid.RowSpacing = 20;

            ToolbarGrid.ColumnDefinitions.Clear();
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ToolbarGrid.RowDefinitions.Clear();
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ReportTypeLabel, 0); Grid.SetRow(ReportTypeLabel, 0);
            Grid.SetColumn(PickerBorder, 1); Grid.SetRow(PickerBorder, 0);
            Grid.SetColumnSpan(PickerBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 2); Grid.SetRow(ActionButtonsLayout, 0);
            Grid.SetColumnSpan(ActionButtonsLayout, 1);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End;

            PickerBorder.WidthRequest = 380;

            SummaryCardsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 4; i++) SummaryCardsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SummaryCardsGrid.RowDefinitions.Clear();
            SummaryCardsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(Card1, 0); Grid.SetRow(Card1, 0);
            Grid.SetColumn(Card2, 1); Grid.SetRow(Card2, 0);
            Grid.SetColumn(Card3, 2); Grid.SetRow(Card3, 0);
            Grid.SetColumn(Card4, 3); Grid.SetRow(Card4, 0);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(6.8, GridUnitType.Star)));
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3.2, GridUnitType.Star)));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 1); Grid.SetRow(RightContentColumn, 0);

            ApplyFonts(28, 14, 18, 26, 14, 18, 14, 13);
            SetButtonSize(14, 16, 8);
            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyLargeTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();

            MainPageGrid.Padding = 20;
            MainPageGrid.RowSpacing = 16;

            ToolbarGrid.ColumnDefinitions.Clear();
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ToolbarGrid.RowDefinitions.Clear();
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ReportTypeLabel, 0); Grid.SetRow(ReportTypeLabel, 0);
            Grid.SetColumn(PickerBorder, 1); Grid.SetRow(PickerBorder, 0);
            Grid.SetColumnSpan(PickerBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 2); Grid.SetRow(ActionButtonsLayout, 0);
            Grid.SetColumnSpan(ActionButtonsLayout, 1);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End;

            PickerBorder.WidthRequest = 280;

            SummaryCardsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) SummaryCardsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SummaryCardsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) SummaryCardsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(Card1, 0); Grid.SetRow(Card1, 0);
            Grid.SetColumn(Card2, 1); Grid.SetRow(Card2, 0);
            Grid.SetColumn(Card3, 0); Grid.SetRow(Card3, 1);
            Grid.SetColumn(Card4, 1); Grid.SetRow(Card4, 1);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(6, GridUnitType.Star)));
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(4, GridUnitType.Star)));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 1); Grid.SetRow(RightContentColumn, 0);

            ApplyFonts(24, 13, 16, 22, 13, 16, 13, 12);
            SetButtonSize(13, 12, 7);
            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = 15;
            MainPageGrid.RowSpacing = 14;

            ToolbarGrid.ColumnDefinitions.Clear();
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ToolbarGrid.RowDefinitions.Clear();
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ReportTypeLabel, 0); Grid.SetRow(ReportTypeLabel, 0);
            Grid.SetColumn(PickerBorder, 1); Grid.SetRow(PickerBorder, 0);
            Grid.SetColumnSpan(PickerBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 0); Grid.SetRow(ActionButtonsLayout, 1);
            Grid.SetColumnSpan(ActionButtonsLayout, 2);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start;

            PickerBorder.WidthRequest = -1;

            SummaryCardsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) SummaryCardsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SummaryCardsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) SummaryCardsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(Card1, 0); Grid.SetRow(Card1, 0);
            Grid.SetColumn(Card2, 1); Grid.SetRow(Card2, 0);
            Grid.SetColumn(Card3, 0); Grid.SetRow(Card3, 1);
            Grid.SetColumn(Card4, 1); Grid.SetRow(Card4, 1);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 0); Grid.SetRow(RightContentColumn, 1);

            ApplyFonts(21, 12, 15, 20, 12, 15, 12, 11);
            SetButtonSize(12, 10, 6);
            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyPhoneLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = 10;
            MainPageGrid.RowSpacing = 10;

            ToolbarGrid.ColumnDefinitions.Clear();
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ToolbarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ToolbarGrid.RowDefinitions.Clear();
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ToolbarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ReportTypeLabel, 0); Grid.SetRow(ReportTypeLabel, 0);
            Grid.SetColumn(PickerBorder, 1); Grid.SetRow(PickerBorder, 0);
            Grid.SetColumnSpan(PickerBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 0); Grid.SetRow(ActionButtonsLayout, 1);
            Grid.SetColumnSpan(ActionButtonsLayout, 2);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start;

            PickerBorder.WidthRequest = -1;

            SummaryCardsGrid.ColumnDefinitions.Clear();
            SummaryCardsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            SummaryCardsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 4; i++) SummaryCardsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(Card1, 0); Grid.SetRow(Card1, 0);
            Grid.SetColumn(Card2, 0); Grid.SetRow(Card2, 1);
            Grid.SetColumn(Card3, 0); Grid.SetRow(Card3, 2);
            Grid.SetColumn(Card4, 0); Grid.SetRow(Card4, 3);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 0); Grid.SetRow(RightContentColumn, 1);

            ApplyFonts(18, 11, 14, 18, 11, 14, 11, 10);
            SetButtonSize(11, 10, 8);
            ConfigureTablesWidth(screenWidth);
        }

        private void SetButtonSize(double fontSize, double horizontalPadding, double verticalPadding)
        {
            RefreshButton.FontSize = fontSize;
            ExportPdfButton.FontSize = fontSize;

            RefreshButton.Padding = new Thickness(horizontalPadding, verticalPadding);
            ExportPdfButton.Padding = new Thickness(horizontalPadding, verticalPadding);
        }

        private void ConfigureTablesWidth(double screenWidth)
        {
            double currentSidebarWidth = SidebarColumn.Width.IsAbsolute ? SidebarColumn.Width.Value : 0;
            double availableWidth = screenWidth - currentSidebarWidth - MainPageGrid.Padding.Left - MainPageGrid.Padding.Right;

            double leftColWidth = availableWidth;

            if (screenWidth >= 1200)
            {
                leftColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.68;
            }
            else if (screenWidth >= 900)
            {
                leftColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.6;
            }

            double finWidth = Math.Max(leftColWidth, 800);
            FinancialTableLayout.WidthRequest = finWidth;
            FinancialHeaderGrid.WidthRequest = finWidth;
            FinancialCollection.WidthRequest = finWidth;

            double custWidth = Math.Max(leftColWidth, 600);
            CustomerTableLayout.WidthRequest = custWidth;
            CustomerHeaderGrid.WidthRequest = custWidth;
            CustomerCollection.WidthRequest = custWidth;

            double taskWidth = Math.Max(leftColWidth, 950);
            TaskTableLayout.WidthRequest = taskWidth;
            TaskHeaderGrid.WidthRequest = taskWidth;
            TaskCollection.WidthRequest = taskWidth;

            double techWidth = Math.Max(leftColWidth, 750);
            TechTableLayout.WidthRequest = techWidth;
            TechHeaderGrid.WidthRequest = techWidth;
            TechCollection.WidthRequest = techWidth;

            double equipWidth = Math.Max(leftColWidth, 500);
            EquipTableLayout.WidthRequest = equipWidth;
            EquipHeaderGrid.WidthRequest = equipWidth;
            EquipCollection.WidthRequest = equipWidth;

            double invWidth = Math.Max(leftColWidth, 750);
            InventoryTableLayout.WidthRequest = invWidth;
            InventoryHeaderGrid.WidthRequest = invWidth;
            InventoryCollection.WidthRequest = invWidth;
        }

        private void ApplyFonts(double pageTitle, double reportType, double tableTitle, double cardValue, double cardTitle, double chartTitle, double buttonFont, double tableFont)
        {
            PageTitleLabel.FontSize = pageTitle;
            ReportTypePicker.FontSize = reportType;

            TableTitle1.FontSize = tableTitle;
            TableTitle2.FontSize = tableTitle;
            TableTitle3.FontSize = tableTitle;
            TableTitle4.FontSize = tableTitle;
            TableTitle5.FontSize = tableTitle;
            TableTitle6.FontSize = tableTitle;

            Card1Value.FontSize = cardValue;
            Card2Value.FontSize = cardValue;
            Card3Value.FontSize = cardValue;
            Card4Value.FontSize = cardValue;

            Card1Title.FontSize = cardTitle;
            Card2Title.FontSize = cardTitle;
            Card3Title.FontSize = cardTitle;
            Card4Title.FontSize = cardTitle;

            ChartTitleLabel.FontSize = chartTitle;

            Resources["TableHeaderFontSize"] = tableFont;
            Resources["TableRowFontSize"] = tableFont;
        }

        private async void OnMenuToggled(object? sender, TappedEventArgs e)
        {
            await Task.WhenAll(
                MenuBurgerPath?.RotateToAsync(MenuBurgerPath.Rotation + 180, 250, Easing.CubicInOut) ?? Task.CompletedTask,
                MenuBurgerPath?.ScaleToAsync(0.8, 125).ContinueWith(t => MenuBurgerPath.ScaleToAsync(1.0, 125)) ?? Task.CompletedTask
            );

            AppSidebar.Toggle();
        }
    }

    public class ReportsPieChartDrawable : IDrawable
    {
        public ReportsViewModel? ViewModel { get; set; }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (ViewModel == null || !ViewModel.ChartData.Any()) return;
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;
            float radius = Math.Min(centerX, centerY) - 10;
            RectF rect = new RectF(centerX - radius, centerY - radius, radius * 2, radius * 2);
            float startAngle = 90;

            foreach (var item in ViewModel.ChartData)
            {
                float sweepAngle = (float)(item.Percentage * -360);
                canvas.FillColor = item.SegmentColor;
                PathF path = new PathF();
                path.MoveTo(centerX, centerY);
                path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle + sweepAngle, true);
                path.LineTo(centerX, centerY);
                path.Close();
                canvas.FillPath(path);
                startAngle += sweepAngle;
            }
            canvas.FillColor = Colors.White;
            canvas.FillCircle(centerX, centerY, radius * 0.65f);
        }
    }

    public class ReportsBarChartDrawable : IDrawable
    {
        public ReportsViewModel? ViewModel { get; set; }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (ViewModel == null || !ViewModel.ChartData.Any()) return;
            var items = ViewModel.ChartData.ToList();
            int count = items.Count;

            float marginX = 20, marginYTop = 30, marginYBottom = 45;
            float chartWidth = dirtyRect.Width - (marginX * 2);
            float chartHeight = dirtyRect.Height - marginYTop - marginYBottom;
            float barSpacing = count > 5 ? 10 : 25;
            float barWidth = (chartWidth - (barSpacing * (count - 1))) / count;

            double maxVal = items.Max(i => i.Percentage);
            if (maxVal == 0) maxVal = 1;

            canvas.StrokeColor = Color.FromArgb("#DEE2E6");
            canvas.StrokeSize = 2;
            canvas.DrawLine(marginX - 10, dirtyRect.Height - marginYBottom, dirtyRect.Width - marginX + 10, dirtyRect.Height - marginYBottom);

            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                float barHeight = (float)((item.Percentage / maxVal) * chartHeight);
                if (barHeight < 5 && item.Percentage > 0) barHeight = 5;

                float x = marginX + (i * (barWidth + barSpacing));
                float y = dirtyRect.Height - marginYBottom - barHeight;

                canvas.FillColor = item.SegmentColor;
                canvas.FillRoundedRectangle(x, y, barWidth, barHeight, 4);

                canvas.FontColor = Color.FromArgb("#212529");
                canvas.FontSize = 11;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(item.DisplayValue, x - 15, y - 20, barWidth + 30, 20, HorizontalAlignment.Center, VerticalAlignment.Center);

                canvas.FontColor = Color.FromArgb("#6C757D");
                canvas.FontSize = 10;
                string safeLabel = item.Label;
                if (safeLabel.Contains(" ")) safeLabel = safeLabel.Replace(" ", "\n");
                canvas.DrawString(safeLabel, x - 15, dirtyRect.Height - marginYBottom + 8, barWidth + 30, 40, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }

    public class ReportsHorizontalBarChartDrawable : IDrawable
    {
        public ReportsViewModel? ViewModel { get; set; }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (ViewModel == null || !ViewModel.ChartData.Any()) return;
            var items = ViewModel.ChartData.ToList();
            int count = items.Count;

            float marginXLeft = 120, marginXRight = 80, marginY = 10;
            float chartWidth = dirtyRect.Width - marginXLeft - marginXRight;
            float chartHeight = dirtyRect.Height - (marginY * 2);
            float barSpacing = count > 5 ? 8 : 15;
            float barHeight = (chartHeight - (barSpacing * (count - 1))) / count;

            if (barHeight < 10) barHeight = 10;

            double maxVal = items.Max(i => i.Percentage);
            if (maxVal == 0) maxVal = 1;

            canvas.StrokeColor = Color.FromArgb("#DEE2E6");
            canvas.StrokeSize = 2;
            canvas.DrawLine(marginXLeft, marginY, marginXLeft, dirtyRect.Height - marginY);

            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                float barWidth = (float)((item.Percentage / maxVal) * chartWidth);
                if (barWidth < 5 && item.Percentage > 0) barWidth = 5;

                float y = marginY + (i * (barHeight + barSpacing));

                canvas.FillColor = item.SegmentColor;
                canvas.FillRoundedRectangle(marginXLeft, y, barWidth, barHeight, 4);

                canvas.FontColor = Color.FromArgb("#6C757D");
                canvas.FontSize = 11;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;

                string label = item.Label.Length > 16 ? item.Label.Substring(0, 14) + ".." : item.Label;
                canvas.DrawString(label, 0, y, marginXLeft - 10, barHeight, HorizontalAlignment.Right, VerticalAlignment.Center);

                canvas.FontColor = Color.FromArgb("#212529");
                canvas.FontSize = 11;
                canvas.DrawString(item.DisplayValue, marginXLeft + barWidth + 10, y, marginXRight, barHeight, HorizontalAlignment.Left, VerticalAlignment.Center);
            }
        }
    }
}