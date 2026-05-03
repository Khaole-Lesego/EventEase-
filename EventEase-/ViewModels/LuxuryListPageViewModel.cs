using System.Collections.Generic;

namespace EventEase.ViewModels
{
    public class LuxuryListPageViewModel<T>
    {
        public string EntityNamePlural { get; set; } = string.Empty;
        public string HeaderTitle { get; set; } = string.Empty;
        public string HeaderKicker { get; set; } = string.Empty;
        public string SearchPlaceholder { get; set; } = string.Empty;
        public string EmptyMessage { get; set; } = string.Empty;
        public string SearchString { get; set; } = string.Empty;
        public string ActiveTab { get; set; } = string.Empty;
        public List<T> Items { get; set; } = new();
    }
}
