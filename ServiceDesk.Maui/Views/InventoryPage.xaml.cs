using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using ServiceDesk.Maui.ViewModels;
using ServiceDesk.Shared.Entities;
using System;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.Views
{
    public partial class InventoryPage : ContentPage
    {
        private readonly InventoryViewModel _viewModel;
        private double _lastWidth = -1;

        public InventoryPage(InventoryViewModel viewModel)
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            _viewModel = viewModel;
            BindingContext = _viewModel;

            AppSidebar.SidebarStateChanged += (s, e) =>
            {
                if (AppSidebar.IsDesktopMode)
                {
                    SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
                }
                UpdateResponsiveLayout(Width);
            };

            SizeChanged += OnInventoryPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateResponsiveLayout(Width);
        }

        private void OnRowTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is InventoryItem selectedItem)
            {
                InventoryCollection.SelectedItem = selectedItem;
                _viewModel.SelectedProduct = selectedItem;
            }
        }

        private void OnInventoryPageSizeChanged(object? sender, EventArgs e)
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
            PageTitleLabel.FontSize = 28;

            ToolbarGrid.ColumnSpacing = 15;
            ToolbarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) };
            ToolbarGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto) };

            Grid.SetColumn(MenuBorder, 0); Grid.SetRow(MenuBorder, 0);
            Grid.SetColumn(SearchBorder, 1); Grid.SetRow(SearchBorder, 0);
            Grid.SetColumnSpan(SearchBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 2); Grid.SetRow(ActionButtonsLayout, 0);
            Grid.SetColumnSpan(ActionButtonsLayout, 1);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End;

            SearchBorder.WidthRequest = 380;
            SearchBorder.HorizontalOptions = LayoutOptions.Start;
            SearchBorder.HeightRequest = 44;
            SearchEntry.FontSize = 14;

            SetButtonSize(14, 16, 8);
            ConfigureTable(13, 14, 900, 20, 15, screenWidth);
        }

        private void ApplyLargeTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();

            MainPageGrid.Padding = 20;
            MainPageGrid.RowSpacing = 16;
            PageTitleLabel.FontSize = 24;

            ToolbarGrid.ColumnSpacing = 10;
            ToolbarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) };
            ToolbarGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto) };

            Grid.SetColumn(MenuBorder, 0); Grid.SetRow(MenuBorder, 0);
            Grid.SetColumn(SearchBorder, 1); Grid.SetRow(SearchBorder, 0);
            Grid.SetColumnSpan(SearchBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 2); Grid.SetRow(ActionButtonsLayout, 0);
            Grid.SetColumnSpan(ActionButtonsLayout, 1);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End;

            SearchBorder.WidthRequest = 280;
            SearchBorder.HorizontalOptions = LayoutOptions.Start;
            SearchBorder.HeightRequest = 42;
            SearchEntry.FontSize = 13;

            SetButtonSize(13, 12, 7);
            ConfigureTable(12, 13, 850, 18, 12, screenWidth);
        }

        private void ApplyTabletLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = 15;
            MainPageGrid.RowSpacing = 14;
            PageTitleLabel.FontSize = 21;

            ToolbarGrid.ColumnSpacing = 8;
            ToolbarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) };
            ToolbarGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto) };

            Grid.SetColumn(MenuBorder, 0); Grid.SetRow(MenuBorder, 0);
            Grid.SetColumn(SearchBorder, 1); Grid.SetRow(SearchBorder, 0);
            Grid.SetColumnSpan(SearchBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 2); Grid.SetRow(ActionButtonsLayout, 0);
            Grid.SetColumnSpan(ActionButtonsLayout, 1);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.End;

            SearchBorder.WidthRequest = 220;
            SearchBorder.HorizontalOptions = LayoutOptions.Start;
            SearchBorder.HeightRequest = 40;
            SearchEntry.FontSize = 12;

            SetButtonSize(12, 10, 6);
            ConfigureTable(11, 12, 800, 15, 10, screenWidth);
        }

        private void ApplyPhoneLayout(double screenWidth)
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = 10;
            MainPageGrid.RowSpacing = 10;
            PageTitleLabel.FontSize = 18;

            ToolbarGrid.ColumnSpacing = 6;
            ToolbarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) };
            ToolbarGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) };

            Grid.SetColumn(MenuBorder, 0); Grid.SetRow(MenuBorder, 0);

            Grid.SetColumn(SearchBorder, 1); Grid.SetRow(SearchBorder, 0);
            Grid.SetColumnSpan(SearchBorder, 1);

            Grid.SetColumn(ActionButtonsLayout, 0); Grid.SetRow(ActionButtonsLayout, 1);
            Grid.SetColumnSpan(ActionButtonsLayout, 2);
            ActionButtonsLayout.HorizontalOptions = LayoutOptions.Fill;
            ActionButtonsLayout.JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start;

            SearchBorder.WidthRequest = -1;
            SearchBorder.HorizontalOptions = LayoutOptions.Fill;
            SearchBorder.HeightRequest = 38;
            SearchEntry.FontSize = 11;

            SetButtonSize(11, 10, 8);
            ConfigureTable(10, 11, 750, 10, 8, screenWidth);
        }

        private void ConfigureTable(double headerFontSize, double rowFontSize, double minimumWidth, double horizontalPadding, double verticalPadding, double screenWidth)
        {
            double currentSidebarWidth = SidebarColumn.Width.IsAbsolute ? SidebarColumn.Width.Value : 0;
            double availableWidth = screenWidth - currentSidebarWidth - MainPageGrid.Padding.Left - MainPageGrid.Padding.Right;
            double finalWidth = Math.Max(availableWidth, minimumWidth);

            InventoryBorder.HorizontalOptions = LayoutOptions.Fill;
            InventoryGrid.HorizontalOptions = LayoutOptions.Start;
            InventoryGrid.WidthRequest = finalWidth;
            TableHeaderGrid.WidthRequest = finalWidth;
            TableHeaderGrid.Padding = new Thickness(horizontalPadding, verticalPadding);
            InventoryCollection.WidthRequest = finalWidth;
            InventoryCollection.HorizontalOptions = LayoutOptions.Start;

            Resources["TableHeaderFontSize"] = headerFontSize;
            Resources["TableRowFontSize"] = rowFontSize;

            InventoryCollection.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
            InventoryBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) };
        }

        private void SetButtonSize(double fontSize, double horizontalPadding, double verticalPadding)
        {
            RefreshButton.FontSize = fontSize;
            DeleteButton.FontSize = fontSize;
            NewProductButton.FontSize = fontSize;

            RefreshButton.Padding = new Thickness(horizontalPadding, verticalPadding);
            DeleteButton.Padding = new Thickness(horizontalPadding, verticalPadding);
            NewProductButton.Padding = new Thickness(horizontalPadding + 3, verticalPadding);
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
}