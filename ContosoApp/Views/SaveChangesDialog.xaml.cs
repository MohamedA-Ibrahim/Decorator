
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Contoso.App.Views
{
    /// <summary>
    /// Creates a dialog that gives the users a chance to save changes, discard them, 
    /// or cancel the operation that trigggered the event. 
    /// </summary>
    public sealed partial class SaveChangesDialog : ContentDialog
    {
        /// <summary>
        /// Initializes a new instance of the SaveChangesDialog class.
        /// </summary>
        public SaveChangesDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the user's choice. 
        /// </summary>
        public SaveChangesDialogResult Result { get; private set; } = SaveChangesDialogResult.Cancel;

        /// <summary>
        /// Occurs when the user chooses to save. 
        /// </summary>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = SaveChangesDialogResult.Save;
            Hide();
        }

        /// <summary>
        /// Occurs when the user chooses to discard changes.
        /// </summary>
        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = SaveChangesDialogResult.DontSave;
            Hide();
        }

        /// <summary>
        /// Occurs when the user chooses to cancel the operation that triggered the event.
        /// </summary>
        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = SaveChangesDialogResult.Cancel;
            Hide();
        }
    }

    /// <summary>
    /// Defines the choices available to the user. 
    /// </summary>
    public enum SaveChangesDialogResult
    {
        Save,
        DontSave,
        Cancel
    }
}
