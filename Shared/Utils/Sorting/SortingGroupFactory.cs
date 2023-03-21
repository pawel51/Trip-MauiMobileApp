using Shared.GoogleApiModels;
using System.Collections.ObjectModel;

namespace Shared.Utils.Sorting
{
    public class SortingGroupFactory
    {
        private readonly ObservableCollection<BasicSearchItem> _sortingList;

        private readonly PlaceListSorter sorter;

        public SortingGroupFactory(ObservableCollection<BasicSearchItem> sortingList) : base()
        {
            _sortingList = sortingList;
            sorter = new();
        }

        public SortingGroup CreateRatingGroup()
        {
            SortingGroup sg = new();
            sg.HeaderText = "Ratings";
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by lowest ratings",
                SortFunc = sorter.SortByRatingsAsc,
                Type = SortingEnum.RatingsDesc
            });
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by top ratings",
                SortFunc = sorter.SortByRatingsDesc,
                Type = SortingEnum.RatingsAsc
            });
            return sg;
        }

        public SortingGroup CreateNameGroup()
        {
            SortingGroup sg = new();
            sg.HeaderText = "Name";
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort Z - A",
                SortFunc = sorter.SortByNameDesc,
                Type = SortingEnum.NameDesc
            });
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort A - Z",
                SortFunc = sorter.SortByNameAsc,
                Type = SortingEnum.NameAsc
            });
            return sg;
        }

        public SortingGroup CreateUserAmountGroup()
        {
            SortingGroup sg = new();
            sg.HeaderText = "Users";
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by fewest reviews",
                SortFunc = sorter.SortByUserTotalAsc,
                Type = SortingEnum.NameDesc
            });
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by most reviews",
                SortFunc = sorter.SortByUserTotalsDesc,
                Type = SortingEnum.NameAsc
            });
            return sg;
        }

        public SortingGroup CreateDistanceGroup()
        {
            SortingGroup sg = new();
            sg.HeaderText = "Distance";
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by closest",
                SortFunc = sorter.SortByDistanceAsc,
                Type = SortingEnum.DistanceAsc
            });
            sg.Add(new PlaceSortItem()
            {
                Text = "Sort by furthest",
                SortFunc = sorter.SortByDistanceDesc,
                Type = SortingEnum.DistanceDesc
            });
            return sg;
        }
    }
}
