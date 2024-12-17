
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
    private const string TempFileB = "..\\B.xlsx";
    private const string TempFileC = "..\\C.xlsx";
    private const string NameA = "Файл A";

    public TableWindow()
    {
        InitializeComponent();
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
        if(lblDelayValue != null)
            lblDelayValue.Content = $"{_delayMilliseconds} мс";
    }

    private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            _filePath = dlg.FileName;
            txtLog.AppendText($"Файл выбран: {_filePath}\n");
        }
    }

    private void BtnLoadTable_Click(object sender, RoutedEventArgs e)
    {
        SortingTable table = new();
        //DisplayChunk(table.ReadCsvFile(_filePath), NameA);
        
        /*
        if (string.IsNullOrEmpty(_filePath))
        {
            MessageBox.Show("Выберите файл перед загрузкой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        SortingTable sortingTable = new();
        if (!int.TryParse(txtChunkSize.Text, out _chunkSize) || _chunkSize <= 0)
        {
            MessageBox.Show("Введите корректный размер чанка.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        //LoadChunk(0);
        DisplayChunk(sortingTable.ReadExcelFile(_filePath), NameA);

        _totalLines = GetTotalLines();
        */
    }

    private void BtnPrevChunk_Click(object sender, RoutedEventArgs e)
    {
        if (_currentChunkStart - _chunkSize < 0)
            return;

        LoadChunk(_currentChunkStart - _chunkSize);
        DisplayChunk();
    }

    private void BtnNextChunk_Click(object sender, RoutedEventArgs e)
    {
        LoadChunk(_currentChunkStart + _chunkSize);
        DisplayChunk();
    }

    private async void BtnSortNow_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtColumnIndex.Text, out _columnToSortBy) || _columnToSortBy < 1)
        {
            MessageBox.Show("Введите корректный номер колонки.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        await Sort(visualize: false);
    }

    private async void BtnSortVisualize_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(txtColumnIndex.Text, out _columnToSortBy) || _columnToSortBy < 1)
        {
            MessageBox.Show("Введите корректный номер колонки.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        await Sort(visualize: true);
    }
    
    private void LoadChunk(int start)
    {
        _currentChunkStart = start;
        _currentChunk.Clear();

        using (var reader = new StreamReader(_filePath))
        {
            for (int i = 0; i < start + _chunkSize && !reader.EndOfStream; i++)
            {
                string line = reader.ReadLine();
                if (i >= start)
                    _currentChunk.Add(line.Split(','));
            }
        }
    }

    private List<string[]> ReadSpecificChunk(int start, int size)
    {
        List<string[]> chunk = new List<string[]>();

        using (var reader = new StreamReader(_filePath))
        {
            for (int i = 0; i < start; i++)
                reader.ReadLine();

            for (int i = 0; i < size && !reader.EndOfStream; i++)
            {
                string line = reader.ReadLine();
                chunk.Add(line.Split(','));
            }
        }

        return chunk;
    }

    private void SaveChunk(int start)
    {
        SaveChunk(start, _currentChunk, _chunkSize);
    }

    private void SaveChunk(int start, List<string[]> chunk, int? chunkSize = null)
    {
        string tempFilePath = Path.GetTempFileName();

        try
        {
            using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
            using (var tempStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, 1024))
            using (var tempWriter = new StreamWriter(tempStream, Encoding.UTF8, 1024))
            {
                // before chunk -> temp file
                for (int i = 0; i < start; i++)
                {
                    string? line = reader.ReadLine();
                    if (line is not null)
                    {
                        tempWriter.WriteLine(line);
                    }
                }

                // chunk -> temp file
                foreach (var line in chunk)
                {
                    tempWriter.WriteLine(string.Join(",", line));
                }

                // skip chunk
                chunkSize ??= chunk.Count;
                    
                for (int i = 0; i < chunkSize && !reader.EndOfStream; i++)
                {
                    reader.ReadLine();
                }

                // after chunk -> temp file
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line is not null)
                    {
                        tempWriter.WriteLine(line);
                    }
                }

                tempWriter.Flush();
            }

            File.Delete(_filePath);
            File.Move(tempFilePath, _filePath);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    private void DisplayChunk()
    {
        DisplayChunk(_currentChunk);
    }

    private void DisplayChunk(List<string[]> chunkRows)
    {
        dataTable.Columns.Clear();
        dataTable.ItemsSource = null;

        int columnCount = chunkRows.Any() ? chunkRows[0].Length : 0;

        for (int i = 0; i < columnCount; i++)
        {
            dataTable.Columns.Add(new DataGridTextColumn
            {
                Header = $"Колонка {i + 1}",
                Binding = new System.Windows.Data.Binding($"[{i}]")
            });
        }

        dataTable.ItemsSource = chunkRows.ToList();

    }
    
    private void DisplayChunk(List<string[]> chunkRows, string chunkName)
    {
        dataTable.Columns.Clear();
        dataTable.ItemsSource = null;

        if (chunkRows == null || chunkRows.Count == 0)
        {
            MessageBox.Show($"Чанк '{chunkName}' пуст или не содержит данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Получаем количество колонок из первой строки (если она не null)
        int columnCount = chunkRows[0]?.Length ?? 0;


        for (int i = 0; i < columnCount; i++)
        {
            dataTable.Columns.Add(new DataGridTextColumn
            {
                Header = $"Колонка {i + 1}",
                Binding = new System.Windows.Data.Binding($"[{i}]") 
            });
        }

        // Преобразуем List<string[]> в формат, подходящий для DataGrid
        dataTable.ItemsSource = chunkRows.Select(row => new RowWrapper(row));

    }


// Вспомогательный класс для отображения массива строк в DataGrid
    public class RowWrapper
    {
        public string[] RowData { get; set; }


        public RowWrapper(string[] rowData)
        {
            RowData = rowData;
        }

        // Индексатор для доступа к элементам массива по индексу
        public string this[int index]
        {
            get
            {
                if (index >= 0 && index < RowData.Length) {
                    return RowData[index];
                }
                return null; // Or throw an exception, or return an empty string, depending on your needs
            }

            //set
            //{
            //    if (index >= 0 && index < RowData.Length) {
            //        RowData[index] = value;
            //    } //  else throw exception...
            //}
        }
    }


    private void HighlightComparison(int rowIndex1, int rowIndex2, string data1, string data2)
    {
        HighlightComparison(rowIndex1, rowIndex2, data1, data2, _currentChunk);
    }

    private void HighlightComparison(int rowIndex1, int rowIndex2, string data1, string data2, List<string[]> chunk)
    {
        _rowsToHighlight.Clear();
        _rowsToHighlight.Add(rowIndex1);
        _rowsToHighlight.Add(rowIndex2);

        DisplayChunk(chunk);

        txtLog.AppendText($"Сравниваем строки {data1} {data2}\n");
    }

    private int GetTotalLines()
    {
        int lineCount = 0;
        using (var reader = new StreamReader(_filePath))
        {
            while (reader.ReadLine() != null)
            {
                lineCount++;
            }
        }

        return lineCount;
    }

    private async Task Sort(bool visualize)
    {
        // _columnToSortBy is 1-based
        int sortColumnIndex = _columnToSortBy - 1;
        SortingTable sortingTable = new SortingTable();
        switch (cmbSortMethod.SelectedIndex)
        {
            case 0:
                await sortingTable.StraightMergeSortCsv(_filePath, _filePath, sortColumnIndex);
                break;

            case 1:
                await sortingTable.NaturalMergeSortCsv(_filePath, _filePath, sortColumnIndex, new CancellationToken(),dataTable);
                break;

            case 2:
                await MultiWayMergeSort(sortColumnIndex, visualize);
                return;

            default:
                MessageBox.Show("Метод сортировки не реализован.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
        }

        LoadChunk(0);
        //DisplayChunk(sortingTable.ReadCsvFile(_filePath), NameA);
    }
    
    

    private async Task StraightMergeSort(int sortColumnIndex, bool visualize)
    {
        int totalItems = _totalLines;
        int numChunks = (int)Math.Ceiling((double)totalItems / _chunkSize);

        for (int i = 0; i < numChunks; i++)
        {
            LoadChunk(i * _chunkSize);

            for (int j = 0; j < _currentChunk.Count - 1; j++)
            {
                for (int k = 0; k < _currentChunk.Count - j - 1; k++)
                {
                    SaveChunk(i * _chunkSize);
                    LoadChunk(i * _chunkSize);

                    if (visualize)
                    {
                        HighlightComparison(k, k + 1,
                            _currentChunk[k][sortColumnIndex],
                            _currentChunk[k + 1][sortColumnIndex]);

                        await Task.Delay(_delayMilliseconds);
                    }

                    if (string.Compare(_currentChunk[k][sortColumnIndex], _currentChunk[k + 1][sortColumnIndex]) > 0)
                    {
                        (_currentChunk[k], _currentChunk[k + 1]) = (_currentChunk[k + 1], _currentChunk[k]);

                        if (visualize)
                            txtLog.AppendText($"Поменяли местами строки {k} и {k + 1}\n");
                    }
                }
            }

            SaveChunk(i * _chunkSize);
        }

        await StraightMergeChunks(sortColumnIndex, numChunks, visualize);
    }


    private async Task StraightMergeChunks(int sortColumnIndex, int numChunks, bool visualize)
    {
        for (int mergeSize = _chunkSize; mergeSize < numChunks * _chunkSize; mergeSize *= 2)
        {
            for (int start = 0; start < _totalLines; start += 2 * mergeSize)
            {
                List<string[]> chunk1 = ReadSpecificChunk(start, mergeSize);
                List<string[]> chunk2 = ReadSpecificChunk(start + mergeSize, mergeSize);

                List<string[]> mergedChunk = new List<string[]>();
                int i = 0, j = 0;

                while (i < chunk1.Count && j < chunk2.Count)
                {
                    if (visualize)
                    {
                        var bothChunks = new List<string[]>(chunk1);
                        bothChunks.AddRange(chunk2);

                        HighlightComparison(i, j,
                            chunk1[i][sortColumnIndex],
                            chunk2[j][sortColumnIndex],
                            bothChunks);

                        await Task.Delay(_delayMilliseconds);
                    }

                    if (string.Compare(chunk1[i][sortColumnIndex], chunk2[j][sortColumnIndex]) <= 0)
                    {
                        mergedChunk.Add(chunk1[i]);
                        i++;
                    }
                    else
                    {
                        mergedChunk.Add(chunk2[j]);
                        j++;
                    }
                }

                while (i < chunk1.Count)
                {
                    mergedChunk.Add(chunk1[i]);
                    i++;
                }

                while (j < chunk2.Count)
                {
                    mergedChunk.Add(chunk2[j]);
                    j++;
                }

                SaveChunk(start, mergedChunk);
            }
        }

        LoadChunk(0);
        DisplayChunk();
    }
    
    
    
    /*private async Task NaturalMergeSort(int sortColumnIndex, bool visualize)
    {
        txtLog.AppendText("Начало естественного слияния\n");

        var runs = NaturalDivideIntoRuns(sortColumnIndex, visualize);
        
        while (runs.Count > 1)
        {
            var mergedRuns = new List<List<string[]>>();

            for (int i = 0; i < runs.Count; i += 2)
            {
                if (i + 1 < runs.Count)
                {
                    var merged = await NaturalMergeTwoRuns(runs[i], runs[i + 1], sortColumnIndex, visualize);
                    mergedRuns.Add(merged);
                }
                else
                {
                    mergedRuns.Add(runs[i]);
                }
            }

            runs = mergedRuns;

            if (visualize)
                txtLog.AppendText($"Слияние: осталось {runs.Count} серий\n");
        }
        
        
        if (runs.Count == 1)
        {
            SaveChunk(0, runs[0]);
        }

        if (!visualize)
            return;

        txtLog.AppendText("Естественное слияние завершено\n");
        LoadChunk(0);
        DisplayChunk();
    }
*/
    private List<List<string[]>> NaturalDivideIntoRuns(int sortColumnIndex, bool visualize)
    {
        var runs = new List<List<string[]>>();
        List<string[]> currentRun = new();

        using (var reader = new StreamReader(_filePath))
        {
            string[]? prevRow = null;
            while (!reader.EndOfStream)
            {
                var currentRow = reader.ReadLine().Split(',');

                if (prevRow == null || string.Compare(prevRow[sortColumnIndex], currentRow[sortColumnIndex]) <= 0)
                {
                    currentRun.Add(currentRow);
                }
                else
                {
                    runs.Add(new List<string[]>(currentRun));
                    currentRun.Clear();
                    currentRun.Add(currentRow);
                }

                prevRow = currentRow;
            }

            if (currentRun.Count > 0)
            {
                runs.Add(currentRun);
            }
        }

        if (visualize)
            txtLog.AppendText($"Разбивка на {runs.Count} серий завершена\n");
        return runs;
    }

    private async Task<List<string[]>> NaturalMergeTwoRuns(List<string[]> run1, List<string[]> run2,
        int sortColumnIndex, bool visualize)
    {
        int i = 0, j = 0;
        List<string[]> mergedRun = new();

        while (i < run1.Count && j < run2.Count)
        {
            if (visualize)
            {
                HighlightComparison(i, j, run1[i][sortColumnIndex], run2[j][sortColumnIndex], mergedRun);
                await Task.Delay(_delayMilliseconds);
            }

            if (string.Compare(run1[i][sortColumnIndex], run2[j][sortColumnIndex]) <= 0)
            {
                mergedRun.Add(run1[i]);
                i++;
            }
            else
            {
                mergedRun.Add(run2[j]);
                j++;
            }
        }

        while (i < run1.Count)
        {
            mergedRun.Add(run1[i]);
            i++;
        }

        while (j < run2.Count)
        {
            mergedRun.Add(run2[j]);
            j++;
        }

        return mergedRun;
    }



    private async Task MultiWayMergeSort(int sortColumnIndex, bool visualize)
    {
        int totalItems = _totalLines;
        int numChunks = (int)Math.Ceiling((double)totalItems / _chunkSize);

        await MultiWayCreateChunks(sortColumnIndex, visualize, numChunks);

        await MultiWayMergeChunks(sortColumnIndex, numChunks, visualize);
    }

    private async Task MultiWayCreateChunks(int sortColumnIndex, bool visualize, int numChunks)
    {
        for (int i = 0; i < numChunks; i++)
        {
            LoadChunk(i * _chunkSize);

            for (int j = 0; j < _currentChunk.Count - 1; j++)
            {
                for (int k = 0; k < _currentChunk.Count - j - 1; k++)
                {
                    if (visualize)
                    {
                        HighlightComparison(k, k + 1,
                            _currentChunk[k][sortColumnIndex],
                            _currentChunk[k + 1][sortColumnIndex]);

                        await Task.Delay(_delayMilliseconds);
                    }

                    if (string.Compare(_currentChunk[k][sortColumnIndex], _currentChunk[k + 1][sortColumnIndex]) > 0)
                    {
                        (_currentChunk[k], _currentChunk[k + 1]) = (_currentChunk[k + 1], _currentChunk[k]);

                        if (visualize)
                            txtLog.AppendText($"Поменяли местами строки {k} и {k + 1}\n");
                    }
                }
            }

            SaveChunk(i * _chunkSize);
        }
    }

    private async Task MultiWayMergeChunks(int sortColumnIndex, int numChunks, bool visualize)
    {
        var priorityQueue = new SortedSet<(string[] Row, int ChunkIndex, int RowIndex)>(
            Comparer<(string[] Row, int ChunkIndex, int RowIndex)>.Create((x, y) =>
                string.Compare(x.Row[sortColumnIndex], y.Row[sortColumnIndex])));

        var chunkReaders = new List<StreamReader>();
        var chunkFiles = new List<string>();

        await MultiWayCreateTempFiles(numChunks, chunkFiles, chunkReaders);

        for (int i = 0; i < numChunks; i++)
        {
            var row = chunkReaders[i].ReadLine().Split(',');
            priorityQueue.Add((row, i, 0)); // 1st row of each chunk
        }

        var mergedChunk = new List<string[]>();

        // MERGE
        while (priorityQueue.Count > 0)
        {
            var (minRow, chunkIndex, rowIndex) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            mergedChunk.Add(minRow);

            if (visualize)
            {
                var nextMin = priorityQueue.Min;
                if(nextMin.Row is not null)
                    HighlightComparison(rowIndex, nextMin.RowIndex, minRow[sortColumnIndex], nextMin.Row[sortColumnIndex]);
                await Task.Delay(_delayMilliseconds);
            }

            var nextRow = chunkReaders[chunkIndex].ReadLine();
            if (nextRow != null)
            {
                var nextRowArray = nextRow.Split(',');
                priorityQueue.Add((nextRowArray, chunkIndex, rowIndex + 1));
            }
        }

        foreach (var reader in chunkReaders)
        {
            reader.Close();
        }

        foreach (var file in chunkFiles)
        {
            File.Delete(file);
        }

        SaveChunk(0, mergedChunk);

        LoadChunk(0);
        DisplayChunk();
    }

    private async Task MultiWayCreateTempFiles(int numChunks, List<string> chunkFiles, List<StreamReader> chunkReaders)
    {
        for (int i = 0; i < numChunks; i++)
        {
            string tempFilePath = Path.GetTempFileName();
            chunkFiles.Add(tempFilePath);

            await using (var writer = new StreamWriter(tempFilePath))
            {
                var chunk = ReadSpecificChunk(i * _chunkSize, _chunkSize);
                foreach (var row in chunk)
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }

            chunkReaders.Add(new StreamReader(tempFilePath));
        }
    }
}

public class SortingTable
{
   // private int _numberSorting;

    /*public List<string[]> ReadCsvFile(string filePath)
    {
        var records = new List<string[]>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    records.Add(line.Split(','));
                }
            }
        }

        return records;
    }*/

  /*  private void WriteCsvFile(string filePath, List<string[]> records)
    {
        using (var writer = new StreamWriter(filePath, false))
        {
            foreach (string[] record in records)
            {
                writer.WriteLine(string.Join(",", record));
            }
        }
    }
*/
   /* private bool IsSorted(string[] row1, string[] row2, int columnIndex)
    {
        if (row1 == null || row2 == null || columnIndex < 0 || columnIndex >= row1.Length || columnIndex >= row2.Length)
        {
            return false;
        }

        return string.Compare(row1[columnIndex], row2[columnIndex], StringComparison.InvariantCultureIgnoreCase) <= 0;
    }
*/
    public void NaturalSplitCsvFile(string inputFile, string tempFileB, string tempFileC, int columnIndex)
    {
        var inputRecords = ReadCsvFile(inputFile);
        var outputB = new List<string[]>();
        var outputC = new List<string[]>();

        bool writeToB = true;

        for (int i = 0; i < inputRecords.Count; i++)
        {
            if (writeToB)
                outputB.Add(inputRecords[i]);
            else
                outputC.Add(inputRecords[i]);

            if (i + 1 < inputRecords.Count && !IsSorted(inputRecords[i], inputRecords[i + 1], columnIndex))
            {
                writeToB = !writeToB;
            }
        }

        WriteCsvFile(tempFileB, outputB);
        WriteCsvFile(tempFileC, outputC);
    }

    public void NaturalMergeCsvFiles(string outputFile, string tempFileB, string tempFileC, int columnIndex)
    {
        var linesB = ReadCsvFile(tempFileB);
        var linesC = ReadCsvFile(tempFileC);

        var output = new List<string[]>();
        int i = 0, j = 0;

        while (i < linesB.Count || j < linesC.Count)
        {
            while (i < linesB.Count && j < linesC.Count)
            {
                if (IsSorted(linesB[i], linesC[j], columnIndex))
                {
                    output.Add(linesB[i]);
                    i++;
                }
                else
                {
                    output.Add(linesC[j]);
                    j++;
                }

                if ((i < linesB.Count - 1 && !IsSorted(linesB[i], linesB[i + 1], columnIndex)) ||
                    (j < linesC.Count - 1 && !IsSorted(linesC[j], linesC[j + 1], columnIndex)))
                    break;
            }


            while (i < linesB.Count)
            {
                output.Add(linesB[i]);
                if (i < linesB.Count - 1 && !IsSorted(linesB[i], linesB[i + 1], columnIndex))
                {
                    i++;
                    break;
                }

                i++;
            }


            while (j < linesC.Count)
            {
                output.Add(linesC[j]);
                if (j < linesC.Count - 1 && !IsSorted(linesC[j], linesC[j + 1], columnIndex))
                {
                    j++;
                    break;
                }

                j++;
            }
        }

        WriteCsvFile(outputFile, output);
    }


    /*public async Task NaturalMergeSortCsv(string inputFile, string outputFile, int columnIndex)
    {
        string tempFileB = "TempB.csv";
        string tempFileC = "TempC.csv";


        while (true)
        {

            NaturalSplitCsvFile(inputFile, tempFileB, tempFileC, columnIndex);


            if (new FileInfo(tempFileC).Length == 0 || new FileInfo(tempFileB).Length == 0)
            {
                File.Copy(tempFileB.Length > 0 ? tempFileB : tempFileC, outputFile, true);
                break;
            }


            NaturalMergeCsvFiles(outputFile, tempFileB, tempFileC, columnIndex);


            File.Copy(outputFile, inputFile, true);

        }
    }*/


   /* private void CopyLines(List<string[]> source, List<string[]> destination, int start, int count)
    {
        for (int i = start; i < start + count && i < source.Count; i++)
        {
            destination.Add(source[i]);
        }
    }*/




    public void SplitCsvFileMultiway(string inputFile, List<string> tempFiles, int numberOfFiles)
    {
        var inputRecords = ReadCsvFile(inputFile);
        var fileOutputs = new List<List<string[]>>();

        for (int j = 0; j < numberOfFiles; j++)
            fileOutputs.Add(new List<string[]>());

        int currentFile = 0;
        int blockSize = (int)Math.Pow(2, _numberSorting);
        int i = 0;

        while (i < inputRecords.Count)
        {
            int remaining = inputRecords.Count - i;
            CopyLines(inputRecords, fileOutputs[currentFile], i, Math.Min(blockSize, remaining));
            i += blockSize;
            currentFile = (currentFile + 1) % numberOfFiles;
        }

        for (int j = 0; j < numberOfFiles; j++)
        {
            WriteCsvFile(tempFiles[j], fileOutputs[j]);
        }

        _numberSorting++;
    }


    public void MultiwayMergeCsvFiles(string outputFile, List<string> tempFiles, int columnIndex)
    {
        var readers = tempFiles.Select(file => new StreamReader(file)).ToList();
        var lines = readers.Select(reader => reader.ReadLine()).ToArray();
        var output = new List<string[]>();

        while (lines.Any(line => line != null))
        {
            int minIndex = -1;
            string[] minValue = null; // Changed to string[]

            // Find minimum value
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null)
                {
                    string[] currentColumns = lines[i].Split(',');
                    if (minValue == null || string.Compare(currentColumns[columnIndex], minValue[columnIndex],
                            StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        minValue = currentColumns;
                        minIndex = i;
                    }
                }
            }

            // Add minimum row to output
            output.Add(minValue); // Add the entire row (string[])
            lines[minIndex] = readers[minIndex].ReadLine();
        }

        foreach (var reader in readers) reader.Close();

        WriteCsvFile(outputFile, output);
    }




    public void StraightSplitCsvFile(string inputFile, string tempFileB, string tempFileC)
    {
        var inputLines = ReadCsvFile(inputFile); // Changed to ReadCsvFile and store as string[][]
        var outputB = new List<string[]>();
        var outputC = new List<string[]>();

        int blockSize = (int)Math.Pow(2, _numberSorting);
        int i = 0;

        while (i < inputLines.Count)
        {
            int remaining = inputLines.Count - i;
            if ((i / blockSize) % 2 == 0)
            {
                CopyLines(inputLines, outputB, i, Math.Min(blockSize, remaining));

            }
            else
            {
                CopyLines(inputLines, outputC, i, Math.Min(blockSize, remaining));
            }

            i += blockSize;
        }

        WriteCsvFile(tempFileB, outputB);
        WriteCsvFile(tempFileC, outputC);
        _numberSorting++;
    }
/// <summary>
/// ///////////////
/// </summary>
private int _numberSorting;
    public List<RowWrapper> DisplayedData { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private List<string[]> ReadCsvFile(string filePath)
    {
        var records = new List<string[]>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    records.Add(line.Split(','));
                }
            }
        }
        return records;
    }

    private void WriteCsvFile(string filePath, List<string[]> records)
    {
        using var writer = new StreamWriter(filePath, false);
        foreach (string[] record in records)
        {
            writer.WriteLine(string.Join(",", record));
        }
    }

    private bool IsSorted(string[] row1, string[] row2, int columnIndex)
    {
        if (row1 == null || row2 == null || columnIndex < 0 ||
            columnIndex >= row1.Length || columnIndex >= row2.Length)
        {
            return false;
        }
        return string.Compare(row1[columnIndex], row2[columnIndex], StringComparison.InvariantCultureIgnoreCase) <= 0;
    }

    private async Task UpdateDataGridAsync(List<string[]> chunkRows, string chunkName, CancellationToken cancellationToken)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (dataTable is null)
                return;
            dataTable.Columns.Clear();
            dataTable.ItemsSource = null;


            if (chunkRows == null || chunkRows.Count == 0)
            {
                MessageBox.Show($"Чанк '{chunkName}' пуст или не содержит данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            int columnCount = chunkRows[0]?.Length ?? 0;


            for (int i = 0; i < columnCount; i++)
            {
                dataTable.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Колонка {i + 1}",
                    Binding = new Binding($"[{i}]")
                });
            }


            DisplayedData = chunkRows.Select(row => new RowWrapper(row)).ToList();
            dataTable.ItemsSource = DisplayedData;


        });
        await Task.Delay(200, cancellationToken);
    }




    private void CopyLines(List<string[]> source, List<string[]> destination, int start, int count)
    {
        for (int i = start; i < start + count && i < source.Count; i++)
        {
            destination.Add(source[i]);
        }
    }
    // Объявляем dataTable как поле
    private DataGrid dataTable;


    public async Task NaturalMergeSortCsv(string inputFile, string outputFile, int columnIndex,
        CancellationToken cancellationToken, DataGrid dataGrid)
    {
        string tempFileB = "TempB.csv";
        string tempFileC = "TempC.csv";

        dataTable = dataGrid; // Инициализируем поле dataGrid


        while (!cancellationToken.IsCancellationRequested)
        {

            NaturalSplitCsvFile(inputFile, tempFileB, tempFileC, columnIndex);

            await UpdateDataGridAsync(ReadCsvFile(inputFile), "Входные данные", cancellationToken);
            await UpdateDataGridAsync(ReadCsvFile(tempFileB), "Временный файл B", cancellationToken);
            await UpdateDataGridAsync(ReadCsvFile(tempFileC), "Временный файл C", cancellationToken);


            if (new FileInfo(tempFileC).Length == 0 || new FileInfo(tempFileB).Length == 0)
            {
                File.Copy(tempFileB.Length > 0 ? tempFileB : tempFileC, outputFile, true);
                await UpdateDataGridAsync(ReadCsvFile(outputFile), "Выходные данные", cancellationToken);
                break;
            }


            NaturalMergeCsvFiles(outputFile, tempFileB, tempFileC, columnIndex);
            await UpdateDataGridAsync(ReadCsvFile(outputFile), "Выходные данные", cancellationToken);


            File.Copy(outputFile, inputFile, true);
        }
    }


    public void StraightMergeCsvFiles(string outputFile, string tempFileB, string tempFileC, int columnIndex)
    {
        var linesB = ReadCsvFile(tempFileB);
        var linesC = ReadCsvFile(tempFileC);

        var output = new List<string[]>();
        int i = 0, j = 0;

        while (i < linesB.Count && j < linesC.Count)
        {
            if (string.Compare(linesB[i][columnIndex], linesC[j][columnIndex],
                    StringComparison.InvariantCultureIgnoreCase) <= 0) // Access elements directly
            {
                output.Add(linesB[i]);
                i++;
            }
            else
            {
                output.Add(linesC[j]);
                j++;
            }
        }


        while (i < linesB.Count) output.Add(linesB[i++]);
        while (j < linesC.Count) output.Add(linesC[j++]);

        WriteCsvFile(outputFile, output);
    }

    public async Task StraightMergeSortCsv(string inputFile, string outputFile, int columnIndex)
    {
        string tempFileB = "TempB.csv";
        string tempFileC = "TempC.csv";

        while (true)
        {
            // Разделение
            StraightSplitCsvFile(inputFile, tempFileB, tempFileC);

            // Если один из файлов пуст, значит сортировка завершена
            if (new FileInfo(tempFileC).Length == 0 || new FileInfo(tempFileB).Length == 0)
            {
                File.Copy(tempFileB.Length > 0 ? tempFileB : tempFileC, outputFile, true);
                break;
            }

            // Слияние
            StraightMergeCsvFiles(outputFile, tempFileB, tempFileC, columnIndex);

            // Подготовка к следующей итерации
            File.Copy(outputFile, inputFile, true);
            ;
        }
    }
    public class RowWrapper
    {
        public string[] RowData { get; }

        public RowWrapper(string[] rowData) => RowData = rowData;

        public string this[int index] => index >= 0 && index < RowData.Length ? RowData[index] : null;
    }
}



