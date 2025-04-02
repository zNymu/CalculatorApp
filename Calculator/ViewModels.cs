using System;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CalculatorApp.Commands;
using CalculatorApp.Models;

namespace CalculatorApp.ViewModels
{
    public class CalculatorViewModel : ViewModelBase
    {
        #region Properties

        private string _resultDisplay = "0";
        public string ResultDisplay
        {
            get => _resultDisplay;
            set => SetProperty(ref _resultDisplay, value);
        }

        private string _equationDisplay = "";
        public string EquationDisplay
        {
            get => _equationDisplay;
            set => SetProperty(ref _equationDisplay, value);
        }

        private string _memoryDisplay = "";
        public string MemoryDisplay
        {
            get => _memoryDisplay;
            set => SetProperty(ref _memoryDisplay, value);
        }

        private bool _isStandardMode = true;
        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
                if (SetProperty(ref _isStandardMode, value) && value)
                {
                    IsProgrammerMode = !value;
                    ResetCalculator();
                    HasDecimal = false;

                    _settings.IsStandardMode = value;
                    _settings.Save();
                }
            }
        }

        private bool _isProgrammerMode;
        public bool IsProgrammerMode
        {
            get => _isProgrammerMode;
            set
            {
                if (SetProperty(ref _isProgrammerMode, value) && value)
                {
                    IsStandardMode = !value;
                    ResetCalculator();
                    UpdateCurrentBase();
                    IsHexMode = CurrentBase == 16;
                    UpdateHexButtonsState();

                    _settings.IsStandardMode = !value;
                    _settings.Save();
                }
            }
        }

        private bool _useOperationOrder;
        public bool UseOperationOrder
        {
            get => _useOperationOrder;
            set
            {
                if (SetProperty(ref _useOperationOrder, value))
                {
                    _settings.UseOperationOrder = value;
                    _settings.Save();
                }
            }
        }


        private bool _isDecimalBase = true;
        public bool IsDecimalBase
        {
            get => _isDecimalBase;
            set
            {
                if (SetProperty(ref _isDecimalBase, value) && value)
                {
                    CurrentBase = 10;
                    UpdateDisplay();
                    IsHexMode = false;
                    UpdateHexButtonsState();

                    _settings.CurrentBase = 10;
                    _settings.Save();
                }
            }
        }

        private bool _isHexBase;
        public bool IsHexBase
        {
            get => _isHexBase;
            set
            {
                if (SetProperty(ref _isHexBase, value) && value)
                {
                    CurrentBase = 16;
                    UpdateDisplay();
                    IsHexMode = true;
                    UpdateHexButtonsState();

                    _settings.CurrentBase = 16;
                    _settings.Save();
                }
            }
        }

        private bool _isOctalBase;
        public bool IsOctalBase
        {
            get => _isOctalBase;
            set
            {
                if (SetProperty(ref _isOctalBase, value) && value)
                {
                    CurrentBase = 8;
                    UpdateDisplay();
                    IsHexMode = false;
                    UpdateHexButtonsState();

                    _settings.CurrentBase = 8;
                    _settings.Save();
                }
            }
        }

        private bool _isBinaryBase;
        public bool IsBinaryBase
        {
            get => _isBinaryBase;
            set
            {
                if (SetProperty(ref _isBinaryBase, value) && value)
                {
                    CurrentBase = 2;
                    UpdateDisplay();
                    IsHexMode = false;
                    UpdateHexButtonsState();

                    _settings.CurrentBase = 2;
                    _settings.Save();
                }
            }
        }

        private bool[] _hexButtonsEnabled = new bool[6];
        public bool[] HexButtonsEnabled
        {
            get => _hexButtonsEnabled;
            set => SetProperty(ref _hexButtonsEnabled, value);
        }

        private bool[] _numberButtonsEnabled = new bool[10];
        public bool[] NumberButtonsEnabled
        {
            get => _numberButtonsEnabled;
            set => SetProperty(ref _numberButtonsEnabled, value);
        }

        public bool IsDigitGroupingEnabled { get; set; } = true;

        #endregion

        #region Private Fields
        private double _currentValue = 0;
        private double _storedValue = 0;
        private string _currentOperation = "";
        private bool _isNewNumber = true;
        private bool _hasDecimal = false;
        private double _lastOperand = 0;
        private string _lastEquation = "";
        private bool _equalsPressed = false;

        private int _currentBase = 10;
        private bool _isHexMode = false;
        private long _currentIntValue = 0;
        private long _storedIntValue = 0;
        #endregion

        #region Private Properties
        private int CurrentBase
        {
            get => _currentBase;
            set
            {
                if (_currentBase != value)
                {
                    _currentBase = value;
                    UpdateDisplay();
                }
            }
        }

        private bool IsHexMode
        {
            get => _isHexMode;
            set
            {
                if (_isHexMode != value)
                {
                    _isHexMode = value;
                }
            }
        }

        private bool HasDecimal
        {
            get => _hasDecimal;
            set
            {
                _hasDecimal = value;
            }
        }
        #endregion

        #region Commands

        public ICommand NumberCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SquareCommand { get; }
        public ICommand SquareRootCommand { get; }
        public ICommand InverseCommand { get; }
        public ICommand NegateCommand { get; }
        public ICommand HexDigitCommand { get; }
        public ICommand BitwiseCommand { get; }
        public ICommand MemoryStoreCommand { get; }
        public ICommand MemoryRecallCommand { get; }
        public ICommand MemoryPlusCommand { get; }
        public ICommand MemoryMinusCommand { get; }
        public ICommand MemoryClearCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand ToggleDigitGroupingCommand { get; }

        public ICommand AboutCommand { get; }
        public ICommand KeyboardCommand { get; }

        #endregion

        private CalculatorSettings _settings;

        public CalculatorViewModel()
        {
            _settings = CalculatorSettings.Load();

            IsDigitGroupingEnabled = _settings.IsDigitGroupingEnabled;

            if (_settings.IsStandardMode)
            {
                IsStandardMode = true;
                IsProgrammerMode = false;
            }
            else
            {
                IsStandardMode = false;
                IsProgrammerMode = true;

                switch (_settings.CurrentBase)
                {
                    case 16:
                        IsHexBase = true;
                        break;
                    case 10:
                        IsDecimalBase = true;
                        break;
                    case 8:
                        IsOctalBase = true;
                        break;
                    case 2:
                        IsBinaryBase = true;
                        break;
                }
            }

            NumberCommand = new RelayCommand(param => HandleNumberInput(param.ToString()[0]));
            OperationCommand = new RelayCommand(param => HandleOperation(param.ToString()[0]));
            EqualsCommand = new RelayCommand(_ => PerformEquals());
            ClearCommand = new RelayCommand(_ => ResetCalculator());
            ClearEntryCommand = new RelayCommand(_ => ClearEntry());
            BackspaceCommand = new RelayCommand(_ => HandleBackspace());
            SquareCommand = new RelayCommand(_ => SquareValue());
            SquareRootCommand = new RelayCommand(_ => SquareRootValue());
            InverseCommand = new RelayCommand(_ => InverseValue());
            NegateCommand = new RelayCommand(_ => NegateValue());
            HexDigitCommand = new RelayCommand(param => HandleHexInput(param.ToString()[0]));
            BitwiseCommand = new RelayCommand(param => HandleBitwiseOperation(param.ToString()));
            MemoryStoreCommand = new RelayCommand(_ => MemoryStore());
            MemoryRecallCommand = new RelayCommand(_ => MemoryRecall());
            MemoryPlusCommand = new RelayCommand(_ => MemoryPlus());
            MemoryMinusCommand = new RelayCommand(_ => MemoryMinus());
            MemoryClearCommand = new RelayCommand(_ => MemoryClear());
            CopyCommand = new RelayCommand(_ => Copy());
            CutCommand = new RelayCommand(_ => Cut());
            PasteCommand = new RelayCommand(_ => Paste());
            AboutCommand = new RelayCommand(_ => ShowAboutDialog());
            ToggleDigitGroupingCommand = new RelayCommand(param => ToggleDigitGrouping((bool)param));
            KeyboardCommand = new RelayCommand(param => HandleKeyInput((Key)param));


            UpdateHexButtonsState();
        }

        private void UpdateCurrentBase()
        {
            if (IsDecimalBase) CurrentBase = 10;
            else if (IsHexBase) CurrentBase = 16;
            else if (IsOctalBase) CurrentBase = 8;
            else if (IsBinaryBase) CurrentBase = 2;
        }

        #region Command Methods

        private void HandleNumberInput(char digit)
        {
            if (IsProgrammerMode)
            {
                int digitValue = "0123456789".IndexOf(digit);
                if (digitValue >= 0 && digitValue < CurrentBase)
                {
                    if (_isNewNumber)
                    {
                        ResultDisplay = digit.ToString();
                        _isNewNumber = false;
                        if (_equalsPressed)
                        {
                            _equalsPressed = false;
                            EquationDisplay = "";
                        }
                    }
                    else
                    {
                        if (ResultDisplay == "0")
                        {
                            ResultDisplay = digit.ToString();
                        }
                        else
                        {
                            ResultDisplay += digit;
                        }
                    }
                    UpdateCurrentIntValue();
                }
            }
            else
            {
                if (_isNewNumber)
                {
                    ResultDisplay = (digit == '.') ? "0." : digit.ToString();
                    _isNewNumber = false;
                    HasDecimal = digit == '.';
                    if (_equalsPressed)
                    {
                        _equalsPressed = false;
                        EquationDisplay = "";
                    }
                }
                else
                {
                    if (digit == '.' && !HasDecimal)
                    {
                        ResultDisplay += digit;
                        HasDecimal = true;
                    }
                    else if (digit != '.')
                    {
                        if (ResultDisplay == "0")
                        {
                            ResultDisplay = digit.ToString();
                        }
                        else
                        {
                            ResultDisplay += digit;
                        }
                    }
                }
            }
        }
        private double EvaluateExpression(string expression)
        {
            try
            {
                expression = expression.Replace("×", "*").Replace("÷", "/");

                System.Data.DataTable table = new System.Data.DataTable();
                table.Columns.Add("expression", typeof(string), expression);
                System.Data.DataRow row = table.NewRow();
                table.Rows.Add(row);

                return double.Parse((string)row["expression"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error evaluating expression: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void HandleHexInput(char digit)
        {
            if (IsHexMode && _isNewNumber)
            {
                ResultDisplay = digit.ToString();
                _isNewNumber = false;
                if (_equalsPressed)
                {
                    _equalsPressed = false;
                    EquationDisplay = "";
                }
            }
            else if (IsHexMode)
            {
                if (ResultDisplay == "0")
                {
                    ResultDisplay = digit.ToString();
                }
                else
                {
                    ResultDisplay += digit;
                }
            }
            UpdateCurrentIntValue();
        }

        private long ParseCurrentDisplayValue()
        {
            if (_isNewNumber)
                return _currentIntValue;

            try
            {
                switch (CurrentBase)
                {
                    case 16:
                        return Convert.ToInt64(ResultDisplay.Replace(" ", ""), 16);
                    case 10:
                        return long.Parse(ResultDisplay.Replace(",", "").Replace(".", ""));
                    case 8:
                        return Convert.ToInt64(ResultDisplay.Replace(" ", ""), 8);
                    case 2:
                        return Convert.ToInt64(ResultDisplay.Replace(" ", ""), 2);
                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private void HandleOperation(char operation)
        {
            try
            {
                if (UseOperationOrder && !IsProgrammerMode)
                {
                    if (_equalsPressed)
                    {
                        EquationDisplay = "";
                        _equalsPressed = false;
                    }

                    if (_isNewNumber && !string.IsNullOrEmpty(EquationDisplay))
                    {
                        if (EquationDisplay.EndsWith("+") ||
                            EquationDisplay.EndsWith("-") ||
                            EquationDisplay.EndsWith("×") ||
                            EquationDisplay.EndsWith("÷"))
                        {
                            EquationDisplay = EquationDisplay.Substring(0, EquationDisplay.Length - 1) + operation;
                        }
                        else
                        {
                            EquationDisplay += operation;
                        }
                    }
                    else
                    {
                        EquationDisplay += (_isNewNumber ? _currentValue.ToString() : ResultDisplay) + operation;
                        _isNewNumber = true;
                    }

                    _currentValue = double.Parse(ResultDisplay);

                    return;
                }

                if (IsProgrammerMode)
                {
                    long displayValue = ParseCurrentDisplayValue();
                    if (_equalsPressed)
                    {
                        _equalsPressed = false;
                        _storedIntValue = displayValue;
                    }
                    else if (!string.IsNullOrEmpty(_currentOperation))
                    {
                        displayValue = PerformIntCalculation(_storedIntValue, displayValue, _currentOperation);
                    }

                    _storedIntValue = displayValue;
                    _currentOperation = operation.ToString();

                    EquationDisplay = $"{FormatNumberForBase(_storedIntValue)} {_currentOperation}";

                    ResultDisplay = FormatNumberForBase(displayValue);
                    _isNewNumber = true;
                }
                else
                {
                    double displayValue = double.Parse(ResultDisplay);

                    if (_equalsPressed)
                    {
                        _equalsPressed = false;
                        _currentValue = displayValue;
                    }
                    else if (!string.IsNullOrEmpty(_currentOperation))
                    {
                        displayValue = PerformCalculation(_currentValue, displayValue, _currentOperation);
                    }

                    _currentValue = displayValue;
                    _currentOperation = operation.ToString();

                    EquationDisplay = $"{_currentValue} {_currentOperation}";

                    ResultDisplay = _currentValue.ToString();
                    _isNewNumber = true;
                    HasDecimal = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void HandleBitwiseOperation(string operation)
        {
            try
            {
                if (operation == "NOT")
                {
                    long result = ~_currentIntValue;

                    EquationDisplay = $"NOT({FormatNumberForBase(_currentIntValue)}) =";
                    ResultDisplay = FormatNumberForBase(result);

                    _currentIntValue = result;
                    _isNewNumber = true;
                    _equalsPressed = true;
                }
                else
                {
                    _currentOperation = operation;
                    EquationDisplay = $"{FormatNumberForBase(_currentIntValue)} {operation}";
                    _isNewNumber = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in bitwise operation: {ex.Message}", "Operation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void PerformEquals()
        {
            if (UseOperationOrder && !IsProgrammerMode)
            {
                try
                {
                    string expression = EquationDisplay;

                    if (_equalsPressed && !string.IsNullOrEmpty(_lastEquation))
                    {
                        expression = _lastEquation;
                    }

                    if (!string.IsNullOrEmpty(expression))
                    {
                        if (expression.EndsWith("="))
                        {
                            expression = expression.Substring(0, expression.Length - 1);
                        }

                        if (!expression.Contains("="))
                        {
                            expression += ResultDisplay;
                        }

                        EquationDisplay = expression + "=";
                        _lastEquation = EquationDisplay;

                        double result = EvaluateExpression(expression);
                        ResultDisplay = result.ToString();
                        _currentValue = result;

                        _equalsPressed = true;
                        _isNewNumber = true;
                        HasDecimal = false;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetCalculator();
                    return;
                }
            }
            if (!string.IsNullOrEmpty(_currentOperation))
            {
                try
                {
                    if (IsProgrammerMode)
                    {
                        long displayValue = ParseCurrentDisplayValue();

                        if (_equalsPressed)
                        {
                            EquationDisplay = $"{FormatNumberForBase(_storedIntValue)} {_currentOperation} {FormatNumberForBase((long)_lastOperand)} =";
                            displayValue = (long)_lastOperand;
                        }
                        else
                        {
                            _lastOperand = displayValue;
                            EquationDisplay = $"{FormatNumberForBase(_storedIntValue)} {_currentOperation} {FormatNumberForBase(displayValue)} =";
                        }

                        long result = PerformIntCalculation(_storedIntValue, displayValue, _currentOperation);

                        _lastEquation = EquationDisplay;
                        ResultDisplay = FormatNumberForBase(result);
                        _currentIntValue = result;

                        _equalsPressed = true;
                        _isNewNumber = true;
                    
                    }
                    else
                    {
                        double displayValue = double.Parse(ResultDisplay);

                        if (_equalsPressed)
                        {
                            EquationDisplay = $"{_currentValue} {_currentOperation} {_lastOperand} =";
                            displayValue = _lastOperand;
                        }
                        else
                        {
                            _lastOperand = displayValue;
                            EquationDisplay = $"{_currentValue} {_currentOperation} {displayValue} =";
                        }

                        double result = PerformCalculation(_currentValue, displayValue, _currentOperation);

                        _lastEquation = EquationDisplay;
                        ResultDisplay = result.ToString();
                        _currentValue = result;

                        _equalsPressed = true;
                        _isNewNumber = true;
                        HasDecimal = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetCalculator();
                }
            }
        }

        private void ClearEntry()
        {
            ResultDisplay = "0";
            _isNewNumber = true;
            HasDecimal = false;
            if (IsProgrammerMode)
            {
                _currentIntValue = 0;
            }
        }

        private void HandleBackspace()
        {
            if (ResultDisplay.Length > 1)
            {
                ResultDisplay = ResultDisplay.Substring(0, ResultDisplay.Length - 1);
                if (!IsProgrammerMode)
                {
                    HasDecimal = ResultDisplay.Contains(".");
                }
                else
                {
                    UpdateCurrentIntValue();
                }
            }
            else
            {
                ResultDisplay = "0";
                _isNewNumber = true;
                HasDecimal = false;
                if (IsProgrammerMode)
                {
                    _currentIntValue = 0;
                }
            }
        }

        private void SquareValue()
        {
            try
            {
                double displayValue = double.Parse(ResultDisplay);
                double result = displayValue * displayValue;

                EquationDisplay = $"sqr({displayValue}) =";
                ResultDisplay = result.ToString();

                _currentValue = result;
                _isNewNumber = true;
                HasDecimal = false;
                _equalsPressed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void SquareRootValue()
        {
            try
            {
                double displayValue = double.Parse(ResultDisplay);

                if (displayValue < 0)
                {
                    MessageBox.Show("Cannot calculate square root of negative number in real number system.", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                double result = Math.Sqrt(displayValue);

                EquationDisplay = $"√({displayValue}) =";
                ResultDisplay = result.ToString();

                _currentValue = result;
                _isNewNumber = true;
                HasDecimal = false;
                _equalsPressed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void InverseValue()
        {
            try
            {
                double displayValue = double.Parse(ResultDisplay);

                if (displayValue == 0)
                {
                    MessageBox.Show("Cannot calculate inverse of zero (division by zero).", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                double result = 1 / displayValue;

                EquationDisplay = $"1/({displayValue}) =";
                ResultDisplay = result.ToString();

                _currentValue = result;
                _isNewNumber = true;
                HasDecimal = false;
                _equalsPressed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void NegateValue()
        {
            try
            {
                if (ResultDisplay != "0")
                {
                    double displayValue = double.Parse(ResultDisplay);
                    displayValue = -displayValue;
                    ResultDisplay = displayValue.ToString();

                    if (_isNewNumber)
                    {
                        _currentValue = displayValue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in calculation: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetCalculator();
            }
        }

        private void MemoryStore()
        {
            if (IsProgrammerMode)
            {
                _storedValue = _currentIntValue;
            }
            else
            {
                _storedValue = double.Parse(ResultDisplay);
            }
            MemoryDisplay = "M";
        }

        private void MemoryRecall()
        {
            if (IsProgrammerMode)
            {
                _currentIntValue = (long)_storedValue;
                ResultDisplay = FormatNumberForBase(_currentIntValue);
            }
            else
            {
                ResultDisplay = _storedValue.ToString();
            }
            _isNewNumber = true;
        }

        private void MemoryPlus()
        {
            if (IsProgrammerMode)
            {
                _storedValue += _currentIntValue;
            }
            else
            {
                double currentDisplayValue = double.Parse(ResultDisplay);
                _storedValue += currentDisplayValue;
            }
            MemoryDisplay = "M";
        }

        private void MemoryMinus()
        {
            if (IsProgrammerMode)
            {
                _storedValue -= _currentIntValue;
            }
            else
            {
                double currentDisplayValue = double.Parse(ResultDisplay);
                _storedValue -= currentDisplayValue;
            }
            MemoryDisplay = "M";
        }

        private void MemoryClear()
        {
            _storedValue = 0;
            MemoryDisplay = string.Empty;
        }

        private void Copy()
        {
            Clipboard.SetText(ResultDisplay);
        }

        private void Cut()
        {
            Clipboard.SetText(ResultDisplay);
            ResultDisplay = "0";
            _isNewNumber = true;
            if (IsProgrammerMode)
            {
                _currentIntValue = 0;
            }
        }

        private void Paste()
        {
            string clipboardText = Clipboard.GetText();

            if (IsProgrammerMode)
            {
                try
                {
                    long pastedValue = 0;
                    if (CurrentBase == 16)
                    {
                        pastedValue = Convert.ToInt64(clipboardText, 16);
                    }
                    else if (CurrentBase == 10)
                    {
                        pastedValue = long.Parse(clipboardText);
                    }
                    else if (CurrentBase == 8)
                    {
                        pastedValue = Convert.ToInt64(clipboardText, 8);
                    }
                    else if (CurrentBase == 2)
                    {
                        pastedValue = Convert.ToInt64(clipboardText, 2);
                    }

                    _currentIntValue = pastedValue;
                    ResultDisplay = FormatNumberForBase(pastedValue);
                    _isNewNumber = true;
                }
                catch
                {
                    MessageBox.Show("Invalid format for current base.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (double.TryParse(clipboardText, out double pastedValue))
                {
                    ResultDisplay = pastedValue.ToString();
                    _isNewNumber = true;
                }
            }
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show("Calculator\n\nDeveloped by: Gelegram Alexandru\nGroup: LF232",
                           "About Calculator", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleDigitGrouping(bool isEnabled)
        {
            IsDigitGroupingEnabled = isEnabled;
            _settings.IsDigitGroupingEnabled = isEnabled;
            _settings.Save();

            if (IsProgrammerMode)
            {
                ResultDisplay = FormatNumberForBase(_currentIntValue);
            }
        }

        private void HandleKeyInput(Key key)
        {
            if (IsProgrammerMode)
            {
                if (IsHexMode && ((key >= Key.A && key <= Key.F) || (key >= Key.A + 32 && key <= Key.F + 32)))
                {
                    HandleHexInput(key.ToString().Last());
                    return;
                }
                if (key >= Key.D0 && key <= Key.D9)
                {
                    int digit = key - Key.D0;
                    if (digit < CurrentBase)
                        HandleNumberInput(key.ToString().Last());
                }
                else if (key >= Key.NumPad0 && key <= Key.NumPad9)
                {
                    int digit = key - Key.NumPad0;
                    if (digit < CurrentBase)
                        HandleNumberInput((key - Key.NumPad0).ToString()[0]);
                }
            }
            else
            {
                if (key >= Key.D0 && key <= Key.D9)
                {
                    HandleNumberInput(key.ToString().Last());
                }
                else if (key >= Key.NumPad0 && key <= Key.NumPad9)
                {
                    HandleNumberInput((key - Key.NumPad0).ToString()[0]);
                }
                else if (key == Key.Decimal || key == Key.OemPeriod)
                {
                    HandleNumberInput('.');
                }
            }

            if (key == Key.Add || key == Key.OemPlus)
            {
                HandleOperation('+');
            }
            else if (key == Key.Subtract || key == Key.OemMinus)
            {
                HandleOperation('-');
            }
            else if (key == Key.Multiply)
            {
                HandleOperation('×');
            }
            else if (key == Key.Divide)
            {
                HandleOperation('÷');
            }
            else if (key == Key.Enter)
            {
                PerformEquals();
            }
            else if (key == Key.Back)
            {
                HandleBackspace();
            }
            else if (key == Key.Escape)
            {
                ResetCalculator();
            }
        }

        #endregion

        #region Helper Methods

        private void ResetCalculator()
        {
            ResultDisplay = "0";
            EquationDisplay = "";
            if (IsProgrammerMode)
            {
                _currentIntValue = 0;
            }
            else
            {
                _currentValue = 0;
            }
            _currentOperation = "";
            _isNewNumber = true;
            HasDecimal = false;
            _equalsPressed = false;
            _lastOperand = 0;
            _lastEquation = "";
        }

        private double PerformCalculation(double a, double b, string operation)
        {
            switch (operation)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "×": return a * b;
                case "÷":
                    if (b == 0)
                    {
                        MessageBox.Show("Cannot divide by zero!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return a;
                    }
                    return a / b;
                case "%": return a % b;
                default: return b;
            }
        }

        private long PerformIntCalculation(long a, long b, string operation)
        {
            switch (operation)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "×": return a * b;
                case "÷":
                    if (b == 0)
                    {
                        MessageBox.Show("Cannot divide by zero!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return a;
                    }
                    return a / b;
                case "%": return a % b;
                case "AND": return a & b;
                case "OR": return a | b;
                case "XOR": return a ^ b;
                case "NOT": return ~a;
                default: return b;
            }
        }

        private void UpdateCurrentIntValue()
        {
            try
            {
                switch (CurrentBase)
                {
                    case 16:
                        _currentIntValue = Convert.ToInt64(ResultDisplay.Replace(" ", ""), 16);
                        break;
                    case 10:
                        _currentIntValue = long.Parse(ResultDisplay.Replace(",", "").Replace(".", ""));
                        break;
                    case 8:
                        _currentIntValue = Convert.ToInt64(ResultDisplay.Replace(" ", ""), 8);
                        break;
                    case 2:
                        _currentIntValue = Convert.ToInt64(ResultDisplay.Replace(" ", ""), 2);
                        break;
                }
            }
            catch (Exception)
            {
                _currentIntValue = 0;
                ResultDisplay = "0";
            }
        }

        private string FormatNumberForBase(long number)
        {
            switch (CurrentBase)
            {
                case 16:
                    return IsDigitGroupingEnabled
                        ? FormatWithSeparator(Convert.ToString(number, 16).ToUpper(), 4, " ")
                        : Convert.ToString(number, 16).ToUpper();
                case 10:
                    return IsDigitGroupingEnabled
                        ? number.ToString("N0", CultureInfo.CurrentCulture)
                        : number.ToString();
                case 8:
                    return IsDigitGroupingEnabled
                        ? FormatWithSeparator(Convert.ToString(number, 8), 3, " ")
                        : Convert.ToString(number, 8);
                case 2:
                    return IsDigitGroupingEnabled
                        ? FormatWithSeparator(Convert.ToString(number, 2), 4, " ")
                        : Convert.ToString(number, 2);
                default:
                    return number.ToString();
            }
        }

        private string FormatWithSeparator(string value, int groupSize, string separator)
        {
            if (string.IsNullOrEmpty(value)) return value;

            bool isNegative = value.StartsWith("-");
            string valueToFormat = isNegative ? value.Substring(1) : value;

            int length = valueToFormat.Length;
            if (length <= groupSize) return value;

            StringBuilder result = new StringBuilder();

            int remainder = length % groupSize;
            if (remainder > 0)
            {
                result.Append(valueToFormat.Substring(0, remainder));
                if (length > remainder) result.Append(separator);
            }

            for (int i = remainder; i < length; i += groupSize)
            {
                result.Append(valueToFormat.Substring(i, Math.Min(groupSize, length - i)));
                if (i + groupSize < length) result.Append(separator);
            }

            return isNegative ? "-" + result.ToString() : result.ToString();
        }

        private void UpdateHexButtonsState()
        {
            bool[] hexButtonsEnabled = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                hexButtonsEnabled[i] = IsHexMode;
            }
            HexButtonsEnabled = hexButtonsEnabled;

            bool[] numberButtonsEnabled = new bool[10];
            for (int i = 0; i < 10; i++)
            {
                numberButtonsEnabled[i] = i < CurrentBase;
            }
            NumberButtonsEnabled = numberButtonsEnabled;
        }

        private void UpdateDisplay()
        {
            if (IsProgrammerMode)
            {
                ResultDisplay = FormatNumberForBase(_currentIntValue);
            }
        }
    }
    #endregion
}
