using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Decorator.DataAccess;
using Contoso.App.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;

namespace Contoso.App.Views
{
    public sealed partial class ProductDetailPage : Page
    {
        public ProductDetailPage()
        {
            InitializeComponent();
        }
        public ProductViewModel ViewModel { get; set; }

        /// <summary>
        /// Navigate to the previous page when the user cancels the creation of a new customer record.
        /// </summary>
        private void AddNewProductCanceled(object sender, EventArgs e) => Frame.GoBack();

        /// <summary>
        /// Displays the selected product data.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null)
            {
                ViewModel = new ProductViewModel
                {
                    IsNewProduct = true,
                    IsInEdit = true
                };
                VisualStateManager.GoToState(this, "NewProduct", false);
            }
            else
            {
                ViewModel = App.ViewModel.Products.Where(product => product.Model.Id == (int)e.Parameter).First();

            }

            ViewModel.AddNewProductCanceled += AddNewProductCanceled;
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Disconnects the AddNewCustomerCanceled event handler from the ViewModel 
        /// when the parent frame navigates to a different page.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.AddNewProductCanceled -= AddNewProductCanceled;
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void ProductSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControls.CollapsibleSearchBox searchBox)
            {
                searchBox.AutoSuggestBox.QuerySubmitted += ProductSearchBox_QuerySubmitted;
                searchBox.AutoSuggestBox.TextChanged += ProductSearchBox_TextChanged;
                searchBox.AutoSuggestBox.PlaceholderText = "Search products...";
            }
        }

        /// <summary>
        /// Queries the database for a customer result matching the search text entered.
        /// </summary>
        private async void ProductSearchBox_TextChanged(AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // If no search query is entered, refresh the complete list.
                if (string.IsNullOrEmpty(sender.Text))
                {
                    sender.ItemsSource = null;
                }
                else
                {
                    sender.ItemsSource = await App.Repository.Products.GetAsync(sender.Text);
                }
            }
        }

        /// <summary>
        /// Search by product name, code, then display results.
        /// </summary>
        private void ProductSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is Product product)
            {
                Frame.Navigate(typeof(ProductDetailPage), product.Id);
            }
        }

        /// <summary>
        /// Navigates to the order page for the customer.
        /// </summary>
        private void ViewOrderButton_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ((sender as FrameworkElement).DataContext as Order).Id,
                new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Adds a new order for the customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.Model.Id);
    }
}
