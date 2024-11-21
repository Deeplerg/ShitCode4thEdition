using System.Windows;

namespace AlgorithmVizualizer.Desktop;

public interface IWindowActivator
{
    T CreateInstance<T>() where T : Window;
}