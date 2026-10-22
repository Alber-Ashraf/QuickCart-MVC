namespace QuickCart.Models.ViewModels
{
    public class EmptyStateVM
    {
        public string Title { get; set; } = "No items found";
        public string Description { get; set; } = "There are currently no items to display.";
        public string IconClass { get; set; } = "bi bi-inbox";
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
    }
}
