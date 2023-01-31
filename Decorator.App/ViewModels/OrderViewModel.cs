using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Decorator.App.Reporting;
using Decorator.DataAccess;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.App.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        #region Constructor

        public OrderViewModel(Order model)
        {
            Model = model;

            // Create an ObservableCollection to wrap Order.OrderDetailss so we can track
            // product additions and deletions.
            OrderDetails = new ObservableCollection<OrderDetail>(Model.OrderDetails);
            OrderDetails.CollectionChanged += OrderDetails_Changed;

            NewOrderDetail = new OrderDetailViewModel();
        }

        #endregion

        #region Private Binding Fields

        private OrderDetailViewModel _newOrderDetail;
        private ObservableCollection<OrderDetail> _orderDetails;
        private bool _isInEdit = false;
        private bool _IsModified = false;

        #endregion

        #region Public Binding Fields

        public ObservableCollection<OrderDetail> OrderDetails
        {
            get => _orderDetails;
            set
            {
                if (_orderDetails != value)
                {
                    if (value != null)
                    {
                        value.CollectionChanged += OrderDetails_Changed;
                    }

                    if (_orderDetails != null)
                    {
                        _orderDetails.CollectionChanged -= OrderDetails_Changed;
                    }
                    _orderDetails = value;
                    OnPropertyChanged();
                    IsModified = true;

                }
            }
        }
        public ObservableCollection<ProductDimension> ProductSuggestions { get; } = new();
        public Order Model { get; }

        public int Id
        {
            get => Model.Id;
            set
            {
                if (Model.Id != value)
                {
                    Model.Id = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public DateTime PurchaseDate
        {
            get => Model.PurchaseDate;
            set
            {
                if (Model.PurchaseDate != value)
                {
                    Model.PurchaseDate = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }
        public string CustomerAddress
        {
            get => Model.CustomerAddress;
            set
            {
                if (Model.CustomerAddress != value)
                {
                    Model.CustomerAddress = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }
        public string CustomerName
        {
            get => Model.CustomerName;
            set
            {
                if (Model.CustomerName != value)
                {
                    Model.CustomerName = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public string CustomerPhone
        {
            get => Model.CustomerPhone;
            set
            {
                if (Model.CustomerPhone != value)
                {
                    Model.CustomerPhone = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }
        public float SubTotal => Model.SubTotal;

        public float Discount
        {
            get => Model.Discount;
            set
            {
                if (Model.Discount != value)
                {
                    Model.Discount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GrandTotal));
                    IsModified = true;
                }
            }
        }

        public float GrandTotal => Model.GrandTotal;
        public bool CanRevert => Model != null && IsModified && IsExistingOrder;
        public bool IsInEdit
        {
            get => _isInEdit;
            set
            {
                if (value != _isInEdit)
                {              
                        _isInEdit = value;
                        OnPropertyChanged();
                }
            }
        }
        public bool IsModified
        {
            get => _IsModified;
            set
            {
                if (value != _IsModified)
                {
                    // Only record changes after the order has loaded. 
                    if (IsLoaded)
                    {
                        if(IsInEdit)
                        {
                            _IsModified = value;

                        }
                        else
                        {
                            _IsModified = false;
                        }

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(CanRevert));
                    }
                }
            }
        }
        public bool IsExistingOrder => !IsNewOrder;
        public bool IsLoaded => Model != null && (IsNewOrder || Model.CustomerName != null);
        public bool IsNotLoaded => !IsLoaded;
        public bool IsNewOrder => Model.InvoiceNumber == 0;
        public bool HasNewOrderDetail => NewOrderDetail != null && NewOrderDetail.ProductDimension != null;

        /// <summary>
        /// Gets the product list price of the new order detail, formatted for display.
        /// </summary>
        public string NewOrderDetailProductListPriceFormatted => (NewOrderDetail?.ProductDimension?.Price ?? 0).ToString("0.00");

        public int InvoiceNumber
        {
            get => Model.InvoiceNumber;
            set
            {
                if (Model.InvoiceNumber != value)
                {
                    Model.InvoiceNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNewOrder));
                    OnPropertyChanged(nameof(IsLoaded));
                    OnPropertyChanged(nameof(IsNotLoaded));
                    OnPropertyChanged(nameof(IsNewOrder));
                    OnPropertyChanged(nameof(IsExistingOrder));
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the order detail that the user is currently working on.
        /// </summary>
        public OrderDetailViewModel NewOrderDetail
        {
            get => _newOrderDetail;
            set
            {
                if (value != _newOrderDetail)
                {
                    if (value != null)
                    {
                        value.PropertyChanged += NewOrderDetail_PropertyChanged;
                    }

                    if (_newOrderDetail != null)
                    {
                        _newOrderDetail.PropertyChanged -= NewOrderDetail_PropertyChanged;
                    }

                    _newOrderDetail = value;
                    UpdateNewOrderDetailBindings();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an OrderViewModel that wraps an Order object created from the specified ID.
        /// </summary>
        public async static Task<OrderViewModel> CreateFromId(int orderId) => new OrderViewModel(await GetOrder(orderId));

        public async Task SaveOrderAsync()
        {

            Order result = null;
            try
            {
                result = await App.Repository.Orders.UpsertAsync(Model);
            }
            catch (Exception ex)
            {
                throw new OrderSavingException("Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again.", ex);
            }
            if (result != null)
            {
                await dispatcherQueue.EnqueueAsync(() =>
                {
                    IsInEdit = false;
                    IsModified = false;
                });
            }
            else
            {
                await dispatcherQueue.EnqueueAsync(() => new OrderSavingException(
                    "Unable to save. There might have been a problem " +
                    "connecting to the database. Please try again."));
            }
        }

        public async void UpdateProductSuggestions(string queryText)
        {
            ProductSuggestions.Clear();

            if (!string.IsNullOrEmpty(queryText))
            {
                var suggestions = await App.Repository.Products.GetWithDimensionsAsync(queryText);

                foreach (ProductDimension p in suggestions)
                {
                    ProductSuggestions.Add(p);
                }
            }
        }

        #endregion

        #region Reporting

        public void PrintOrderInvoice()
        {
            ReportGenerator.GenerateOrderReport(Model);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Notifies anyone listening to this object that an order detail changed. 
        /// </summary>
        private void OrderDetails_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (OrderDetails != null)
            {
                Model.OrderDetails = OrderDetails.ToList();
            }

            OnPropertyChanged(nameof(OrderDetails));

            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(GrandTotal));
            IsModified = true;
        }
        private void NewOrderDetail_PropertyChanged(object sender, PropertyChangedEventArgs e) => UpdateNewOrderDetailBindings();
        private void UpdateNewOrderDetailBindings()
        {
            OnPropertyChanged(nameof(NewOrderDetail));
            OnPropertyChanged(nameof(HasNewOrderDetail));
            OnPropertyChanged(nameof(NewOrderDetailProductListPriceFormatted));
        }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        private static async Task<Order> GetOrder(int orderId) => await App.Repository.Orders.GetAsync(orderId);

        #endregion
    }
}
