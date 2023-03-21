using Shared.GoogleApiModels;
using System.Collections.ObjectModel;

namespace Shared.Utils.Sorting
{
    public class PlaceSortItem
    {
        public bool CanSort { get; set; } = true;
        public required SortingEnum Type { get; set; }

        public required string Text { get; set; }

        public required Action<ObservableCollection<BasicSearchItem>> SortFunc { get; set; }
    }
}
