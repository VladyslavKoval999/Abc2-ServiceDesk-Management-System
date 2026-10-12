using Microsoft.Maui.Controls;
using ServiceDesk.Maui.ViewModels;
using System;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;
        private double _lastWidth = -1;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = _viewModel = viewModel;

            AppSidebar.SidebarStateChanged += (s, e) =>
            {
                if (AppSidebar.IsDesktopMode)
                {
                    SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
                }
                UpdateResponsiveLayout(Width);
            };

            SizeChanged += OnSettingsPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateResponsiveLayout(Width);
        }

        private void OnSettingsPageSizeChanged(object? sender, EventArgs e)
        {
            if (Width <= 0 || Math.Abs(_lastWidth - Width) < 3) return;
            _lastWidth = Width;
            UpdateResponsiveLayout(Width);
        }

        private void UpdateResponsiveLayout(double width)
        {
            if (width <= 0) return;
            if (width >= 1200) ApplyDesktopLayout();
            else if (width >= 900) ApplyLargeTabletLayout();
            else if (width >= 650) ApplyTabletLayout();
            else ApplyPhoneLayout();
        }

        private void ApplyDesktopLayout()
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();
            MainPageGrid.Padding = 25;
            PageTitleLabel.FontSize = 28;
        }

        private void ApplyLargeTabletLayout()
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();
            MainPageGrid.Padding = 20;
            PageTitleLabel.FontSize = 24;
        }

        private void ApplyTabletLayout()
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();
            MainPageGrid.Padding = 15;
            PageTitleLabel.FontSize = 21;
        }

        private void ApplyPhoneLayout()
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();
            MainPageGrid.Padding = 10;
            PageTitleLabel.FontSize = 18;
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