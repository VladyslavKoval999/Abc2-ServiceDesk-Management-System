using Microsoft.Maui.Controls;
using ServiceDesk.Maui.ViewModels;
using System;

namespace ServiceDesk.Maui.Views
{
    public partial class LoginPage : ContentPage
    {
        private double _lastWidth = -1;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            SizeChanged += OnLoginPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Shell.SetNavBarIsVisible(this, false);
            if (Shell.Current != null)
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            UpdateResponsiveLayout(Width);
        }

        private void OnLoginPageSizeChanged(object? sender, EventArgs e)
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
            ApplyStyles(400, 70, 32, 28, 15, 14, 15, 17, 36, 54);
        }

        private void ApplyLargeTabletLayout()
        {
            ApplyStyles(380, 65, 30, 26, 14, 14, 14, 16, 34, 52);
        }

        private void ApplyTabletLayout()
        {
            ApplyStyles(350, 60, 28, 24, 14, 13, 14, 15, 30, 50);
        }

        private void ApplyPhoneLayout()
        {
            ApplyStyles(320, 55, 24, 22, 13, 12, 13, 15, 26, 48);
        }

        private void ApplyStyles(double containerWidth, double logoBoxHeight, double logoSize, double welcomeSize, double subtitleSize, double labelSize, double fieldSize, double buttonSize, double loadingSize, double fieldHeight)
        {
            Resources["LoginContainerWidth"] = containerWidth;
            Resources["LogoBoxHeight"] = logoBoxHeight;
            Resources["LoginLogoFontSize"] = logoSize;
            Resources["LoginWelcomeFontSize"] = welcomeSize;
            Resources["LoginSubtitleFontSize"] = subtitleSize;
            Resources["LoginLabelFontSize"] = labelSize;
            Resources["LoginFieldFontSize"] = fieldSize;
            Resources["LoginButtonFontSize"] = buttonSize;
            Resources["LoadingFontSize"] = loadingSize;
            Resources["LoginFieldHeight"] = fieldHeight;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

#if WINDOWS
            var window = this.Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

                var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                
                if (presenter != null && presenter.State != Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                {
                    presenter.Maximize();
                }
            }
#endif
        }
    }
}