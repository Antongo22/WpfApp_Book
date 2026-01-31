using System.Windows;
using System.Windows.Controls;

namespace WpfApp_Book.Chapters.Chapter1_Introduction.Demo
{
    public partial class IntroductionPage : Page
    {
        private int clickCount = 0;

        public IntroductionPage()
        {
            InitializeComponent();
        }

        // ===== КЛИКЕР =====
        private void ClickerButton_Click(object sender, RoutedEventArgs e)
        {
            clickCount++;
            ClickCountText.Text = $"Кликов: {clickCount}";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            clickCount = 0;
            ClickCountText.Text = "Кликов: 0";
        }

        // ===== TEXTBOX =====
        private void DemoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxPreview != null)
            {
                TextBoxPreview.Text = $" → {DemoTextBox.Text}";
            }
        }

        // ===== CHECKBOX =====
        private void CheckBoxDemo_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxStatus != null)
            {
                bool? isChecked = CheckBoxDemo.IsChecked;
                string status = isChecked switch
                {
                    true => "Включено ✓",
                    false => "Выключено ✗",
                    null => "Неопределено ?"
                };
                CheckBoxStatus.Text = $"Статус: {status}";
            }
        }

        // ===== RADIOBUTTON =====
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadioStatus != null)
            {
                // Определяем выбранный размер
                string size = "?";
                if (RadioSmall.IsChecked == true) size = "Маленький";
                else if (RadioMedium.IsChecked == true) size = "Средний";
                else if (RadioLarge.IsChecked == true) size = "Большой";

                // Определяем выбранный цвет
                string color = "?";
                if (RadioRed.IsChecked == true) color = "Красный";
                else if (RadioGreen.IsChecked == true) color = "Зелёный";
                else if (RadioBlue.IsChecked == true) color = "Синий";

                RadioStatus.Text = $"Выбрано: {size}, {color}";
            }
        }
    }
}
