using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Decorator.DataAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Networking;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;

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

            Model = model ?? new Product(){ ProductDimensions = new List<ProductDimension>()};

            ProductDimensions = new ObservableCollection<ProductDimension>(Model.ProductDimensions);
            ProductDimensions.CollectionChanged += ProductDimensions_Changed;
        }


        #region Observable Properties


        private ObservableCollection<ProductDimension> _productDimensions;

        public ObservableCollection<ProductDimension> ProductDimensions
        {
            get => _productDimensions;
            set
            {
                if (_productDimensions != value)
                {
                    if (value != null)
                    {
                        value.CollectionChanged += ProductDimensions_Changed;
                    }

                    if (_productDimensions != null)
                    {
                        _productDimensions.CollectionChanged -= ProductDimensions_Changed;
                    }
                    _productDimensions = value;
                    OnPropertyChanged();
                    IsModified = true;

                }
            }
        }


        private Product _model;

        public Product Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    // Raise the PropertyChanged event for all properties.
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        public string ProductName
        {
            get => Model.Name;
            set
            {
                if(value != Model.Name) 
                {
                    if(_isNewProduct && string.IsNullOrEmpty(value))
                        Model.Name = "New product";
                    else
                    {
                        Model.Name = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public string Code
        {
            get => Model.Code;
            set
            {
                if (value != Model.Code)
                {              
                        Model.Code = value;
                        OnPropertyChanged();
                    
                }
            }
        }


        [ObservableProperty]
        private ProductDimension _selectedProductDimension;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private bool _isInEdit = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying model has been modified. 
        /// </summary>
        public bool IsModified { get; set; }

        #endregion

        private void ProductDimensions_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ProductDimensions != null)
            {
                Model.ProductDimensions = ProductDimensions.ToList();
            }

            OnPropertyChanged(nameof(ProductDimensions));
            IsModified = true;

        }

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
            ProductDimensions = new ObservableCollection<ProductDimension>(Model.ProductDimensions);

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