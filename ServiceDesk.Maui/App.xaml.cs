using Microsoft.Maui.Controls;
using ServiceDesk.Maui.Views;
using System.Linq;

namespace ServiceDesk.Maui
{
    public partial class App : Application
    {
        private readonly LoginPage _loginPage;

        public App(LoginPage loginPage)
        {
            InitializeComponent();
            _loginPage = loginPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_loginPage);
        }

        public static void ChangeLanguage(string languageCode)
        {
            if (Current == null) return;

            var dictionaries = Current.Resources.MergedDictionaries;

            var oldLangDict = dictionaries.FirstOrDefault(d => d.GetType().Namespace == "ServiceDesk.Maui.Localization");
            if (oldLangDict != null)
            {
                dictionaries.Remove(oldLangDict);
            }

            if (languageCode == "pt")
            {
                dictionaries.Add(new Localization.StringsPt());
            }
            else
            {
                dictionaries.Add(new Localization.StringsEn());
            }
        }
    }
}