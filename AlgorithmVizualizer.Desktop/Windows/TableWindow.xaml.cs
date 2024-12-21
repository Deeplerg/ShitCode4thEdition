
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace AlgorithmVizualizer.Desktop.Windows;

public partial class TableWindow : Window
{
    private int _chunkSize = 8;
    private int _currentChunkStart = 0;
    private string _filePath = "A.xlsx";
    private int _columnToSortBy; // starts with 1
    private int _delayMilliseconds = 50;
    private readonly List<string[]> _currentChunk = new();
    private readonly List<int> _rowsToHighlight = new();
    private int _totalLines;
    private int _numberTempFiles = 4;
    private const string TempFileB = "B.cvs";
    private const string TempFileC = "C.cvs";
    private const string NameA = "Файл A";

    public TableWindow()
    {
        InitializeComponent();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    private void DataTable_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (_rowsToHighlight.Any(r => r == e.Row.GetIndex()))
        {
            e.Row.Style = (Style)FindResource("HighlightedRowStyle");
        }
    }

    private void SliderDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _delayMilliseconds = (int)sliderDelay.Value;

        // I have no idea why InitializeComponent doesn't finish the entire thing
        if (lblDelayValue != null)
            lblDelayValue.Content = $"{_delayMilliseconds} мс";
    }

    private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            _filePath = dlg.FileName;
            txtLog.AppendText($"Файл выбран: {_filePath}\n");
        }
    }

    private  void BtnLoadTable_Click(object sender, RoutedEventArgs e)
    {
         PrintTable(_filePath);
        
    }

    private Task Log(string message)
    {
        txtLog.AppendText( $"{message}\n");
        txtLog.ScrollToEnd();
        return Task.CompletedTask;
    }
    

    private async void BtnSortNow_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtColumnIndex.Text, out _columnToSortBy) || _columnToSortBy < 1)
        {
            MessageBox.Show("Введите корректный номер колонки.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        await Sort();
    }

// Вспомогательный класс для отображения массива строк в DataGrid

private void PrintTable(String fileName)
    {
        TextBlockFileName.Text = fileName;
        dataTable.Columns.Clear();
        dataTable.ItemsSource = null;
        var data = ReadCsvFile(fileName);

        
        if (data.Any())
        {
            int columnCount = data[0].Count();
            for (int i = 0; i < columnCount; i++)
            {
                dataTable.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Колонка {i + 1}",
                    Binding = new Binding($"[{i}]")
                });
            }
        }

        dataTable.ItemsSource = data;
         Log($"Таблица {fileName} отрисована");
    }







    private async Task Sort()
    {
        // _columnToSortBy is 1-based
        var sortColumnIndex = _columnToSortBy - 1;
        switch (cmbSortMethod.SelectedIndex)
        {
            case 0:
                await StraightMergeSortExcel(_filePath, sortColumnIndex);
                break;

            case 1:
                 await NaturalMergeSortExcel(_filePath, sortColumnIndex);
                break;

            case 2:
                await MultiwayMergeSortExcel(_filePath, sortColumnIndex, _numberTempFiles );
                return;

            default:
                MessageBox.Show("Метод сортировки не реализован.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
        }
         PrintTable(_filePath);
    }


    private Task StraightMergeSortExcel(string inputFile, int columnIndex)
    {
        string tempFileB = "TempB.xlsx";
        string tempFileC = "TempC.xlsx";

        int runSize = 1; // Начальный размер блоков
        while (true)
        {
            // Разделение на два временных файла
            StraightSplitCsvFile(inputFile, tempFileB, tempFileC);

            // Если один из временных файлов пуст, процесс завершён
            if (IsSorted(inputFile, columnIndex))
            {
                File.Copy(tempFileB, inputFile, true);
                File.Delete(tempFileB);
                File.Delete(tempFileC);
                break;
            }

            // Слияние временных файлов обратно в исходный файл
            StraightMergeCsvFiles(inputFile , tempFileB, tempFileC, columnIndex);

            // Увеличиваем размер блоков
            runSize *= 2;

            // Удаление временных файлов после слияния
            File.Delete(tempFileB);
            File.Delete(tempFileC);
        }

        return Task.CompletedTask;
    }


    private async Task NaturalMergeSortExcel(string inputFile, int columnIndex)
    {
        var tempFileB = TempFileB;
        var tempFileC = TempFileC;

        while (true)
        {
            // Разделение файла на две части (естественное)
             await NaturalSplitExcelFile(inputFile, tempFileB, tempFileC, columnIndex);
             await Log("Произведено разделение");
            // Печать текущих частей для отладки
             PrintTable(tempFileB);
            Thread.Sleep(_delayMilliseconds);
             PrintTable(tempFileC);
            Thread.Sleep(_delayMilliseconds);

            // Если один из временных файлов пуст, сортировка завершена
            if (IsSorted(inputFile, columnIndex))
            {
                File.Copy(tempFileB, inputFile, true);
                File.Delete(tempFileB);
                File.Delete(tempFileC);
                break;
            }

            // Слияние разделённых частей
             await NaturalMergeExcelFiles(inputFile, tempFileB, tempFileC, columnIndex);
             PrintTable(inputFile);
             await Log("Произведено слияние");
            Thread.Sleep(_delayMilliseconds);

            // Удаление временных файлов после слияния
            File.Delete(tempFileB);
            File.Delete(tempFileC);

            // Защита от бесконечного цикла: если данные не меняются
            if (new FileInfo(inputFile).Length == 0)
            {
                throw new InvalidOperationException("Sorting failed: input file became empty.");
            }
        }

        // Финальный вывод отсортированного файла
         PrintTable(inputFile);
    }




    public async Task MultiwayMergeSortExcel(string inputFile, int columnIndex, int numWays)
    {
        // Создание временных файлов для каждого пути
        var tempFiles = new string[numWays];
        for (var i = 0; i < numWays; i++)
        {
            tempFiles[i] = $"Temp_{i}.xlsx";
        }

        while (true)
        {
            // Разделение исходного файла на несколько частей
            MultiwaySplitCsvFile(inputFile, tempFiles);

            // Проверка, осталась ли одна непустая часть
            var nonEmptyFileCount = tempFiles.Count(file => new FileInfo(file).Length > 0);
            if (IsSorted(inputFile, columnIndex))
            {
                break;
            }

            // Слияние всех путей
            MultiwayMergeCsvFiles(inputFile, tempFiles, columnIndex);

            // Обновление исходного файла для следующей итерации
        }
    }

    private bool IsSorted(string inputFile, int columnIndex)
    {
        
        int rowCount = GetRowCountInCsv(inputFile);
        for (int row = 2; row < rowCount; row++)
        {
            var currentValue = ReadValueFromCsv(inputFile, row, columnIndex);
            var previousValue = ReadValueFromCsv(inputFile, row - 1, columnIndex);

            if (!string.IsNullOrEmpty(currentValue) &&
                !string.IsNullOrEmpty(previousValue) &&
                String.CompareOrdinal(currentValue, previousValue) < 0)
            {
                return false;
            }
        }
        return true;
    }

    private async Task NaturalMergeExcelFiles(string inputFile, string tempFileB, string tempFileC, int columnIndex)
{
    // Удаляем старый файл и создаем новый
    if (File.Exists(inputFile)) File.Delete(inputFile);
    File.Create(inputFile).Dispose();

    var rowCountB = GetRowCountInCsv(tempFileB);
    var rowCountC = GetRowCountInCsv(tempFileC);

    int currentRowB = 0, currentRowC = 0;

    // Объединяем строки
    while (currentRowB < rowCountB || currentRowC < rowCountC)
    {
        bool writeFromB = false;

        if (currentRowB < rowCountB && currentRowC < rowCountC)
        {
            var valueB = ReadValueFromCsv(tempFileB, currentRowB, columnIndex);
            var valueC = ReadValueFromCsv(tempFileC, currentRowC, columnIndex);

            // Сравниваем значения из файлов B и C
            if (string.CompareOrdinal(valueB, valueC) <= 0)
            {
                writeFromB = true;
            }
        }
        else if (currentRowB < rowCountB)
        {
            writeFromB = true;
        }

        if (writeFromB)
        {
            CopyRowToAnotherCsv(tempFileB, inputFile, currentRowB);
            currentRowB++;
        }
        else
        {
            CopyRowToAnotherCsv(tempFileC, inputFile, currentRowC);
            currentRowC++;
        }
    }
}

private async Task NaturalSplitExcelFile(string inputFile, string tempFileB, string tempFileC, int columnIndex)
{
    int rowCount = GetRowCountInCsv(inputFile);

    // Удаляем временные файлы, если они существуют
    if (File.Exists(tempFileB)) File.Delete(tempFileB);
    if (File.Exists(tempFileC)) File.Delete(tempFileC);

    File.Create(tempFileB).Dispose();
    File.Create(tempFileC).Dispose();

    int currentRowB = 0, currentRowC = 0;
    bool writeToB = true;

    // Разделяем строки
    for (int row = 0; row < rowCount; row++)
    {
        if (row > 0)
        {
            var currentValue = ReadValueFromCsv(inputFile, row, columnIndex);
            var previousValue = ReadValueFromCsv(inputFile, row - 1, columnIndex);

            // Смена файла записи, если порядок нарушен
            if (!string.IsNullOrEmpty(currentValue) &&
                !string.IsNullOrEmpty(previousValue) &&
                String.CompareOrdinal(currentValue, previousValue) < 0)
            {
                writeToB = !writeToB;
            }
        }

        var targetFile = writeToB ? tempFileB : tempFileC;
        if (writeToB)
        {
            CopyRowToAnotherCsv(inputFile, tempFileB, row);
            currentRowB++;
        }
        else
        {
            CopyRowToAnotherCsv(inputFile, tempFileC, row);
            currentRowC++;
        }
    }
}

public void CopyRowToAnotherCsv(string sourceFilePath, string targetFilePath, int rowIndex)
{
    var sourceLines = File.ReadAllLines(sourceFilePath).ToList();

    if (rowIndex >= sourceLines.Count)
    {
        throw new IndexOutOfRangeException("Row index is out of range.");
    }

    var rowToCopy = sourceLines[rowIndex];

    // Добавляем строку в целевой файл
    using (var writer = new StreamWriter(targetFilePath, append: true))
    {
        writer.WriteLine(rowToCopy);
    }
}

private int GetRowCountInCsv(string filePath)
{
    if (!File.Exists(filePath))
        throw new FileNotFoundException($"File not found: {filePath}");

    return File.ReadAllLines(filePath).Length;
}

private string ReadValueFromCsv(string filePath, int rowIndex, int columnIndex)
{
    var lines = File.ReadAllLines(filePath);
    if (rowIndex >= lines.Length)
        throw new IndexOutOfRangeException("Row index is out of range.");

    var columns = lines[rowIndex].Split(',');
    if (columnIndex >= columns.Length)
        throw new IndexOutOfRangeException("Column index is out of range.");

    return columns[columnIndex];
}


    
private void MultiwayMergeCsvFiles(string outputFile, string[] tempFiles, int columnIndex)
{
    var tempData = tempFiles.Select(file => ReadCsvFile(file)).ToList();
    var indices = new int[tempData.Count];
    var mergedData = new List<List<string>>();

    // Пока есть строки в любом из временных файлов
    while (tempData.Any(t => indices[tempData.IndexOf(t)] < t.Count))
    {
        List<string> smallestRow = null;
        int smallestIndex = -1;

        // Ищем минимальную строку среди текущих строк из каждого файла
        for (int i = 0; i < tempData.Count; i++)
        {
            if (indices[i] < tempData[i].Count &&
                (smallestRow == null || IsSorted(tempData[i][indices[i]], smallestRow, columnIndex)))
            {
                smallestRow = tempData[i][indices[i]];
                smallestIndex = i;
            }
        }

        if (smallestRow != null)
        {
            mergedData.Add(smallestRow);
            indices[smallestIndex]++;
        }
    }

    // Записываем итоговые данные в выходной файл
    WriteCsvFile(outputFile, mergedData);
}

public void MultiwaySplitCsvFile(string inputFile, string[] tempFiles)
{
    var data = ReadCsvFile(inputFile);
    int numChunks = tempFiles.Length;
    int chunkSize = (int)Math.Ceiling(data.Count / (double)numChunks);

    for (int i = 0; i < numChunks; i++)
    {
        var chunk = data.Skip(i * chunkSize).Take(chunkSize).ToList();
        WriteCsvFile(tempFiles[i], chunk);
    }
}



    public void StraightMergeCsvFiles(string outputFile, string tempFileB, string tempFileC, int columnIndex)
    {
        var dataB = ReadCsvFile(tempFileB);
        var dataC = ReadCsvFile(tempFileC);

        var mergedData = new List<List<string>>();
        int i = 0, j = 0;

        // Слияние данных
        while (i < dataB.Count && j < dataC.Count)
        {
            if (IsSorted(dataB[i], dataC[j], columnIndex))
                mergedData.Add(dataB[i++]);
            else
                mergedData.Add(dataC[j++]);
        }

        // Добавляем оставшиеся строки из tempFileB
        while (i < dataB.Count)
            mergedData.Add(dataB[i++]);

        // Добавляем оставшиеся строки из tempFileC
        while (j < dataC.Count)
            mergedData.Add(dataC[j++]);

        WriteCsvFile(outputFile, mergedData);
    }

    public void StraightSplitCsvFile(string inputFile, string tempFileB, string tempFileC)
    {
        var data = ReadCsvFile(inputFile);
        int size = (int)Math.Ceiling(data.Count / 2.0);

        // Разделяем данные на две части
        var partB = data.Take(size).ToList();
        var partC = data.Skip(size).ToList();

        WriteCsvFile(tempFileB, partB);
        WriteCsvFile(tempFileC, partC);
    }

    private bool IsSorted(List<string> row1, List<string> row2, int columnIndex)
    {
        return String.CompareOrdinal(row1[columnIndex], row2[columnIndex]) <= 0;
    }

    private void WriteCsvFile(string filePath, List<List<string>> data)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var row in data)
            {
                writer.WriteLine(string.Join(",", row));
            }
        }
    }

    public List<List<string>> ReadCsvFile(string filePath)
    {
        var data = new List<List<string>>();

        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    var values = line.Split(',').ToList();
                    data.Add(values);
                }
            }
        }

        return data;
    }

    
}



