using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System;

namespace Contoso.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Decorator";
        }
    }
}
