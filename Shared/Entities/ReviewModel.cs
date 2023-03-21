using CommunityToolkit.Mvvm.ComponentModel;

namespace Shared.Entities
{
    public sealed partial class ReviewModel : ObservableObject, ICloneable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private string text = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ModifiedAt { get; set; }

        public byte[] Photo { get; set; }

        public string PlaceId { get; set; } = "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
