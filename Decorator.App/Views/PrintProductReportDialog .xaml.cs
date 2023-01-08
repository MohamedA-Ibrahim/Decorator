using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Decorator.App.Views
{

    public sealed partial class PrintProductReportDialog : ContentDialog
    {
        public PrintProductReportDialog()
        {
            InitializeComponent();
        }

        public PrintProductReportDialogResult Result { get; private set; } = new();


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result.ButtonClicked = ButtonClicked.Print;
            Result.DateFrom = DateFromPicker.Date.Value.LocalDateTime;
            Result.DateTo = DateToPicker.Date.Value.LocalDateTime;

            Hide();
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result.ButtonClicked = ButtonClicked.Cancel;
            Hide();
        }
    }


    public class PrintProductReportDialogResult
    {
        public ButtonClicked ButtonClicked { get;set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public enum ButtonClicked
    {
        Print,
        Cancel
    }
}
