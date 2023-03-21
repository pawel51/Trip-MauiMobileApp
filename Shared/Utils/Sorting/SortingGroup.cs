namespace Shared.Utils.Sorting
{
    public class SortingGroup : List<PlaceSortItem>
    {
        public string HeaderText { get; set; } 

        public SortingGroup() { }

        public SortingGroup(string headerText, List<PlaceSortItem> items) : base(items)
        {
            HeaderText = headerText;
        } 
    }
}
