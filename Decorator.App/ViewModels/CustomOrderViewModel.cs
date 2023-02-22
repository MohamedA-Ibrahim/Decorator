using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Decorator.App.Reporting;
using Decorator.DataAccess.Models.DatabaseModels;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Decorator.App.ViewModels
{
    public partial class CustomOrderViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        #region Constructor

        public CustomOrderViewModel(CustomOrder model)
        {
            Model = model;

            _orderItems = new ObservableCollection<CustomOrderItem>(Model.CustomOrderItems);
            OrderItems.CollectionChanged += OrderItems_Changed;

            NewCustomOrderItem = new CustomOrderItemViewModel();
        }

        #endregion

        #region Private Binding Fields

        private CustomOrderItemViewModel _newCustomOrderItem;
        private ObservableCollection<CustomOrderItem> _orderItems;
        private bool _isInEdit = false;
        private bool _IsModified = false;

        #endregion

        #region Public Binding Fields

        public ObservableCollection<CustomOrderItem> OrderItems
        {
            get => _orderItems;
            set
            {
                if (_orderItems != value)
                {
                    if (value != null)
                    {
                        value.CollectionChanged += OrderItems_Changed;
                    }

                    if (_orderItems != null)
                    {
                        _orderItems.CollectionChanged -= OrderItems_Changed;
                    }
                    _orderItems = value;
                    OnPropertyChanged();
                    IsModified = true;
                }
            }
        }

        public CustomOrder Model { get; }

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
                    OnPropertyChanged(nameof(UnpaidAmount));

                    IsModified = true;
                }
            }
        }

        public float PaidAmount
        {
            get => Model.PaidAmount;
            set
            {
                if (Model.PaidAmount != value)
                {
                    Model.PaidAmount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UnpaidAmount));
                    IsModified = true;
                }
            }
        }

        public float GrandTotal => Model.GrandTotal;
        public float UnpaidAmount => Model.UnpaidAmount;

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
        public bool HasNewCustomOrderItem => NewCustomOrderItem != null;

     
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
        public CustomOrderItemViewModel NewCustomOrderItem
        {
            get => _newCustomOrderItem;
            set
            {
                if (value != _newCustomOrderItem)
                {
                    if (value != null)
                    {
                        value.PropertyChanged += NewCustomOrderItem_PropertyChanged;
                    }

                    if (_newCustomOrderItem != null)
                    {
                        _newCustomOrderItem.PropertyChanged -= NewCustomOrderItem_PropertyChanged;
                    }

                    _newCustomOrderItem = value;
                    UpdateNewCustomOrderItemBindings();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an OrderViewModel that wraps an Order object created from the specified ID.
        /// </summary>
        public async static Task<CustomOrderViewModel> CreateFromId(int orderId) => new CustomOrderViewModel(await GetCustomOrder(orderId));

        public async Task SaveOrderAsync()
        {

            CustomOrder result = null;
            try
            {
                result = await App.Repository.CustomOrders.UpsertAsync(Model);
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

        #endregion

        #region Reporting

        public void PrintOrderInvoice()
        {
            ReportGenerator.GenerateCustomOrderReport(Model);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Notifies anyone listening to this object that an order detail changed. 
        /// </summary>
        private void OrderItems_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (OrderItems != null)
            {
                Model.CustomOrderItems = OrderItems.ToList();
            }

            OnPropertyChanged(nameof(OrderItems));

            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(GrandTotal));
            OnPropertyChanged(nameof(UnpaidAmount));

            IsModified = true;
        }
        private void NewCustomOrderItem_PropertyChanged(object sender, PropertyChangedEventArgs e) => UpdateNewCustomOrderItemBindings();
        private void UpdateNewCustomOrderItemBindings()
        {
            OnPropertyChanged(nameof(NewCustomOrderItem));
            OnPropertyChanged(nameof(HasNewCustomOrderItem));
        }

        /// <summary>
        /// Returns the order with the specified ID.
        /// </summary>
        private static async Task<CustomOrder> GetCustomOrder(int orderId) => await App.Repository.CustomOrders.GetAsync(orderId);

        #endregion
    }
}
