using System.Collections.ObjectModel;

namespace Shared.Utils
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> items, IEnumerable<T>? itemsToAdd)
        {
            if (itemsToAdd is null)
                return;
            foreach (T item in itemsToAdd)
            {
                items.Add(item);
            }
        }
    }
}
