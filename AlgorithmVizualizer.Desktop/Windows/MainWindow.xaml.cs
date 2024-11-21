using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AlgorithmVizualizer.Desktop.Windows;

public partial class MainWindow : Window
{
    private readonly IWindowActivator _activator;
    private Point? _previousPosition = null;
    private Point? _currentPosition = null;
    private readonly TranslateTransform _buttonTransform = new();

    private const int ButtonMoveOffsetPixels = 10;
    
    public MainWindow(IWindowActivator activator)
    {
        _activator = activator;
        
        InitializeComponent();

        SortVizualizationButton.RenderTransform = _buttonTransform;
    }

    private void SortVizualizationButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateWindowAndShow<SortWindow>();
    }
    
    private void CreateWindowAndShow<TWindow>() where TWindow : Window
    {
        var window = _activator.CreateInstance<TWindow>();
        window.Show();
    }
    
    private void Window_OnMouseMove(object sender, MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(this);
        SetNewMousePosition(mousePosition);
        
        var buttonRectangle = SortVizualizationButton.GetRect(relativeTo: this);
        
        if (IsWithinBounds(mousePosition, buttonRectangle))
            MoveButton(ButtonMoveOffsetPixels);
    }

    private void SetNewMousePosition(Point newPosition)
    {
        _previousPosition = _currentPosition;
        _currentPosition = newPosition;
    }

    private bool IsWithinBounds(Point point, Rect bounds)
        => bounds.Contains(point);
    
    private void MoveButton(int offset)
    {
        if (_currentPosition is null)
            throw new ArgumentNullException(nameof(_currentPosition));
        if (_previousPosition is null)
            throw new ArgumentNullException(nameof(_previousPosition));
        
        double dx = _currentPosition.Value.X - _previousPosition.Value.X;
        double dy = _currentPosition.Value.Y - _previousPosition.Value.Y;
        
        _buttonTransform.X += dx + (dx != 0 ? (dx < 0 ? -offset : offset) : 0);
        _buttonTransform.Y += dy + (dy != 0 ? (dy < 0 ? -offset : offset) : 0);
        SortVizualizationButton.RenderTransform = _buttonTransform;
    }
}