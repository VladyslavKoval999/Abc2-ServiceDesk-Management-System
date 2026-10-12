using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.Controls
{
    public partial class SidebarControl : ContentView
    {
        public event EventHandler? SidebarStateChanged;

        public bool IsDesktopMode { get; private set; }
        public bool IsDesktopMenuOpen { get; private set; } = true;
        private bool _isPt = false;

        private readonly Dictionary<string, Border> _menuItems = new();

        public SidebarControl()
        {
            InitializeComponent();
            InitializeMenuMap();
        }

        private void InitializeMenuMap()
        {
            _menuItems.Add("DashboardPage", MenuDashboard);
            _menuItems.Add("MainPage", MenuMain);
            _menuItems.Add("CustomersPage", MenuCustomers);
            _menuItems.Add("InventoryPage", MenuInventory);
            _menuItems.Add("EquipmentPage", MenuEquipment);
            _menuItems.Add("QuotationsPage", MenuQuotations);
            _menuItems.Add("InvoicesPage", MenuInvoices);
            _menuItems.Add("ReportsRoute", MenuReports);
            _menuItems.Add("SuppliersPage", MenuSuppliers);
            _menuItems.Add("PurchasingPage", MenuPurchasing);
            _menuItems.Add("TechniciansPage", MenuTechnicians);
            _menuItems.Add("AiChatPage", MenuAiChat);
            _menuItems.Add("SettingsPage", MenuSettings);
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            Element parent = this.Parent;
            while (parent != null)
            {
                if (parent is Page page)
                {
                    string pageName = page.GetType().Name;

                    if (pageName == "ReportsPage")
                        pageName = "ReportsRoute";

                    HighlightMenuItem(pageName);
                    return;
                }
                parent = parent.Parent;
            }

            HighlightMenuItem("DashboardPage");
        }

        public void HighlightMenuItem(string route)
        {
            foreach (var item in _menuItems)
            {
                bool isActive = item.Key == route;
                var border = item.Value;

                border.BackgroundColor = isActive ? Color.FromArgb("#152840") : Colors.Transparent;

                if (border.Content is HorizontalStackLayout layout && layout.Children.Count >= 2)
                {
                    if (layout.Children[0] is Label iconLabel)
                    {
                        iconLabel.TextColor = isActive ? Color.FromArgb("#0DCAF0") : Color.FromArgb("#8BA1B8");
                    }
                    if (layout.Children[1] is Label textLabel)
                    {
                        textLabel.TextColor = isActive ? Color.FromArgb("#0DCAF0") : Colors.White;
                    }
                }
            }
        }

        public void SetDesktopMode()
        {
            IsDesktopMode = true;
            SidebarPanel.TranslationX = 0;
            MobileOverlay.IsVisible = false;
            this.InputTransparent = false;
            InnerBurgerBtn.IsVisible = true;

            this.IsVisible = IsDesktopMenuOpen;
        }

        public void SetMobileMode()
        {
            IsDesktopMode = false;
            SidebarPanel.TranslationX = -300;
            MobileOverlay.IsVisible = false;
            this.InputTransparent = true;
            InnerBurgerBtn.IsVisible = false;

            this.IsVisible = true;
        }

        public async void Toggle()
        {
            if (IsDesktopMode)
            {
                IsDesktopMenuOpen = !IsDesktopMenuOpen;
                this.IsVisible = IsDesktopMenuOpen;

                SidebarStateChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (SidebarPanel.TranslationX < 0)
                {
                    this.InputTransparent = false;
                    MobileOverlay.IsVisible = true;
                    _ = MobileOverlay.FadeTo(0.5, 250);
                    await SidebarPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
                }
                else
                {
                    await CloseMobileMenu();
                }
            }
        }

        private async void CloseSidebar(object? sender, EventArgs e)
        {
            if (IsDesktopMode) Toggle();
            else await CloseMobileMenu();
        }

        private async Task CloseMobileMenu()
        {
            _ = MobileOverlay.FadeTo(0, 250);
            await SidebarPanel.TranslateTo(-300, 0, 300, Easing.CubicIn);
            MobileOverlay.IsVisible = false;
            this.InputTransparent = true;
        }

        private async void NavigateTo(object? sender, TappedEventArgs e)
        {
            if (!IsDesktopMode)
            {
                await CloseMobileMenu();
            }

            string route = e.Parameter?.ToString() ?? "";

            HighlightMenuItem(route);

            await Shell.Current.GoToAsync($"///{route}");
        }

        private void OnLanguageToggledCustom(object? sender, TappedEventArgs e)
        {
            _isPt = !_isPt;
            if (_isPt)
            {
                BtnLangPt.BackgroundColor = Color.FromArgb("#0A58CA");
                LblLangPt.TextColor = Colors.White;
                BtnLangEn.BackgroundColor = Colors.Transparent;
                LblLangEn.TextColor = Color.FromArgb("#8BA1B8");
                App.ChangeLanguage("pt");
            }
            else
            {
                BtnLangEn.BackgroundColor = Color.FromArgb("#0A58CA");
                LblLangEn.TextColor = Colors.White;
                BtnLangPt.BackgroundColor = Colors.Transparent;
                LblLangPt.TextColor = Color.FromArgb("#8BA1B8");
                App.ChangeLanguage("en");
            }
        }
    }
}