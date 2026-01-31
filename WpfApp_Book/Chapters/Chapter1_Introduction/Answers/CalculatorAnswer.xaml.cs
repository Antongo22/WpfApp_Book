using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_Book.Chapters.Chapter1_Introduction.Answers
{
    public partial class CalculatorAnswer : Page
    {
        private string currentOperation = "+";

        public CalculatorAnswer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Выбор операции
        /// </summary>
        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            currentOperation = btn.Content.ToString()!;
            OperatorText.Text = currentOperation;
            ErrorText.Text = "";
        }

        /// <summary>
        /// Вычисление результата
        /// </summary>
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            // Парсим первое число
            if (!double.TryParse(Number1Box.Text, out double num1))
            {
                ErrorText.Text = "Ошибка: первое число некорректно";
                return;
            }

            // Парсим второе число
            if (!double.TryParse(Number2Box.Text, out double num2))
            {
                ErrorText.Text = "Ошибка: второе число некорректно";
                return;
            }

            double result = 0;
            string expression = $"{num1} {currentOperation} {num2}";

            // Выполняем операцию
            switch (currentOperation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "−":
                    result = num1 - num2;
                    break;
                case "×":
                    result = num1 * num2;
                    break;
                case "÷":
                    if (num2 == 0)
                    {
                        ErrorText.Text = "Ошибка: деление на ноль!";
                        ResultText.Text = "∞";
                        return;
                    }
                    result = num1 / num2;
                    break;
            }

            // Отображаем результат
            ExpressionText.Text = expression + " =";
            ResultText.Text = result.ToString("G10"); // Форматирование для красивого вывода
        }

        /// <summary>
        /// Очистка
        /// </summary>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Number1Box.Text = "0";
            Number2Box.Text = "0";
            ResultText.Text = "0";
            ExpressionText.Text = "";
            ErrorText.Text = "";
            currentOperation = "+";
            OperatorText.Text = "+";
        }
    }
}
