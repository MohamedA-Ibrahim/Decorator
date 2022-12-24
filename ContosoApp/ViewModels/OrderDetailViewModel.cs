using CommunityToolkit.Mvvm.ComponentModel;
using Decorator.DataAccess;

namespace Contoso.App.ViewModels
{
    public partial class OrderDetailViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the OrderDetailWrapper class that wraps a OrderDetail object.
        /// </summary>
        public OrderDetailViewModel(OrderDetail model = null) => Model = model ?? new OrderDetail();

        public OrderDetail Model { get; }

        [ObservableProperty]
        private ProductDimension _productDimension;

        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private decimal _price;
        
    }
}
