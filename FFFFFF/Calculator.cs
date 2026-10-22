namespace FFFFFF;

/// <summary>
/// Зберігає стан калькулятора окремо від елементів WPF.
/// Це дозволяє тестувати арифметику незалежно від інтерфейсу.
/// </summary>
public sealed class Calculator
{
    private double? _storedValue;
    private string? _pendingOperator;
    private bool _startNewNumber = true;

    public string Display { get; private set; } = "0";

    public void InputDigit(string digit)
    {
        if (_startNewNumber || Display == "0")
            Display = digit;
        else
            Display += digit;

        _startNewNumber = false;
    }

    public void InputDecimalSeparator()
    {
        if (_startNewNumber)
        {
            Display = "0.";
            _startNewNumber = false;
        }
        else if (!Display.Contains('.'))
        {
            Display += ".";
        }
    }

    public void SetOperator(string operation)
    {
        if (!double.TryParse(Display, System.Globalization.CultureInfo.InvariantCulture, out var current))
            throw new InvalidOperationException("Некоректне число.");

        if (_storedValue.HasValue && _pendingOperator is not null && !_startNewNumber)
            _storedValue = Calculate(_storedValue.Value, current, _pendingOperator);
        else
            _storedValue = current;

        _pendingOperator = operation;
        Display = Format(_storedValue.Value);
        _startNewNumber = true;
    }

    public void CalculateResult()
    {
        if (!_storedValue.HasValue || _pendingOperator is null)
            return;

        if (!double.TryParse(Display, System.Globalization.CultureInfo.InvariantCulture, out var current))
            throw new InvalidOperationException("Некоректне число.");

        _storedValue = Calculate(_storedValue.Value, current, _pendingOperator);
        Display = Format(_storedValue.Value);
        _pendingOperator = null;
        _startNewNumber = true;
    }

    public void Clear()
    {
        _storedValue = null;
        _pendingOperator = null;
        Display = "0";
        _startNewNumber = true;
    }

    private static double Calculate(double left, double right, string operation) => operation switch
    {
        "+" => left + right,
        "-" => left - right,
        "*" => left * right,
        "/" when right != 0 => left / right,
        "/" => throw new DivideByZeroException("Ділення на нуль неможливе."),
        _ => throw new InvalidOperationException($"Невідома операція: {operation}")
    };

    private static string Format(double value) => value.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
}
