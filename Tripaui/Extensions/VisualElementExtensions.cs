namespace Tripaui.Extensions
{
    public static class VisualElementExtensions
    {
        public static void ChangeColorTo(this VisualElement e, int r, int g, int b)
        {
            e.BackgroundColor = Color.FromRgb(r, g, b);
        }
    }
}
