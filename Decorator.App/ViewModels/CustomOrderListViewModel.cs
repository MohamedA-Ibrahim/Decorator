using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Decorator.DataAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using Decorator.DataAccess.Models.DatabaseModels;

namespace Decorator.App.ViewModels
{
    public partial class CustomOrderListViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public CustomOrderListViewModel() => IsLoading = false;

        /// <summary>
        /// Gets the unfiltered collection of all orders. 
        /// </summary>
        private List<CustomOrder> MasterOrdersList { get; } = new List<CustomOrder>();

        /// <summary>
        /// Gets the orders to display.
        /// </summary>
        public ObservableCollection<CustomOrder> Orders { get; private set; } = new ObservableCollection<CustomOrder>();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private CustomOrder _selectedOrder;

        /// <summary>
        /// Retrieves orders from the data source.
        /// </summary>
        public async void LoadOrders()
        {
            await dispatcherQueue.EnqueueAsync(() =>
            {
                IsLoading = true;
                Orders.Clear();
                MasterOrdersList.Clear();
            });

            var orders = await Task.Run(App.Repository.CustomOrders.GetAsync);

            await dispatcherQueue.EnqueueAsync(() =>
            {
                foreach (var order in orders)
                {
                    Orders.Add(order);
                    MasterOrdersList.Add(order);
                }

                IsLoading = false;
            });
        }

        public async void SearchOrders(string query)
        {
            string[] parameters = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrWhiteSpace(query))
            {
                IsLoading = true;
                Orders.Clear();
                var results = MasterOrdersList
                            .Where(order => parameters
                                .Any(parameter =>
                                    order.CustomerName.Contains(parameter) ||
                                    order.InvoiceNumber.ToString().StartsWith(parameter)))
                            .OrderByDescending(order => parameters
                                .Count(parameter =>
                                    order.CustomerName.Contains(parameter) ||
                                    order.InvoiceNumber.ToString().StartsWith(parameter)));

                await dispatcherQueue.EnqueueAsync(() =>
                {
                    foreach (CustomOrder o in results)
                    {
                        Orders.Add(o);
                    }
                });
                IsLoading = false;
            }
        }


        public async Task DeleteOrder(CustomOrder orderToDelete) =>
            await App.Repository.CustomOrders.DeleteAsync(orderToDelete.Id);

        public ObservableCollection<CustomOrder> OrderSuggestions { get; } = new ObservableCollection<CustomOrder>();

    }
}
