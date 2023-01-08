using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Decorator.DataAccess;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Decorator.App.ViewModels
{
    public partial class OrderListPageViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public OrderListPageViewModel() => IsLoading = false;

        /// <summary>
        /// Gets the unfiltered collection of all orders. 
        /// </summary>
        private List<Order> MasterOrdersList { get; } = new List<Order>();

        /// <summary>
        /// Gets the orders to display.
        /// </summary>
        public ObservableCollection<Order> Orders { get; private set; } = new ObservableCollection<Order>();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private Order _selectedOrder;

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

            var orders = await Task.Run(App.Repository.Orders.GetAsync);

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
                    foreach (Order o in results)
                    {
                        Orders.Add(o);
                    }
                });
                IsLoading = false;
            }
        }


        public async Task DeleteOrder(Order orderToDelete) =>
            await App.Repository.Orders.DeleteAsync(orderToDelete.Id);

        public ObservableCollection<Order> OrderSuggestions { get; } = new ObservableCollection<Order>();

    }
}