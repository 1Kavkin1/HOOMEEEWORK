namespace FFFFFF;

/// <summary>
/// Містить математичні операції над матрицями та не залежить від WPF.
/// </summary>
public static class MatrixMath
{
    public static double[,] Add(double[,] left, double[,] right)
    {
        if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
            throw new InvalidOperationException("Матриці A та B повинні мати однаковий розмір.");

        int rows = left.GetLength(0);
        int columns = left.GetLength(1);
        var result = new double[rows, columns];

        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                result[row, column] = left[row, column] + right[row, column];

        return result;
    }
}
