using Decorator.App.Helpers;
using Decorator.App.ViewModels;
using Decorator.DataAccess.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Decorator.App.Views.Custom
{
    public sealed partial class CustomOrderDetailPage : Page, INotifyPropertyChanged
    {
        public CustomOrderDetailPage() => InitializeComponent();

        private CustomOrderViewModel _viewModel;

        /// <summary>
        /// We use this object to bind the UI to our data.
        /// </summary>
        public CustomOrderViewModel ViewModel
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
                ViewModel = new CustomOrderViewModel(new CustomOrder())
                {
                    IsInEdit = true
                };
            }
            else
            {
                // Order is an existing order.
                var order = await App.Repository.CustomOrders.GetAsync(parameter.SelectedOrderId);
                ViewModel = new CustomOrderViewModel(order)
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
                    ViewModel = await CustomOrderViewModel.CreateFromId(ViewModel.Id);
                    break;
                case SaveChangesDialogResult.DontSave:
                    ViewModel = await CustomOrderViewModel.CreateFromId(ViewModel.Id);
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
                    || ViewModel.OrderItems.Count == 0)
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
        /// Adds the new line item to the list of line items.
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {       
            if(string.IsNullOrWhiteSpace(ViewModel.NewCustomOrderItem.Name) 
                || ViewModel.NewCustomOrderItem.Quantity == 0
                || ViewModel.NewCustomOrderItem.Price == 0)
                return;

            ViewModel.OrderItems.Add(ViewModel.NewCustomOrderItem.Model);
           
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
            ProductName.Text = string.Empty;
            ViewModel.NewCustomOrderItem = new CustomOrderItemViewModel();
        }

        /// <summary>
        /// Removes a line item from the order.
        /// </summary>
        private void RemoveProduct_Click(object sender, RoutedEventArgs e) =>
            ViewModel.OrderItems.Remove((sender as FrameworkElement).DataContext as CustomOrderItem);

        /// <summary>
        /// Fired when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed. 
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var orderItem = (sender as FrameworkElement).DataContext as CustomOrderItem;

            ViewModel.NewCustomOrderItem = new CustomOrderItemViewModel(orderItem);

            ViewModel.OrderItems.Remove(orderItem);
        }
    }
}
