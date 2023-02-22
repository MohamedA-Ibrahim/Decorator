using CommunityToolkit.Mvvm.ComponentModel;
using Decorator.DataAccess.Models.DatabaseModels;

namespace Decorator.App.ViewModels
{
    public partial class CustomOrderItemViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the OrderDetailWrapper class that wraps a OrderDetail object.
        /// </summary>
        public CustomOrderItemViewModel(CustomOrderItem model = null) => Model = model ?? new CustomOrderItem();

        public CustomOrderItem Model { get; }

        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Quantity
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
