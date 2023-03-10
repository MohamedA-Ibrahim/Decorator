using Decorator.App.Helpers;
using Decorator.App.Reporting;
using Decorator.App.ViewModels;
using Decorator.DataAccess;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QuestPDF.Fluent;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Decorator.App.Views
{
    public sealed partial class OrderDetailPage : Page, INotifyPropertyChanged
    {
        public OrderDetailPage() => InitializeComponent();

        private OrderViewModel _viewModel;

        /// <summary>
        /// We use this object to bind the UI to our data.
        /// </summary>
        public OrderViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Loads the specified order, a cached order, or creates a new order.
        /// </summary>
        /// <param name="e">Info about the event.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = (e.Parameter as OrderListToDetailParameter);

            if (e.Parameter == null)
            {
                ViewModel = new OrderViewModel(new Order())
                {
                    IsInEdit = true
                };
            }
            else
            {
                // Order is an existing order.
                var order = await App.Repository.Orders.GetAsync(parameter.SelectedOrderId);
                ViewModel = new OrderViewModel(order)
                {
                    IsInEdit = parameter.IsEdit
                };
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Reverts the page.
        /// </summary>
        private async void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveChangesDialog()
            {
                Title = $"Save changes to Invoice # {ViewModel.InvoiceNumber.ToString()}?",
                Content = $"Invoice # {ViewModel.InvoiceNumber.ToString()} " + 
                    "has unsaved changes that will be lost. Do you want to save your changes?"
            };
            saveDialog.XamlRoot = this.Content.XamlRoot;
            await saveDialog.ShowAsync();
            SaveChangesDialogResult result = saveDialog.Result;

            switch (result)
            {
                case SaveChangesDialogResult.Save:
                    await ViewModel.SaveOrderAsync();
                    ViewModel = await OrderViewModel.CreateFromId(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.DontSave:
                    ViewModel = await OrderViewModel.CreateFromId(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.Cancel:
                    break;
            }         
        }

        /// <summary>
        /// Saves the current order.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ViewModel.CustomerName)
                    || ViewModel.OrderDetails.Count == 0)
                {
                    InfoBarMessges.ShowErrorMessage(InfoBarControl);
                    return;
                }

                await ViewModel.SaveOrderAsync();
                InfoBarMessges.ShowSuccessMessage(InfoBarControl);
                Bindings.Update();
            }
            catch (OrderSavingException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unable to save",
                    Content = $"There was an error saving your order:\n{ex.Message}", 
                    PrimaryButtonText = "OK"                 
                };
                dialog.XamlRoot = App.Window.Content.XamlRoot;
                await dialog.ShowAsync();
            }
        }
        /// <summary>
        /// Queries for products.
        /// </summary>
        private void ProductSearchBox_TextChanged(AutoSuggestBox sender, 
            AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.UpdateProductSuggestions(sender.Text);
            }
        }

        /// <summary>
        /// Notifies the page that a new item was chosen.
        /// </summary>
        private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, 
            AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                var selectedProductDimension = args.SelectedItem as ProductDimension;
                ViewModel.NewOrderDetail.ProductDimension = selectedProductDimension;
                ViewModel.NewOrderDetail.Price = selectedProductDimension.Price;
            }
        }

        /// <summary>
        /// Adds the new line item to the list of line items.
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var existing = ViewModel.OrderDetails.FirstOrDefault(x=> x.ProductDimension.Id == ViewModel.NewOrderDetail.Model.ProductDimension.Id);
            if(existing == null)
            {
                ViewModel.OrderDetails.Add(ViewModel.NewOrderDetail.Model);
            }
            else
            {
                existing.Quantity = ViewModel.NewOrderDetail.Quantity;

                //Remove and add the value to trigger collectionChanged
                //instead of having to implement INotifyPropertyChanged for Quantity
                ViewModel.OrderDetails.Remove(existing);
                ViewModel.OrderDetails.Add(existing);
            }

            ClearCandidateProduct();
        }

        /// <summary>
        /// Clears the new line item without adding it to the list of line items.
        /// </summary>
        private void CancelProductButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCandidateProduct();
        }

        /// <summary>
        /// Cleears the new line item entry area.
        /// </summary>
        private void ClearCandidateProduct()
        {
            ProductSearchBox.Text = string.Empty;
            ViewModel.NewOrderDetail = new OrderDetailViewModel();
        }

        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        private void RemoveProduct_Click(object sender, RoutedEventArgs e) =>
            ViewModel.OrderDetails.Remove((sender as FrameworkElement).DataContext as OrderDetail);

        /// <summary>
        /// Fired when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed. 
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
