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
using Decorator.App.ViewModels;
using Decorator.DataAccess;
using System.Diagnostics;
using Decorator.App.Reporting;
using QuestPDF.Fluent;

namespace Decorator.App.Views
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
                ProductSearchBox.AutoSuggestBox.PlaceholderText = "البحث عن منتج ...";
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
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            // If no search query is entered, refresh the complete list.
            if (string.IsNullOrEmpty(sender.Text))
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
                        product.ProductName.StartsWith(parameter)))
                    .Select(product => $"{product.Code} - {product.ProductName}");
            }
        }

        /// Filters or resets the customer list based on the search text.
        /// </summary>
        private async void ProductSearchBox_QuerySubmitted(AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.QueryText))
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

            var matches = ViewModel.Products.Where(product => 
                    product.ProductName.StartsWith(text) ||
                    product.Code.StartsWith(text))
                .OrderByDescending(product => parameters.Count(parameter =>
                    product.ProductName.StartsWith(parameter) ||
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

        private async void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintProductReportDialog()
            {
                Title = $"طباعة جرد مبيعات {ViewModel.SelectedProduct.Model.Name}"
            };

            printDialog.XamlRoot = this.Content.XamlRoot;
            await printDialog.ShowAsync();

            var result = printDialog.Result;

            switch (result.ButtonClicked)
            {
                case ButtonClicked.Print:
                    await PrintReport(ViewModel.SelectedProduct.Model.Id, result.DateFrom, result.DateTo);
                    break;
                case ButtonClicked.Cancel:
                    break;
            }
        }

        private async Task PrintReport(int productId, DateTime dateFrom, DateTime dateTo)
        {
            var productSales = await App.Repository.Products.GetProductOrdersAsync(productId, dateFrom, dateTo);
            ReportGenerator.GenerateProductReport(productSales.ToList());
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
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Products.Sort);

    }
}
