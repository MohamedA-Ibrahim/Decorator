using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Decorator.App.ViewModels
{
    /// <summary>
    /// Provides data and commands accessible to the entire app.  
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public MainViewModel() => Task.Run(GetProductListAsync);

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

        [ObservableProperty]
        private ProductViewModel _selectedProduct;

        [ObservableProperty]
        private bool _isLoading = false;


        public async Task GetProductListAsync()
        {
            await dispatcherQueue.EnqueueAsync(() => IsLoading = true);

            var products = await App.Repository.Products.GetAsync();
            if (products == null)
            {
                return;
            }

            await dispatcherQueue.EnqueueAsync(() =>
            {
                Products.Clear();
                foreach (var c in products)
                {
                    var m = new ProductViewModel(c);
                    Products.Add(m);
                }
                IsLoading = false;
            });
        }
    }
}
