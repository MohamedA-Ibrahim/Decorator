using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;

namespace Contoso.App.ViewModels
{
    /// <summary>
    /// Provides data and commands accessible to the entire app.  
    /// </summary>
    public class MainViewModel : BindableBase
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Creates a new MainViewModel.
        /// </summary>
        public MainViewModel() => Task.Run(GetCustomerListAsync);

        public ObservableCollection<ProductViewModel> Customers { get; } = new ObservableCollection<ProductViewModel>();

        private ProductViewModel _selectedCustomer;

        /// <summary>
        /// Gets or sets the selected customer, or null if no customer is selected. 
        /// </summary>
        public ProductViewModel SelectedCustomer
        {
            get => _selectedCustomer;
            set => Set(ref _selectedCustomer, value);
        }

        private bool _isLoading = false;

        /// <summary>
        /// Gets or sets a value indicating whether the Customers list is currently being updated. 
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading; 
            set => Set(ref _isLoading, value);
        }

        /// <summary>
        /// Gets the complete list of customers from the database.
        /// </summary>
        public async Task GetCustomerListAsync()
        {
            await dispatcherQueue.EnqueueAsync(() => IsLoading = true);

            var customers = await App.Repository.Customers.GetAsync();
            if (customers == null)
            {
                return;
            }

            await dispatcherQueue.EnqueueAsync((System.Action)(() =>
            {
                Customers.Clear();
                foreach (var c in customers)
                {
                    Customers.Add((ProductViewModel)new ViewModels.CustomerViewModel(c));
                }
                IsLoading = false;
            }));
        }

        /// <summary>
        /// Saves any modified customers and reloads the customer list from the database.
        /// </summary>
        public void Sync()
        {
            Task.Run(async () =>
            {
                //foreach (var modifiedCustomer in Customers
                //    .Where(customer => customer.IsModified).Select(customer => customer.Model))
                //{
                //    await App.Repository.Customers.UpsertAsync(modifiedCustomer);
                //}

                await GetCustomerListAsync();
            });
        }
    }
}
