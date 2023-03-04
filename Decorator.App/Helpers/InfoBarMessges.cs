using Microsoft.UI.Xaml.Controls;

namespace Decorator.App.Helpers
{
    public static class InfoBarMessges
    {
        public static void ShowSuccessMessage(InfoBar infoBar)
        {
            infoBar.Severity = InfoBarSeverity.Success;
            infoBar.Message = "تم الحفظ بنجاح";
            infoBar.IsOpen = true;
        }

        public static void ShowErrorMessage(InfoBar infoBar)
        {
            infoBar.Severity = InfoBarSeverity.Error;
            infoBar.Message = "برجاء التأكد من ادخال جميع البيانات";
            infoBar.IsOpen = true;
        }
    }
}
