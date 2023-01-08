using CommunityToolkit.Mvvm.ComponentModel;
using Decorator.DataAccess;
using System.ComponentModel;

namespace Decorator.App.ViewModels
{
    public partial class OrderDetailViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the OrderDetailWrapper class that wraps a OrderDetail object.
        /// </summary>
        public OrderDetailViewModel(OrderDetail model = null) => Model = model ?? new OrderDetail();

        public OrderDetail Model { get; }


        public ProductDimension ProductDimension
        {
            get => Model.ProductDimension;
            set
            {
                if (Model.ProductDimension != value)
                {
                    Model.ProductDimension = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => Model.Quantity;
            set
            {
                if (Model.Quantity != value)
                {
                    Model.Quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Price
        {
            get => Model.Price;
            set
            {
                if (Model.Price != value)
                {
                    Model.Price = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
