namespace Decorator.App
{
    /// <summary>
    /// A class for passing multiple parameters from OrderListPage to OrderDetailsPage
    /// using Frame.Navigate method which only accepts one parameter
    /// </summary>
    /// <param name="SelectedOrderId">The Id of the selected order</param>
    /// <param name="IsEdit">Is it Edit</param>
    public record OrderListToDetailParameter(int SelectedOrderId, bool IsEdit);
}
