using System.Windows;

namespace AlgorithmVizualizer.Desktop;

internal static class FrameworkElementExtensions
{
    public static Rect GetRect(this FrameworkElement element, FrameworkElement relativeTo)
    {
        var pointZero = new Point();
        
        var position = element
            .TransformToAncestor(relativeTo)
            .Transform(pointZero);
        
        var elementSize = new Size(element.Width, element.Height);
        return new Rect(position, elementSize);
    }
}