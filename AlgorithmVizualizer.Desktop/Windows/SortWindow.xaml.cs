using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgorithmVizualizer.Desktop.Windows
{
    public partial class SortWindow : Window
    {
        private int[] _array; // Массив для сортировки
        private  int _arraySize = 1; // Размер массива
        private bool _isSorting = false; // Флаг для проверки выполнения сортировки

        public SortWindow()
        {
            InitializeComponent();
            InitializeAlgorithms();
            DelaySlider.Value = 150;
            ArrayLengthSlider.Value = 50;
        }

        // Инициализация списка алгоритмов
        private void InitializeAlgorithms()
        {
            QuadraticAlgorithmComboBox.Items.Add("Сортировка выбором (SelectSort)");
            QuadraticAlgorithmComboBox.Items.Add("Сортировка пузырьком (BubbleSort)");
            QuadraticAlgorithmComboBox.SelectedIndex = 0;

            ImprovedAlgorithmComboBox.Items.Add("Быстрая сортировка (QuickSort)");
            ImprovedAlgorithmComboBox.Items.Add("Пирамидальная сортировка (HeapSort)");
            ImprovedAlgorithmComboBox.SelectedIndex = 0;
        }

        // Генерация случайного массива
        private void GenerateArray_Click(object sender, RoutedEventArgs e)
        {
            _arraySize = (int)ArrayLengthSlider.Value;
            var random = new Random();
            _array = Enumerable.Range(1, _arraySize).OrderBy(_ => random.Next()).ToArray();
            UpdateVisualization(_array);
            LogTextBox.Clear();
            Log("Массив сгенерирован.");
        }

        // Запуск сортировки
        private async void StartSortingQuadro_Click(object sender, RoutedEventArgs e)
        {
            StartSortingImprovedButton.IsEnabled = false;
            if (_array.Length == 0)
            {
                Log("Массив не сгенерирован!");
                return;
            }

            int delay = (int)DelaySlider.Value;

            _isSorting = true;
            StartSortingQuadroButton.IsEnabled = false;
            StopSortingButton.IsEnabled = true; 
            // Выбор алгоритма
            string? quadraticAlgorithm = QuadraticAlgorithmComboBox.SelectedItem.ToString();
            int[] arrayCopy = (int[])_array.Clone();

            // Выполнение квадратичного алгоритма
            Log($"Запускаем {quadraticAlgorithm}...");
            if (quadraticAlgorithm != null && quadraticAlgorithm.Contains("SelectSort"))
                await SelectionSort(_array, delay);
            else if (quadraticAlgorithm != null && quadraticAlgorithm.Contains("BubbleSort"))
                await BubbleSort(_array, delay);


            if (_isSorting)
            {
                Log("Сортировка завершена.");
            }

            // Разблокируем интерфейс
            _isSorting = false;
            StartSortingQuadroButton.IsEnabled = true;
            StartSortingImprovedButton.IsEnabled = true;
            StopSortingButton.IsEnabled = false;
        }

        private async void StartSortingImproved_Click(object sender, RoutedEventArgs e)
        {
            StartSortingQuadroButton.IsEnabled = false;

            if (_array.Length == 0)
            {
                Log("Массив не сгенерирован!");
                return;
            }

            int delay = (int)DelaySlider.Value;

            _isSorting = true;
            StartSortingImprovedButton.IsEnabled = false;
            StopSortingButton.IsEnabled = true;
            // Выбор алгоритма
            string? improvedAlgorithm = ImprovedAlgorithmComboBox.SelectedItem.ToString();

            int[] arrayCopy = (int[])_array.Clone();

            // Выполнение усовершенствованного алгоритма
            Log($"Запускаем {improvedAlgorithm}...");
            if (improvedAlgorithm != null && improvedAlgorithm.Contains("QuickSort"))
                await QuickSort(arrayCopy, 0, arrayCopy.Length - 1, delay);
            else if (improvedAlgorithm != null && improvedAlgorithm.Contains("HeapSort"))
                await HeapSort(arrayCopy, delay);

            if (_isSorting)
            {
                Log("Сортировка завершена.");
            }

            // Разблокируем интерфейс
            _isSorting = false;
            StartSortingImprovedButton.IsEnabled = true;
            StartSortingQuadroButton.IsEnabled = true;
            StopSortingButton.IsEnabled = false;
        }

        // Логирование
        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }

        // Обновление визуализации массива
        private void UpdateVisualization(int[] array, int index1 = -1, int index2 = -1)
        {
            ArrayCanvas.Children.Clear();
            double barWidth = ArrayCanvas.ActualWidth / array.Length;
            double barHeight = ArrayCanvas.ActualHeight / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                var bar = new Rectangle
                {
                    Width = barWidth - 2,
                    Height = barHeight * array[i], // Масштабирование для визуализации
                    Fill = (i == index1 || i == index2) ? Brushes.Red : Brushes.Blue
                };
                Canvas.SetLeft(bar, i * barWidth);
                Canvas.SetBottom(bar, 0);
                ArrayCanvas.Children.Add(bar);
            }
        }

        private void UpdateVisualization(int[] array, int index1, int index2, int elementQuick)
        {
            ArrayCanvas.Children.Clear();
            double barWidth = ArrayCanvas.ActualWidth / array.Length;
            double barHeight = ArrayCanvas.ActualHeight / array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                var bar = new Rectangle
                {
                    Width = barWidth - 2,
                    Height = barHeight * array[i], // Масштабирование для визуализации
                    Fill = (i == index1 || i == index2) ? Brushes.Red : (i == elementQuick) ? Brushes.Green : Brushes.Blue
                };
                Canvas.SetLeft(bar, i * barWidth);
                Canvas.SetBottom(bar, 0);
                ArrayCanvas.Children.Add(bar);
            }
        }
        
        // Сортировка выбором
        private async Task SelectionSort(int[] array, int delay)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (!_isSorting) return; // Прерывание при остановке

                    Log($"Сравниваем: {array[j]} и {array[minIndex]}");
                    UpdateVisualization(array, j, minIndex);
                    if (array[j] < array[minIndex])
                        minIndex = j;

                    await Task.Delay(delay);
                    UpdateVisualization(array, i, j);
                }

                if (minIndex != i)
                {
                    Log($"Меняем местами: {array[i]} и {array[minIndex]}");
                    (array[i], array[minIndex]) = (array[minIndex], array[i]);
                    await Task.Delay(delay);
                    UpdateVisualization(array, i, minIndex);
                }
            }
        }

        // Сортировка пузырьком
        private async Task BubbleSort(int[] array, int delay)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    if (!_isSorting) return; // Прерывание при остановке

                    Log($"Сравниваем: {array[j]} и {array[j + 1]}");
                    UpdateVisualization(array, j, j + 1);
                    await Task.Delay(delay);
                    if (array[j] > array[j + 1])
                    {
                        Log($"Меняем местами: {array[j]} и {array[j + 1]}");
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        await Task.Delay(delay);
                        UpdateVisualization(array, j, j + 1);
                    }
                }
            }
        }

        // Быстрая сортировка
        private async Task QuickSort(int[] array, int left, int right, int delay)
        {
            if (left >= right) return;

            int pivotIndex = await Partition(array, left, right, delay);

            await QuickSort(array, left, pivotIndex - 1, delay);
            await QuickSort(array, pivotIndex + 1, right, delay);
        }

        private async Task<int> Partition(int[] array, int left, int right, int delay)
        {
            int pivot = array[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (!_isSorting) return i + 1; // Прерывание при остановке

                Log($"Сравниваем: {array[j]} с опорным {pivot}");
                UpdateVisualization(array, j, j, right);
                if (array[j] < pivot)
                {
                    i++;
                    Log($"Меняем местами: {array[i]} и {array[j]}");
                    (array[i], array[j]) = (array[j], array[i]);
                    await Task.Delay(delay);
                    UpdateVisualization(array, i, j, right);
                    await Task.Delay(delay);
                }
            }

            Log($"Ставим опорный {pivot} на позицию {i + 1}");
            (array[i + 1], array[right]) = (array[right], array[i + 1]);
            await Task.Delay(delay);
            UpdateVisualization(array, i + 1, i + 1, right);

            return i + 1;
        }

        // Пирамидальная сортировка
        private async Task HeapSort(int[] array, int delay)
        {
            int n = array.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
                await Heapify(array, n, i, delay);

            for (int i = n - 1; i >= 0; i--)
            {
                if (!_isSorting) return; // Прерывание при остановке

                Log($"Меняем местами: {array[0]} и {array[i]}");
                (array[0], array[i]) = (array[i], array[0]);
                await Task.Delay(delay);
                UpdateVisualization(array, 0, i);

                await Heapify(array, i, 0, delay);
            }
        }

        private void StopSorting_Click(object sender, RoutedEventArgs e)
        {
            _isSorting = false; // Устанавливаем флаг, чтобы остановить текущую сортировку
            Log("Сортировка остановлена.");
        }

        private async Task Heapify(int[] array, int n, int i, int delay)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && array[left] > array[largest])
                largest = left;

            if (right < n && array[right] > array[largest])
                largest = right;

            if (largest != i)
            {
                if (!_isSorting) return; // Прерывание при остановке

                Log($"Меняем местами: {array[i]} и {array[largest]}");
                (array[i], array[largest]) = (array[largest], array[i]);
                await Task.Delay(delay);
                UpdateVisualization(array, i, largest);

                await Heapify(array, n, largest, delay);
            }
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
