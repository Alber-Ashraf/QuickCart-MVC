namespace QuickCart.Models.ViewModels
{
    public class PageHeaderVM
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? IconClass { get; set; }
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
        public string? ActionIcon { get; set; }
        public string? SecondaryActionText { get; set; }
        public string? SecondaryActionUrl { get; set; }
        public string? SecondaryActionIcon { get; set; }
    }
}
