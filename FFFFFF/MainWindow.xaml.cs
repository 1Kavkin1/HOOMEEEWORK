using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FFFFFF;

public partial class MainWindow : Window
{
    private readonly Calculator _calculator = new();
    private readonly Random _random = new();

    public MainWindow()
    {
        InitializeComponent();
        Resize(GridA, 3, 3, false);
        Resize(GridB, 3, 3, false);
        Resize(GridC, 3, 3, true);
    }

    private void CalculatorDigit_Click(object sender, RoutedEventArgs e)
    {
        _calculator.InputDigit(((Button)sender).Content.ToString()!);
        UpdateCalculatorDisplay();
    }

    private void CalculatorDecimal_Click(object sender, RoutedEventArgs e)
    {
        _calculator.InputDecimalSeparator();
        UpdateCalculatorDisplay();
    }

    private void CalculatorOperator_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _calculator.SetOperator(((Button)sender).Content.ToString()!);
            UpdateCalculatorDisplay();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void CalculatorEquals_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _calculator.CalculateResult();
            UpdateCalculatorDisplay();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void CalculatorClear_Click(object sender, RoutedEventArgs e)
    {
        _calculator.Clear();
        UpdateCalculatorDisplay();
    }

    private void UpdateCalculatorDisplay() => CalculatorDisplay.Text = _calculator.Display;

    private void ResizeMatrix_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var (grid, rowsBox, colsBox) = GetMatrixControls(sender);
            Resize(grid, ReadDimension(rowsBox.Text, "рядків"), ReadDimension(colsBox.Text, "стовпців"), false);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void RandomMatrix_Click(object sender, RoutedEventArgs e)
    {
        var (grid, _, _) = GetMatrixControls(sender);
        for (int i = 0; i < grid.Children.Count; i++)
            ((TextBox)grid.Children[i]).Text = _random.Next(-9, 10).ToString(CultureInfo.InvariantCulture);
    }

    private void IdentityMatrix_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var (grid, rowsBox, colsBox) = GetMatrixControls(sender);
            int size = ReadDimension(rowsBox.Text, "розміру");
            colsBox.Text = size.ToString(CultureInfo.InvariantCulture);
            Resize(grid, size, size, false);

            for (int row = 0; row < size; row++)
                for (int column = 0; column < size; column++)
                    ((TextBox)grid.Children[row * size + column]).Text = row == column ? "1" : "0";
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ClearMatrix_Click(object sender, RoutedEventArgs e)
    {
        var (grid, _, _) = GetMatrixControls(sender);
        foreach (TextBox textBox in grid.Children)
            textBox.Text = "0";
    }

    private void AddMatrices_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            double[,] a = Read(GridA, "A");
            double[,] b = Read(GridB, "B");
            double[,] result = MatrixMath.Add(a, b);
            Resize(GridC, result.GetLength(0), result.GetLength(1), true);
            Fill(GridC, result);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    // Resize зберігає старі значення за координатами, а нові комірки заповнює нулями.
    private static void Resize(UniformGrid grid, int rows, int columns, bool readOnly)
    {
        var old = new Dictionary<(int Row, int Column), string>();
        if (grid.Columns > 0)
        {
            for (int index = 0; index < grid.Children.Count; index++)
                old[(index / grid.Columns, index % grid.Columns)] = ((TextBox)grid.Children[index]).Text;
        }

        grid.Children.Clear();
        grid.Rows = rows;
        grid.Columns = columns;

        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var textBox = new TextBox
                {
                    Width = 60,
                    Height = 28,
                    Margin = new Thickness(2),
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsReadOnly = readOnly,
                    Text = old.TryGetValue((row, column), out string? value) ? value : "0"
                };
                Brush normal = readOnly ? Brushes.WhiteSmoke : Brushes.White;
                textBox.Background = normal;
                textBox.TextChanged += (_, _) => textBox.Background = TryParse(textBox.Text, out _) ? normal : Brushes.MistyRose;
                grid.Children.Add(textBox);
            }
    }

    private static bool TryParse(string text, out double value) =>
        double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        && !double.IsNaN(value) && !double.IsInfinity(value);

    // UniformGrid зберігає елементи послідовно: індекс комірки = рядок * кількість стовпців + стовпець.
    private static double[,] Read(UniformGrid grid, string name)
    {
        var matrix = new double[grid.Rows, grid.Columns];
        for (int row = 0; row < grid.Rows; row++)
            for (int column = 0; column < grid.Columns; column++)
            {
                TextBox textBox = (TextBox)grid.Children[row * grid.Columns + column];
                if (!TryParse(textBox.Text, out matrix[row, column]))
                    throw new InvalidOperationException($"Помилка у матриці {name}: некоректне число.");
            }

        return matrix;
    }

    private static void Fill(UniformGrid grid, double[,] matrix)
    {
        for (int row = 0; row < matrix.GetLength(0); row++)
            for (int column = 0; column < matrix.GetLength(1); column++)
                ((TextBox)grid.Children[row * grid.Columns + column]).Text = Format(matrix[row, column]);
    }

    private static string Format(double value) => Math.Round(value, 6).ToString("G7", CultureInfo.InvariantCulture);

    private (UniformGrid Grid, TextBox Rows, TextBox Columns) GetMatrixControls(object sender)
    {
        string tag = (string)((Button)sender).Tag;
        return tag == "A" ? (GridA, RowsA, ColsA) : (GridB, RowsB, ColsB);
    }

    private static int ReadDimension(string text, string name)
    {
        if (!int.TryParse(text, out int value) || value < 1 || value > 20)
            throw new InvalidOperationException($"Кількість {name} повинна бути цілим числом від 1 до 20.");
        return value;
    }

    private static void ShowError(string message) => MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
}