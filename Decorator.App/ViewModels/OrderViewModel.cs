using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Decorator.DataAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Decorator.App.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public OrderViewModel(Order model)
        {
            Model = model;

            // Create an ObservableCollection to wrap Order.OrderDetailss so we can track
            // product additions and deletions.
            OrderDetails = new ObservableCollection<OrderDetail>(Model.OrderDetails);
            OrderDetails.CollectionChanged += OrderDetails_Changed;

            NewOrderDetail = new OrderDetailViewModel();
        }

        /// <summary>
        /// Creates an OrderViewModel that wraps an Order object created from the specified ID.
        /// </summary>
        public async static Task<OrderViewModel> CreateFromId(int orderId) => new OrderViewModel(await GetOrder(orderId));

        public Order Model { get; }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        private static async Task<Order> GetOrder(int orderId) => await App.Repository.Orders.GetAsync(orderId); 

        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert => Model != null && IsModified && IsExistingOrder;

        private bool _isInEdit = false;

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

        private bool _IsModified = false;

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying model has been modified. 
        /// </summary>
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

        /// <summary>
        /// Gets a value that indicates whether there is a backing order.
        /// </summary>
        public bool IsLoaded => Model != null && (IsNewOrder || Model.CustomerName != null);

        /// <summary>
        /// Gets a value that indicates whether there is not a backing order.
        /// </summary>
        public bool IsNotLoaded => !IsLoaded;

        /// <summary>
        /// Gets a value that indicates whether this is a newly-created order.
        /// </summary>
        public bool IsNewOrder => Model.InvoiceNumber == 0;

        /// <summary>
        /// Gets or sets the invoice number for this order. 
        /// </summary>
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

        private ObservableCollection<OrderDetail> _orderDetails;
        
        /// <summary>
        /// Gets the line items in this invoice. 
        /// </summary>
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

        /// <summary>
        /// Notifies anyone listening to this object that a line item changed. 
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


        private OrderDetailViewModel _newOrderDetail;

        /// <summary>
        /// Gets or sets the line item that the user is currently working on.
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

        private void NewOrderDetail_PropertyChanged(object sender, PropertyChangedEventArgs e) => UpdateNewOrderDetailBindings();

        private void UpdateNewOrderDetailBindings()
        {
            OnPropertyChanged(nameof(NewOrderDetail));
            OnPropertyChanged(nameof(HasNewOrderDetail));
            OnPropertyChanged(nameof(NewOrderDetailProductListPriceFormatted));
        }

        /// <summary>
        /// Gets or sets whether there is a new order detail in progress.
        /// </summary>
        public bool HasNewOrderDetail => NewOrderDetail != null && NewOrderDetail.ProductDimension != null;

        /// <summary>
        /// Gets the product list price of the new line item, formatted for display.
        /// </summary>
        public string NewOrderDetailProductListPriceFormatted => (NewOrderDetail?.ProductDimension?.Price ?? 0).ToString("0.00");

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

        /// <summary>
        /// Stores the product suggestions. 
        /// </summary>
        public ObservableCollection<ProductDimension> ProductSuggestions { get; } = new ();

        /// <summary>
        /// Queries the database and updates the list of new product suggestions. 
        /// </summary>
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
    }
}
