using Shared.GoogleApiModels;
using System.Collections.ObjectModel;

namespace Shared.Utils.Sorting
{
    public class PlaceListSorter
    {
        public void SortByNameAsc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderBy(item => item.Candidate.Name));
            list.Clear();
            list.AddRange(orderedList);
        }
                

        public void SortByNameDesc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderByDescending(item => item.Candidate.Name));
            list.Clear();
            list.AddRange(orderedList);
        }

        public void SortByUserTotalAsc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderBy(item => item.Candidate.UserRatingsTotal));
            list.Clear();
            list.AddRange(orderedList);
        }


        public void SortByUserTotalsDesc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderByDescending(item => item.Candidate.UserRatingsTotal));
            list.Clear();
            list.AddRange(orderedList);
        }

        public void SortByRatingsAsc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderBy(item => item.Candidate.Rating));
            list.Clear();
            list.AddRange(orderedList);
        }


        public void SortByRatingsDesc(ObservableCollection<BasicSearchItem> list)
        {

            var orderedList = new List<BasicSearchItem>(list.OrderByDescending(item => item.Candidate.Rating));
            list.Clear();
            list.AddRange(orderedList);
        }

        public void SortByDistanceAsc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderBy(item => item.DistanceInMeters));
            list.Clear();
            list.AddRange(orderedList);
        }


        public void SortByDistanceDesc(ObservableCollection<BasicSearchItem> list)
        {
            var orderedList = new List<BasicSearchItem>(list.OrderByDescending(item => item.DistanceInMeters));
            list.Clear();
            list.AddRange(orderedList);
        }
    }
}
