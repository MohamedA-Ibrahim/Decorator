using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Decorator.DataAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Networking;

namespace Contoso.App.ViewModels
{
    public partial class ProductViewModel : ObservableObject, IEditableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes a new instance of the CustomerViewModel class that wraps a Customer object.
        /// </summary>
        public ProductViewModel(Product model = null)
        {
            Model = model ?? new Product();
        }

        [ObservableProperty]
        private Product _model;

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if(_isNewProduct && string.IsNullOrEmpty(value))
                    _name = "New customer";
                else
                    _name = value;
            }
        }

      [ObservableProperty]
      private string _code;


        public ObservableCollection<ProductDimension> ProductDimensions { get; } = new ObservableCollection<ProductDimension>();

        [ObservableProperty]
        private ProductDimension _selectedProductDimension;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private bool _isInEdit = false;

        /// <summary>
        /// Saves customer data that has been edited.
        /// </summary>
        public async Task SaveAsync()
        {
            IsInEdit = false;
            if (_isNewProduct)
            {
                _isNewProduct = false;
                App.ViewModel.Products.Add(this);
            }

            await App.Repository.Products.UpsertAsync(Model);
        }

        public event EventHandler AddNewProductCanceled;

        public async Task CancelEditsAsync()
        {
            if (_isNewProduct)
            {
                AddNewProductCanceled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await RevertChangesAsync();
            }
        }

        /// <summary>
        /// Discards any edits that have been made, restoring the original values.
        /// </summary>
        public async Task RevertChangesAsync()
        {
            IsInEdit = false;
            await RefreshProductsAsync();
        }

        /// <summary>
        /// Enables edit mode.
        /// </summary>
        public void StartEdit() => IsInEdit = true;

        /// <summary>
        /// Reloads all of the customer data.
        /// </summary>
        public async Task RefreshProductsAsync()
        {
            Model = await App.Repository.Products.GetAsync(Model.Id);
        }

        public void BeginEdit()
        {
            // Not used.
        }

        /// <summary>
        /// Called when a bound DataGrid control cancels the edits that have been made to a customer.
        /// </summary>
        public async void CancelEdit() => await CancelEditsAsync();

        /// <summary>
        /// Called when a bound DataGrid control commits the edits that have been made to a customer.
        /// </summary>
        public async void EndEdit() => await SaveAsync();
    }
}