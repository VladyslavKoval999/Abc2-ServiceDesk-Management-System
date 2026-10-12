using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using ServiceDesk.Maui.ViewModels;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.Views
{
    public partial class AiChatPage : ContentPage
    {
        private double _lastWidth = -1;

        private AiChatViewModel? ViewModel => BindingContext as AiChatViewModel;

        public AiChatPage(AiChatViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            Shell.SetNavBarIsVisible(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            AppSidebar.SidebarStateChanged += OnSidebarStateChanged;
            SizeChanged += OnAiChatPageSizeChanged;

            if (ViewModel != null)
            {
                ViewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateResponsiveLayout(Width);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(200);
                RefreshChatLayout();
                await Task.Delay(100);
                ScrollToLastMessage();
            });
        }

        protected override void OnDisappearing()
        {
            if (ViewModel != null)
                ViewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;

            SizeChanged -= OnAiChatPageSizeChanged;
            AppSidebar.SidebarStateChanged -= OnSidebarStateChanged;

            base.OnDisappearing();
        }

        private void OnSidebarStateChanged(object? sender, EventArgs e)
        {
            if (AppSidebar.IsDesktopMode)
            {
                SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            }
            UpdateResponsiveLayout(Width);
            RefreshChatLayout();
        }

        private void OnAiChatPageSizeChanged(object? sender, EventArgs e)
        {
            if (Width <= 0 || Math.Abs(_lastWidth - Width) < 2)
                return;

            _lastWidth = Width;
            UpdateResponsiveLayout(Width);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(30);
                RefreshChatLayout();
            });
        }

        private void RefreshChatLayout()
        {
            ChatCollection?.InvalidateMeasure();
            ChatBorder?.InvalidateMeasure();
            MainPageGrid?.InvalidateMeasure();
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

            MainPageGrid.Padding = new Thickness(25);
            MainPageGrid.RowSpacing = 15;
            PageTitleLabel.FontSize = 26;
            ToolbarGrid.ColumnSpacing = 12;
        }

        private void ApplyLargeTabletLayout()
        {
            SidebarColumn.Width = AppSidebar.IsDesktopMenuOpen ? 260 : 0;
            Grid.SetColumnSpan(AppSidebar, 1);
            AppSidebar.SetDesktopMode();

            MainPageGrid.Padding = new Thickness(20);
            MainPageGrid.RowSpacing = 12;
            PageTitleLabel.FontSize = 23;
            ToolbarGrid.ColumnSpacing = 10;
        }

        private void ApplyTabletLayout()
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = new Thickness(15);
            MainPageGrid.RowSpacing = 10;
            PageTitleLabel.FontSize = 21;
            ToolbarGrid.ColumnSpacing = 8;
        }

        private void ApplyPhoneLayout()
        {
            SidebarColumn.Width = 0;
            Grid.SetColumnSpan(AppSidebar, 2);
            AppSidebar.SetMobileMode();

            MainPageGrid.Padding = new Thickness(10);
            MainPageGrid.RowSpacing = 8;
            PageTitleLabel.FontSize = 19;
            ToolbarGrid.ColumnSpacing = 6;
        }

        private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                RefreshChatLayout();
                await Task.Delay(50);
                ScrollToLastMessage();
            });
        }

        private void ScrollToLastMessage()
        {
            if (ChatCollection == null || ViewModel == null || ViewModel.Messages.Count == 0)
                return;

            try
            {
                ChatCollection.ScrollTo(ViewModel.Messages.Count - 1, position: ScrollToPosition.End, animate: true);
            }
            catch
            {

            }
        }

        private async void OnMenuToggled(object? sender, TappedEventArgs e)
        {
            try
            {
                await Task.WhenAll(
                    MenuBurgerPath?.RotateToAsync(MenuBurgerPath.Rotation + 180, 250, Easing.CubicInOut) ?? Task.CompletedTask,
                    MenuBurgerPath?.ScaleToAsync(0.8, 100).ContinueWith(t => MenuBurgerPath.ScaleToAsync(1.0, 100)) ?? Task.CompletedTask
                );
            }
            finally
            {
                AppSidebar.Toggle();
            }
        }
    }
}