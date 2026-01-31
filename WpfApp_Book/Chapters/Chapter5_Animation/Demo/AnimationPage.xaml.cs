using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter5_Animation.Demo
{
    public partial class AnimationPage : Page
    {
        // Таймер для игрового цикла
        private DispatcherTimer gameTimer;
        
        // Скорость (пикселей за кадр)
        // velocityX > 0 → вправо, < 0 → влево
        // velocityY > 0 → вниз, < 0 → вверх
        private double velocityX = 4;
        private double velocityY = 3;
        
        // Счётчик отскоков
        private int bounceCount = 0;
        
        // Генератор случайных чисел для цветов
        private Random random = new Random();
        
        // Флаг: запущена ли анимация
        private bool isRunning = false;

        public AnimationPage()
        {
            InitializeComponent();
            
            // Создаём и настраиваем таймер
            gameTimer = new DispatcherTimer();
            
            // Интервал 16 мс ≈ 60 кадров в секунду (60 FPS)
            // Формула: FPS = 1000 / interval_ms
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            
            // Подписываемся на событие Tick — оно будет вызываться каждые 16 мс
            gameTimer.Tick += GameLoop;
            
            UpdateVelocityText();
        }

        /// <summary>
        /// Вызывается при загрузке страницы
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем начальную позицию мяча
            Canvas.SetLeft(Ball, 50);
            Canvas.SetTop(Ball, 50);
        }

        /// <summary>
        /// Вызывается при выгрузке страницы (переключение на другую вкладку)
        /// ВАЖНО: останавливаем таймер, чтобы он не работал в фоне
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            isRunning = false;
        }

        /// <summary>
        /// Обработчик кнопки Старт/Стоп
        /// </summary>
        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                gameTimer.Stop();
                StartStopButton.Content = "▶ Старт";
                StartStopButton.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            }
            else
            {
                gameTimer.Start();
                StartStopButton.Content = "⏸ Стоп";
                StartStopButton.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            }
            isRunning = !isRunning;
        }

        /// <summary>
        /// Игровой цикл — вызывается каждые 16 мс (60 раз в секунду)
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            // 1. ПОЛУЧАЕМ ТЕКУЩУЮ ПОЗИЦИЮ
            double x = Canvas.GetLeft(Ball);
            double y = Canvas.GetTop(Ball);
            
            // 2. ПРИМЕНЯЕМ СКОРОСТЬ (сдвигаем позицию)
            x += velocityX;
            y += velocityY;
            
            // 3. ПРОВЕРЯЕМ СТОЛКНОВЕНИЯ С ГРАНИЦАМИ
            bool bounced = false;
            
            // Размеры мяча и холста
            double ballWidth = Ball.Width;
            double ballHeight = Ball.Height;
            double canvasWidth = AnimationCanvas.ActualWidth;
            double canvasHeight = AnimationCanvas.ActualHeight;
            
            // Левая граница (x <= 0) или правая граница (x + ширина >= ширина_холста)
            if (x <= 0 || x + ballWidth >= canvasWidth)
            {
                velocityX = -velocityX;  // Инвертируем горизонтальную скорость
                bounced = true;
            }
            
            // Верхняя граница (y <= 0) или нижняя граница (y + высота >= высота_холста)
            if (y <= 0 || y + ballHeight >= canvasHeight)
            {
                velocityY = -velocityY;  // Инвертируем вертикальную скорость
                bounced = true;
            }
            
            // 4. ДЕЙСТВИЯ ПРИ ОТСКОКЕ
            if (bounced)
            {
                // Меняем цвет на случайный
                BallBrush.Color = Color.FromRgb(
                    (byte)random.Next(50, 256),   // R: не слишком тёмный
                    (byte)random.Next(50, 256),   // G
                    (byte)random.Next(50, 256)    // B
                );
                
                // Увеличиваем счётчик
                bounceCount++;
                BounceCountText.Text = $"Отскоков: {bounceCount}";
                
                UpdateVelocityText();
            }
            
            // 5. ОБНОВЛЯЕМ ПОЗИЦИЮ (с ограничением, чтобы не выйти за границы)
            Canvas.SetLeft(Ball, Math.Clamp(x, 0, Math.Max(0, canvasWidth - ballWidth)));
            Canvas.SetTop(Ball, Math.Clamp(y, 0, Math.Max(0, canvasHeight - ballHeight)));
        }
        
        /// <summary>
        /// Обновляет текст с информацией о скорости
        /// </summary>
        private void UpdateVelocityText()
        {
            double speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
            string dirX = velocityX > 0 ? "→" : "←";
            string dirY = velocityY > 0 ? "↓" : "↑";
            VelocityText.Text = $"Скорость: {speed:F1} px/кадр ({dirX}{dirY})";
        }
    }
}
