using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SortingText;

namespace AlgorithmVizualizer.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for TextSortWindow.xaml
    /// </summary>
    public partial class TextSortWindow : Window
    {
        public TextSortWindow()
        {
            InitializeComponent();
            algComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = textBox.Text;
            string? selectedAlgorithmName = (algComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedAlgorithmName)) return;
            
            ISortAlgorithm? selectedAlgorithm = null;
            switch (selectedAlgorithmName)
            {
                case "BuiltInSort":
                {
                    selectedAlgorithm = new BuiltInSort();
                    selectedAlgorithmName = "встроенным";
                    break;
                }
                case "RadixSort":
                {
                    selectedAlgorithm = new RadixSort();
                    selectedAlgorithmName = "radix";
                    break;
                }
            }
            
            var sortProcessor = new TextProcessor(selectedAlgorithm);
            string[]? words = sortProcessor.SplitText(text);
            string[]? sorted = sortProcessor.SortWords(words);
            Dictionary<string, int> counts = sortProcessor.CountWords(sorted);

            lbl.Content = $"Сортировка {selectedAlgorithmName} методом:\n";
            lbl.Content += string.Join(", ", sorted) + "\n\n";
            lbl.Content += "Подсчёт слов:\n";
            foreach (var pair in counts)
            {
                lbl.Content += $"{pair.Key}: {pair.Value}\n";
            }
        }
    }
}
