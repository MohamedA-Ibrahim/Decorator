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

namespace Contoso.App.ViewModels
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
        public async static Task<OrderViewModel> CreateFromGuid(Guid orderId) => new OrderViewModel(await GetOrder(orderId));

        public Order Model { get; }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        private static async Task<Order> GetOrder(Guid orderId) => await App.Repository.Orders.GetAsync(orderId); 

        public bool CanRefresh => Model != null && IsExistingOrder;

        /// <summary>
        /// Gets a value that specifies whether the user can revert changes. 
        /// </summary>
        public bool CanRevert => Model != null && IsExistingOrder;

        [ObservableProperty]
        private Guid _id;

        public bool IsExistingOrder => !IsNewOrder;

        /// <summary>
        /// Gets a value that indicates whether there is a backing order.
        /// </summary>
        public bool IsLoaded => Model != null && (IsNewOrder);

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
        /// 

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNewOrder), nameof(IsLoaded), nameof(IsNotLoaded), nameof(IsExistingOrder))]
        private int _invoiceNumber;

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
            OnPropertyChanged(nameof(HasNewLineItem));
            OnPropertyChanged(nameof(NewOrderDetailProductListPriceFormatted));
        }

        /// <summary>
        /// Gets or sets whether there is a new order detail in progress.
        /// </summary>
        public bool HasNewLineItem => NewOrderDetail != null && NewOrderDetail.ProductDimension != null;

        /// <summary>
        /// Gets the product list price of the new line item, formatted for display.
        /// </summary>
        public string NewOrderDetailProductListPriceFormatted => (NewOrderDetail?.ProductDimension?.Price ?? 0).ToString("c");

        [ObservableProperty]
        private DateTime _purchaseDate;


        /// <summary>
        /// Gets the subtotal. This value is calculated automatically. 
        /// </summary>
        public decimal Subtotal => Model.Subtotal;

        [ObservableProperty]
        private string _customerName;

        [ObservableProperty]
        private string _customerAddress;

        [ObservableProperty]
        private string _customerPhone;


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
        }

        /// <summary>
        /// Stores the product suggestions. 
        /// </summary>
        public ObservableCollection<Product> ProductSuggestions { get; } = new ObservableCollection<Product>();

        /// <summary>
        /// Queries the database and updates the list of new product suggestions. 
        /// </summary>
        public async void UpdateProductSuggestions(string queryText)
        {
            ProductSuggestions.Clear();

            if (!string.IsNullOrEmpty(queryText))
            {
                var suggestions = await App.Repository.Products.GetAsync(queryText);

                foreach (Product p in suggestions)
                {
                    ProductSuggestions.Add(p);
                }
            }
        }
    }
}
