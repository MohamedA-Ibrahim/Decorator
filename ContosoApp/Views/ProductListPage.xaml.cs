using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Controls;
using Contoso.App.ViewModels;
using Decorator.DataAccess;

namespace Contoso.App.Views
{
    public sealed partial class ProductListPage : Page
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes the page.
        /// </summary>
        public ProductListPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the app-wide ViewModel instance.
        /// </summary>
        public MainViewModel ViewModel => App.ViewModel;

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void ProductSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (ProductSearchBox != null)
            {
                ProductSearchBox.AutoSuggestBox.QuerySubmitted += ProductSearchBox_QuerySubmitted;
                ProductSearchBox.AutoSuggestBox.TextChanged += ProductSearchBox_TextChanged;
                ProductSearchBox.AutoSuggestBox.PlaceholderText = "Search products...";
            }
        }

        /// <summary>
        /// Updates the search box items source when the user changes the search text.
        /// </summary>
        private async void ProductSearchBox_TextChanged(AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // If no search query is entered, refresh the complete list.
                if (String.IsNullOrEmpty(sender.Text))
                {
                    await dispatcherQueue.EnqueueAsync(async () => await ViewModel.GetProductListAsync());
                    sender.ItemsSource = null;
                }
                else
                {
                    string[] parameters = sender.Text.Split(new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);
                    sender.ItemsSource = ViewModel.Products
                        .Where(product => parameters.Any(parameter =>
                            product.Code.StartsWith(parameter) ||
                            product.Name.StartsWith(parameter)))
                        .Select(product => $"{product.Code} - {product.Name}"); 
                }
            }
        }

        /// Filters or resets the customer list based on the search text.
        /// </summary>
        private async void ProductSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (String.IsNullOrEmpty(args.QueryText))
            {
                await ResetProductList();
            }
            else
            {
                await FilterProductList(args.QueryText);
            }
        }

        private async Task ResetProductList()
        {
            await dispatcherQueue.EnqueueAsync(async () => await ViewModel.GetProductListAsync());
        }
        private async Task FilterProductList(string text)
        {
            string[] parameters = text.Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            var matches = ViewModel.Products.Where(product => parameters
                .Any(parameter =>
                    product.Name.StartsWith(parameter) ||
                    product.Code.StartsWith(parameter)))
                .OrderByDescending(product => parameters.Count(parameter =>
                    product.Name.StartsWith(parameter) ||
                    product.Code.StartsWith(parameter)))
                .ToList();

            await dispatcherQueue.EnqueueAsync(() =>
            {
                ViewModel.Products.Clear();
                foreach (var match in matches)
                {
                    ViewModel.Products.Add(match);
                }
            });
        }

        /// <summary>
        /// Applies any existing filter when navigating to the page.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ProductSearchBox.AutoSuggestBox.Text))
            {
                await FilterProductList(ProductSearchBox.AutoSuggestBox.Text);
            }
        }

        /// <summary>
        /// Menu flyout click control for selecting a product and displaying details.
        /// </summary>
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProduct != null)
            {
                Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Model.Id,
                    new DrillInNavigationTransitionInfo());
            }
        }

        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(ProductDetailPage), ViewModel.SelectedProduct.Model.Id,
                    new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Navigates to a blank customer details page for the user to fill in.
        /// </summary>
        private void CreateProduct_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(ProductDetailPage), null, new DrillInNavigationTransitionInfo());



        /// <summary>
        /// Selects the tapped customer. 
        /// </summary>
        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) =>
            ViewModel.SelectedProduct = (e.OriginalSource as FrameworkElement).DataContext as ProductViewModel;

        /// <summary>
        /// Opens the order detail page for the user to create an order for the selected customer.
        /// </summary>
        private void AddOrder_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(typeof(OrderDetailPage), ViewModel.SelectedProduct.Model.Id);

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Products.Sort);

    }
}
