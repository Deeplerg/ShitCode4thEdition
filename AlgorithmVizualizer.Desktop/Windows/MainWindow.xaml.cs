using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AlgorithmVizualizer.Desktop.Windows;

public partial class MainWindow : Window
{
    private readonly IWindowActivator _activator;
    private Point? _previousPosition = null;
    private Point? _currentPosition = null;
    private readonly Button _movingButton;
    private readonly TranslateTransform _buttonTransform = new();
    
    private const int ButtonMoveOffsetPixels = 10;
    
    
    public MainWindow(IWindowActivator activator)
    {
        _activator = activator;
        
        InitializeComponent();

        _movingButton = ExitButton;
        _movingButton.RenderTransform = _buttonTransform;
    }

    private void SortVizualizationButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateWindowAndShow<SortWindow>();
    }

    private void TextSortVizualizationButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateWindowAndShow<TextSortWindow>();
    }
    
    private void TableButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateWindowAndShow<TableWindow>();
    }
    
    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
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

        var button = _movingButton;
        var buttonRectangle = button.GetRect(relativeTo: this);
        
        if (IsWithinBounds(mousePosition, buttonRectangle))
            MoveButton(button, ButtonMoveOffsetPixels);
    }

    private void SetNewMousePosition(Point newPosition)
    {
        _previousPosition = _currentPosition;
        _currentPosition = newPosition;
    }

    private bool IsWithinBounds(Point point, Rect bounds)
        => bounds.Contains(point);
    
    private void MoveButton(Button button, int offset)
    {
        if (_currentPosition is null)
            throw new ArgumentNullException(nameof(_currentPosition));
        if (_previousPosition is null)
            throw new ArgumentNullException(nameof(_previousPosition));
        
        double dx = _currentPosition.Value.X - _previousPosition.Value.X;
        double dy = _currentPosition.Value.Y - _previousPosition.Value.Y;
        
        _buttonTransform.X += dx + (dx != 0 ? (dx < 0 ? -offset : offset) : 0);
        _buttonTransform.Y += dy + (dy != 0 ? (dy < 0 ? -offset : offset) : 0);
        button.RenderTransform = _buttonTransform;
    }
    
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        MessageBox.Show("Используйте кнопку выхода!", "Неправильно!", MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}