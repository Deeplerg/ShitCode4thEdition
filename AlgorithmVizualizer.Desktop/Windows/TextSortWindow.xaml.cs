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
            AlgComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = TextBox.Text;
            string? selectedAlgorithmName = (AlgComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedAlgorithmName)) return;
            
            ISortAlgorithm? selectedAlgorithm = null;
            switch (selectedAlgorithmName)
            {
                case "Сортировка слиянием (MergeSort)":
                {
                    selectedAlgorithm = new MergeSort();
                    selectedAlgorithmName = "merge";
                    break;
                }
                case "Поразрядная сортировка (RadixSort)":
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

            Lbl.Text = $"Сортировка {selectedAlgorithmName} методом:\n";
            Lbl.Text += string.Join(", ", sorted) + "\n\n";
            Lbl.Text += "Подсчёт слов:\n";
            foreach (var pair in counts)
            {
                Lbl.Text += $"{pair.Key}: {pair.Value}\n";
            }
        }
    }
}
