using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter3_Keyboard.Answers
{
    public partial class KeyGameAnswer : Page
    {
        private Random random = new Random();
        private DispatcherTimer gameTimer;
        private DispatcherTimer feedbackTimer;
        
        private char currentLetter = '?';
        private int score = 0;
        private int highScore = 0;
        private int timeLeft = 30;
        private bool isPlaying = false;

        public KeyGameAnswer()
        {
            InitializeComponent();
            
            // Таймер игры (1 секунда)
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            gameTimer.Tick += GameTimer_Tick;
            
            // Таймер для обратной связи (скрывает сообщение)
            feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            feedbackTimer.Tick += (s, e) =>
            {
                FeedbackText.Text = "";
                LetterBorder.Background = new SolidColorBrush(Color.FromRgb(44, 62, 80));
                feedbackTimer.Stop();
            };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        /// <summary>
        /// Обработка нажатия клавиши
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isPlaying) return;

            // Получаем нажатую клавишу
            Key pressedKey = e.Key;
            
            // Ожидаемая клавиша
            Key expectedKey = (Key)Enum.Parse(typeof(Key), currentLetter.ToString());

            if (pressedKey == expectedKey)
            {
                // Правильно!
                score++;
                ShowFeedback("✓", Colors.LimeGreen);
                GenerateNewLetter();
            }
            else if (pressedKey >= Key.A && pressedKey <= Key.Z)
            {
                // Неправильно (только буквы считаются ошибкой)
                score = Math.Max(0, score - 1);
                ShowFeedback("✗", Colors.Red);
            }

            ScoreText.Text = $"Счёт: {score}";
            e.Handled = true;
        }

        /// <summary>
        /// Показать обратную связь
        /// </summary>
        private void ShowFeedback(string text, Color color)
        {
            FeedbackText.Text = text;
            FeedbackText.Foreground = new SolidColorBrush(color);
            LetterBorder.Background = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
            
            feedbackTimer.Stop();
            feedbackTimer.Start();
        }

        /// <summary>
        /// Генерация новой случайной буквы
        /// </summary>
        private void GenerateNewLetter()
        {
            currentLetter = (char)('A' + random.Next(26));
            LetterText.Text = currentLetter.ToString();
        }

        /// <summary>
        /// Тик игрового таймера
        /// </summary>
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            timeLeft--;
            TimerText.Text = $"Время: {timeLeft}";

            if (timeLeft <= 0)
            {
                EndGame();
            }
            else if (timeLeft <= 5)
            {
                TimerText.Foreground = Brushes.Red;
            }
        }

        /// <summary>
        /// Старт игры
        /// </summary>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                // Пауза
                gameTimer.Stop();
                isPlaying = false;
                StartButton.Content = "▶ Старт";
                InstructionText.Text = "Пауза. Нажмите Старт.";
            }
            else
            {
                // Запуск
                if (timeLeft <= 0)
                {
                    // Новая игра
                    timeLeft = 30;
                    score = 0;
                    ScoreText.Text = "Счёт: 0";
                    TimerText.Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18));
                }
                
                isPlaying = true;
                StartButton.Content = "⏸ Пауза";
                InstructionText.Text = "Нажимайте показанную букву!";
                GenerateNewLetter();
                gameTimer.Start();
                this.Focus();
            }
        }

        /// <summary>
        /// Сброс игры
        /// </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            isPlaying = false;
            
            score = 0;
            timeLeft = 30;
            currentLetter = '?';
            
            ScoreText.Text = "Счёт: 0";
            TimerText.Text = "Время: 30";
            TimerText.Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18));
            LetterText.Text = "?";
            FeedbackText.Text = "";
            InstructionText.Text = "Нажмите Старт!";
            StartButton.Content = "▶ Старт";
            LetterBorder.Background = new SolidColorBrush(Color.FromRgb(44, 62, 80));
            
            this.Focus();
        }

        /// <summary>
        /// Конец игры
        /// </summary>
        private void EndGame()
        {
            gameTimer.Stop();
            isPlaying = false;
            
            // Обновляем рекорд
            if (score > highScore)
            {
                highScore = score;
                HighScoreText.Text = $"Рекорд: {highScore} 🏆";
            }
            
            StartButton.Content = "▶ Старт";
            InstructionText.Text = $"Игра окончена! Счёт: {score}";
            LetterText.Text = "🎮";
            
            MessageBox.Show($"Время вышло!\n\nВаш счёт: {score}\nРекорд: {highScore}", 
                "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
