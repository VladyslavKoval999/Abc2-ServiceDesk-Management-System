using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ServiceDesk.Maui.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ServiceDesk.Maui.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;
        private double _lastWidth = -1;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = _viewModel = viewModel;

            var chartDrawer = new BarChartDrawable { ViewModel = _viewModel };
            ChartGraphicsView.Drawable = chartDrawer;

            _viewModel.OnDataLoaded += () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
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

            SizeChanged += OnDashboardSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateResponsiveLayout(Width);
            if (_viewModel.LoadDashboardDataCommand.CanExecute(null))
            {
                _viewModel.LoadDashboardDataCommand.Execute(null);
            }
        }

        private void OnDashboardSizeChanged(object? sender, EventArgs e)
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

            DashboardGrid.Padding = 25;
            DashboardGrid.RowSpacing = 25;

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            StatisticsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 4; i++) StatisticsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            StatisticsGrid.RowDefinitions.Clear();
            StatisticsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ResetStatisticsPositions();

            QuickActionsGrid.ColumnDefinitions.Clear();
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            QuickButtonsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 4; i++) QuickButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickButtonsGrid.RowDefinitions.Clear();
            QuickButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ResetQuickButtonPositions();

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(6, GridUnitType.Star)));
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(4, GridUnitType.Star)));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 1); Grid.SetRow(RightContentColumn, 0);

            ApplyFonts(28, 15, 13, 24, 16, 14, 18, 14, 16, 10, 14, 16, 18);
            ApplySizes(48, 48, 40, 26);
            ApplySpacing(20, 15, 25, 20);

            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyLargeTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();

            DashboardGrid.Padding = 20;
            DashboardGrid.RowSpacing = 20;

            StatisticsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) StatisticsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            StatisticsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) StatisticsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CustomersCard, 0); Grid.SetRow(CustomersCard, 0);
            Grid.SetColumn(RevenueCard, 1); Grid.SetRow(RevenueCard, 0);
            Grid.SetColumn(PendingCard, 0); Grid.SetRow(PendingCard, 1);
            Grid.SetColumn(LowStockCard, 1); Grid.SetRow(LowStockCard, 1);

            QuickActionsGrid.ColumnDefinitions.Clear();
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickActionsGrid.RowDefinitions.Clear();
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetColumn(QuickActionsLabel, 0); Grid.SetRow(QuickActionsLabel, 0);
            Grid.SetColumn(QuickButtonsGrid, 0); Grid.SetRow(QuickButtonsGrid, 1);

            QuickButtonsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) QuickButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickButtonsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) QuickButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(NewTaskButton, 0); Grid.SetRow(NewTaskButton, 0);
            Grid.SetColumn(CreateInvoiceButton, 1); Grid.SetRow(CreateInvoiceButton, 0);
            Grid.SetColumn(ManageStockButton, 0); Grid.SetRow(ManageStockButton, 1);
            Grid.SetColumn(AnalyticsButton, 1); Grid.SetRow(AnalyticsButton, 1);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 1); Grid.SetRow(RightContentColumn, 0);

            ApplyFonts(24, 14, 12, 21, 15, 13, 16, 13, 14, 9, 12, 14, 16);
            ApplySizes(42, 42, 36, 23);
            ApplySpacing(15, 12, 15, 15);

            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            DashboardGrid.Padding = 15;
            DashboardGrid.RowSpacing = 15;

            StatisticsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) StatisticsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            StatisticsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) StatisticsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CustomersCard, 0); Grid.SetRow(CustomersCard, 0);
            Grid.SetColumn(RevenueCard, 1); Grid.SetRow(RevenueCard, 0);
            Grid.SetColumn(PendingCard, 0); Grid.SetRow(PendingCard, 1);
            Grid.SetColumn(LowStockCard, 1); Grid.SetRow(LowStockCard, 1);

            QuickActionsGrid.ColumnDefinitions.Clear();
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickActionsGrid.RowDefinitions.Clear();
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetColumn(QuickActionsLabel, 0); Grid.SetRow(QuickActionsLabel, 0);
            Grid.SetColumn(QuickButtonsGrid, 0); Grid.SetRow(QuickButtonsGrid, 1);

            QuickButtonsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 2; i++) QuickButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickButtonsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 2; i++) QuickButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(NewTaskButton, 0); Grid.SetRow(NewTaskButton, 0);
            Grid.SetColumn(CreateInvoiceButton, 1); Grid.SetRow(CreateInvoiceButton, 0);
            Grid.SetColumn(ManageStockButton, 0); Grid.SetRow(ManageStockButton, 1);
            Grid.SetColumn(AnalyticsButton, 1); Grid.SetRow(AnalyticsButton, 1);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 0); Grid.SetRow(RightContentColumn, 1);

            ApplyFonts(21, 13, 11, 19, 14, 12, 15, 12, 13, 8, 11, 13, 15);
            ApplySizes(38, 38, 34, 21);
            ApplySpacing(12, 10, 12, 12);

            ConfigureTablesWidth(screenWidth);
        }

        private void ApplyPhoneLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            DashboardGrid.Padding = 10;
            DashboardGrid.RowSpacing = 10;

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            UserInfoLayout.Spacing = 6;

            StatisticsGrid.ColumnDefinitions.Clear();
            StatisticsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            StatisticsGrid.RowDefinitions.Clear();
            for (int i = 0; i < 4; i++) StatisticsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CustomersCard, 0); Grid.SetRow(CustomersCard, 0);
            Grid.SetColumn(RevenueCard, 0); Grid.SetRow(RevenueCard, 1);
            Grid.SetColumn(PendingCard, 0); Grid.SetRow(PendingCard, 2);
            Grid.SetColumn(LowStockCard, 0); Grid.SetRow(LowStockCard, 3);

            QuickActionsGrid.ColumnDefinitions.Clear();
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickActionsGrid.RowDefinitions.Clear();
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetColumn(QuickActionsLabel, 0); Grid.SetRow(QuickActionsLabel, 0);
            Grid.SetColumn(QuickButtonsGrid, 0); Grid.SetRow(QuickButtonsGrid, 1);

            QuickButtonsGrid.ColumnDefinitions.Clear();
            QuickButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            QuickButtonsGrid.RowDefinitions.Clear();
            QuickButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            QuickButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(NewTaskButton, 0); Grid.SetRow(NewTaskButton, 0);
            Grid.SetColumn(CreateInvoiceButton, 1); Grid.SetRow(CreateInvoiceButton, 0);
            Grid.SetColumn(ManageStockButton, 0); Grid.SetRow(ManageStockButton, 1);
            Grid.SetColumn(AnalyticsButton, 1); Grid.SetRow(AnalyticsButton, 1);

            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            MainContentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(LeftContentColumn, 0); Grid.SetRow(LeftContentColumn, 0);
            Grid.SetColumn(RightContentColumn, 0); Grid.SetRow(RightContentColumn, 1);

            ApplyFonts(18, 11, 10, 17, 13, 11, 14, 11, 12, 7, 10, 12, 14);
            ApplySizes(32, 32, 30, 19);
            ApplySpacing(8, 8, 10, 10);

            ConfigureTablesWidth(screenWidth);
        }

        private void ConfigureTablesWidth(double screenWidth)
        {
            double currentSidebarWidth = SidebarColumn.Width.IsAbsolute ? SidebarColumn.Width.Value : 0;
            double availableWidth = screenWidth - currentSidebarWidth - DashboardGrid.Padding.Left - DashboardGrid.Padding.Right;

            double leftColWidth = availableWidth;
            double rightColWidth = availableWidth;

            if (screenWidth >= 1200)
            {
                leftColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.6;
                rightColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.4;
            }
            else if (screenWidth >= 900)
            {
                leftColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.5;
                rightColWidth = (availableWidth - MainContentGrid.ColumnSpacing) * 0.5;
            }

            double finalOrdersWidth = Math.Max(leftColWidth, 600);
            RecentOrdersTableLayout.WidthRequest = finalOrdersWidth;
            RecentOrdersHeaderGrid.WidthRequest = finalOrdersWidth;

            double finalLowStockWidth = Math.Max(leftColWidth, 380);
            LowStockTableLayout.WidthRequest = finalLowStockWidth;

            double finalTechWidth = Math.Max(rightColWidth, 350);
            TechnicianTableLayout.WidthRequest = finalTechWidth;
        }

        private void ResetStatisticsPositions()
        {
            Grid.SetColumn(CustomersCard, 0); Grid.SetRow(CustomersCard, 0);
            Grid.SetColumn(RevenueCard, 1); Grid.SetRow(RevenueCard, 0);
            Grid.SetColumn(PendingCard, 2); Grid.SetRow(PendingCard, 0);
            Grid.SetColumn(LowStockCard, 3); Grid.SetRow(LowStockCard, 0);
        }

        private void ResetQuickButtonPositions()
        {
            Grid.SetColumn(NewTaskButton, 0); Grid.SetRow(NewTaskButton, 0);
            Grid.SetColumn(CreateInvoiceButton, 1); Grid.SetRow(CreateInvoiceButton, 0);
            Grid.SetColumn(ManageStockButton, 2); Grid.SetRow(ManageStockButton, 0);
            Grid.SetColumn(AnalyticsButton, 3); Grid.SetRow(AnalyticsButton, 0);
        }

        private void ApplyFonts(double dashboardTitle, double username, double statisticTitle, double statisticValue, double quickActionsTitle, double buttonFont, double sectionTitle, double rowText, double rowValue, double smallText, double iconFont, double chartTitle, double statusFont)
        {
            DashboardTitleLabel.FontSize = dashboardTitle;
            UsernameLabel.FontSize = username;
            CustomersTitleLabel.FontSize = statisticTitle;
            RevenueTitleLabel.FontSize = statisticTitle;
            PendingTitleLabel.FontSize = statisticTitle;
            LowStockTitleLabel.FontSize = statisticTitle;
            CustomersValueLabel.FontSize = statisticValue;
            RevenueValueLabel.FontSize = statisticValue;
            PendingValueLabel.FontSize = statisticValue;
            LowStockValueLabel.FontSize = statisticValue;
            QuickActionsLabel.FontSize = quickActionsTitle;
            NewTaskButton.FontSize = buttonFont;
            CreateInvoiceButton.FontSize = buttonFont;
            ManageStockButton.FontSize = buttonFont;
            AnalyticsButton.FontSize = buttonFont;
            RecentOrdersTitleLabel.FontSize = sectionTitle;
            LowStockAlertsTitleLabel.FontSize = sectionTitle;
            TechnicianWorkloadTitleLabel.FontSize = sectionTitle;
            TaskDistributionLabel.FontSize = chartTitle;
            CustomersIconLabel.FontSize = iconFont;
            RevenueIconLabel.FontSize = iconFont;
            PendingIconLabel.FontSize = iconFont;
            LowStockIconLabel.FontSize = iconFont;

            Resources["TableHeaderFontSize"] = rowText;
            Resources["TableRowFontSize"] = rowText;
        }

        private void ApplySizes(double statisticIconSize, double statisticIconHeight, double avatarSize, double burgerSize)
        {
            CustomersIconBorder.WidthRequest = statisticIconSize;
            CustomersIconBorder.HeightRequest = statisticIconHeight;
            RevenueIconBorder.WidthRequest = statisticIconSize;
            RevenueIconBorder.HeightRequest = statisticIconHeight;
            PendingIconBorder.WidthRequest = statisticIconSize;
            PendingIconBorder.HeightRequest = statisticIconHeight;
            LowStockIconBorder.WidthRequest = statisticIconSize;
            LowStockIconBorder.HeightRequest = statisticIconHeight;
            UserAvatarBorder.WidthRequest = avatarSize;
            UserAvatarBorder.HeightRequest = avatarSize;
            MenuBurgerPath.WidthRequest = burgerSize;
            MenuBurgerPath.HeightRequest = burgerSize;
        }

        private void ApplySpacing(double cardPadding, double cardSpacing, double contentSpacing, double quickPadding)
        {
            CustomersCard.Padding = cardPadding;
            RevenueCard.Padding = cardPadding;
            PendingCard.Padding = cardPadding;
            LowStockCard.Padding = cardPadding;
            CustomersLayout.Spacing = cardSpacing;
            RevenueLayout.Spacing = cardSpacing;
            PendingLayout.Spacing = cardSpacing;
            LowStockLayout.Spacing = cardSpacing;
            QuickActionsBorder.Padding = quickPadding;
            QuickButtonsGrid.ColumnSpacing = cardSpacing;
            QuickButtonsGrid.RowSpacing = 8;
            LeftContentColumn.Spacing = contentSpacing;
            RightContentColumn.Spacing = contentSpacing;
            MainContentGrid.ColumnSpacing = contentSpacing;
            ChartBorder.Padding = cardPadding;
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

    public class BarChartDrawable : IDrawable
    {
        public DashboardViewModel? ViewModel { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (ViewModel == null || !ViewModel.ChartData.Any()) return;
            var items = ViewModel.ChartData.ToList();
            int count = items.Count;
            if (count == 0) return;

            float marginX = 20, marginYTop = 30, marginYBottom = 45;
            float chartWidth = dirtyRect.Width - (marginX * 2);
            float chartHeight = dirtyRect.Height - marginYTop - marginYBottom;
            if (chartWidth <= 0 || chartHeight <= 0) return;

            float barSpacing = dirtyRect.Width < 400 ? 8 : (dirtyRect.Width < 700 ? 12 : 20);
            float barWidth = (chartWidth - (barSpacing * (count - 1))) / count;
            if (barWidth < 20) barWidth = 20;

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
                canvas.FillRoundedRectangle(x, y, barWidth, barHeight, 6);

                canvas.FontColor = Color.FromArgb("#212529");
                canvas.FontSize = dirtyRect.Width < 500 ? 10 : 14;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(item.DisplayValue.Split(' ')[0], x, y - 25, barWidth, 20, HorizontalAlignment.Center, VerticalAlignment.Center);

                canvas.FontColor = Color.FromArgb("#6C757D");
                canvas.FontSize = dirtyRect.Width < 500 ? 8 : 11;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(item.Label, x - 10, dirtyRect.Height - marginYBottom + 8, barWidth + 20, 40, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}