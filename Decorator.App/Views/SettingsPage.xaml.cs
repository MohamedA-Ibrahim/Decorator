using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.System;
using Decorator.DataAccess;
using Windows.Globalization;
using Microsoft.Graph.SecurityNamespace;

namespace Decorator.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            //if (ApplicationLanguages.PrimaryLanguageOverride == "Arabic")
            //{
            //    ArabicRadio.IsChecked = true;
            //}
            //else
            //{
            //    EnglishRadio.IsChecked = true;
            //}

        }

        private void OnLanguageChanged(object sender, RoutedEventArgs e)
        {
            //var radio = (RadioButton)sender;
            //switch (radio.Tag)
            //{
            //    case "Arabic":
            //        ApplicationLanguages.PrimaryLanguageOverride = "ar";
            //        break;
            //    case "English":
            //        ApplicationLanguages.PrimaryLanguageOverride = "en";
            //        break;
            //    default: throw new NotImplementedException();
            //}

            ////Reset the resource context to avoid needing to re-launch the app
            ////Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
            //Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();

            // Be sure to clear the Frame stack so that cached Pages are removed, otherwise they will have the old language.
            //Frame.BackStack.Clear();

            //Refresh the page
            //Frame.Navigate(this.GetType());
        }
    }
}
