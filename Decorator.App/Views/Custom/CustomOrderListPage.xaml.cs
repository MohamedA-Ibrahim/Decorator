using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.WinUI.UI.Controls;
using Decorator.DataAccess;
using Decorator.App.ViewModels;
using System.Linq;
using Decorator.App.Reporting;
using Decorator.DataAccess.Models.DatabaseModels;

namespace Decorator.App.Views.Custom
{
    /// <summary>
    /// Displays the list of orders.
    /// </summary>
    public sealed partial class CustomOrderListPage : Page
    {
        /// <summary>
        /// We use this object to bind the UI to our data. 
        /// </summary>
        public CustomOrderListViewModel ViewModel { get; } = new CustomOrderListViewModel();

        /// <summary>
        /// Creates a new OrderListPage.
        /// </summary>
        public CustomOrderListPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieve the list of orders when the user navigates to the page. 
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel.Orders.Count < 1)
            {
                ViewModel.LoadOrders();
            }
        }

        /// <summary>
        /// Opens the order in the order details page for editing. 
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e) => 
            Frame.Navigate(typeof(CustomOrderDetailPage), new OrderListToDetailParameter(ViewModel.SelectedOrder.Id, true));

        /// <summary>
        /// Deletes the currently selected order.
        /// </summary>
        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deleteDialog = new ContentDialog
                {
                    Title = "هل أنت متأكد من الحذف؟",
                    Content = "بعد حذف هذه الفاتورة سيتم ارجاع كمية الاصناف الى قيمتها الأصلية",
                    PrimaryButtonText = "حذف",
                    CloseButtonText = "الغاء",
                    DefaultButton = ContentDialogButton.Close,
                    FlowDirection = FlowDirection.RightToLeft,
                    XamlRoot = App.Window.Content.XamlRoot
                };

                ContentDialogResult result = await deleteDialog.ShowAsync();

                if (result != ContentDialogResult.Primary)
                {
                    return;
                }

                var orderToDelete = ViewModel.SelectedOrder;
                await ViewModel.DeleteOrder(orderToDelete);
                
                ViewModel.LoadOrders();
            }
            catch(OrderDeletionException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "تعذر الحذف",
                    Content = $"حدثت مشكلة أثناء حذف " + 
                        $"فاتورة #{ViewModel.SelectedOrder.InvoiceNumber}:\n{ex.Message}",
                    PrimaryButtonText = "OK"
                };
                dialog.XamlRoot = App.Window.Content.XamlRoot;
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Initializes the AutoSuggestBox portion of the search box.
        /// </summary>
        private void OrderSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControls.CollapsibleSearchBox searchBox)
            {
                searchBox.AutoSuggestBox.QuerySubmitted += OrderSearch_QuerySubmitted;
                searchBox.AutoSuggestBox.TextChanged += OrderSearch_TextChanged;
                searchBox.AutoSuggestBox.PlaceholderText = "البحث عن طلبية...";
                searchBox.AutoSuggestBox.ItemsSource = ViewModel.OrderSuggestions;
            }
        }

        /// <summary>
        /// Searches the list of orders.
        /// </summary>
        private void OrderSearch_QuerySubmitted(AutoSuggestBox sender, 
            AutoSuggestBoxQuerySubmittedEventArgs args) =>
                ViewModel.SearchOrders(args.QueryText);

        /// <summary>
        /// Updates the suggestions for the AutoSuggestBox as the user types. 
        /// </summary>
        private void OrderSearch_TextChanged(AutoSuggestBox sender, 
            AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing, 
            // otherwise we assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            if (string.IsNullOrEmpty(sender.Text))
            {
                ViewModel.LoadOrders();
                sender.ItemsSource = null;
            }
            else
            {

                string[] parameters = sender.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                sender.ItemsSource = ViewModel.Orders
                    .Where(order => parameters
                        .Any(parameter =>
                            order.CustomerName.Contains(parameter) ||
                            order.InvoiceNumber.ToString().StartsWith(parameter)))
                    .OrderByDescending(order => parameters
                        .Count(parameter =>
                            order.CustomerName.Contains(parameter) ||
                            order.InvoiceNumber.ToString().StartsWith(parameter)))
                    .Select(o => $"{o.InvoiceNumber} - {o.CustomerName}");
            }
        }

        /// <summary>
        /// Navigates to the order detail page when the user
        /// double-clicks an order. 
        /// </summary>
        private void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) =>
            Frame.Navigate(
                typeof(CustomOrderDetailPage),
                new OrderListToDetailParameter(ViewModel.SelectedOrder.Id, false));

        // Navigates to the details page for the selected customer when the user presses SPACE.
        private void DataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                Frame.Navigate(
                    typeof(CustomOrderDetailPage),
                    new OrderListToDetailParameter(ViewModel.SelectedOrder.Id, false));
            }
        }

        /// <summary>
        /// Selects the tapped order. 
        /// </summary>
        private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) =>
            ViewModel.SelectedOrder = (e.OriginalSource as FrameworkElement).DataContext as CustomOrder;

        /// <summary>
        /// Navigates to the order details page.
        /// </summary>
        private void MenuFlyoutViewDetails_Click(object sender, RoutedEventArgs e) =>
            Frame.Navigate(
                typeof(CustomOrderDetailPage),
                new OrderListToDetailParameter(ViewModel.SelectedOrder.Id, false),
                new DrillInNavigationTransitionInfo());

        /// <summary>
        /// Sorts the data in the DataGrid.
        /// </summary>
        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e) =>
            (sender as DataGrid).Sort(e.Column, ViewModel.Orders.Sort);

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CustomOrderDetailPage));
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.GenerateCustomOrderReport(ViewModel.SelectedOrder);
        }
    }
}
