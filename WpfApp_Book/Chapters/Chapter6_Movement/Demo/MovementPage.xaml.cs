using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter6_Movement.Demo
{
    public partial class MovementPage : Page
    {
        // Перечисление возможных направлений движения
        private enum Direction { Up, Down, Left, Right }
        
        // Текущее направление движения змейки
        private Direction currentDirection = Direction.Right;
        
        // Список позиций всех сегментов змейки (snakeBody[0] = голова)
        private List<Point> snakeBody = new List<Point>();
        
        // Список визуальных элементов (прямоугольников) для каждого сегмента
        private List<Rectangle> snakeSegments = new List<Rectangle>();
        
        // Размер одной ячейки сетки
        private const int CellSize = 20;
        
        // Позиция еды и её визуальный элемент
        private Point foodPosition;
        private Ellipse? foodElement;
        
        // Таймер для игрового цикла
        private DispatcherTimer gameTimer;
        private bool isRunning = false;
        
        // Счёт игрока
        private int score = 0;
        
        // Генератор случайных чисел
        private Random random = new Random();

        public MovementPage()
        {
            InitializeComponent();
            
            // Создаём таймер с интервалом 150 мс (примерно 6-7 шагов в секунду)
            // Для змейки нужна более низкая скорость, чем для плавной анимации
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            gameTimer.Tick += GameLoop;
        }

        /// <summary>
        /// При загрузке страницы — получаем фокус и инициализируем игру
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // ВАЖНО: без Focus() страница не будет получать события клавиатуры
            this.Focus();
            InitGame();
        }

        /// <summary>
        /// При выгрузке страницы — останавливаем таймер
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
        }

        /// <summary>
        /// Инициализация/сброс игры
        /// </summary>
        private void InitGame()
        {
            // Очищаем всё
            GameCanvas.Children.Clear();
            snakeBody.Clear();
            snakeSegments.Clear();
            
            // Сбрасываем состояние
            currentDirection = Direction.Right;
            score = 0;
            ScoreText.Text = "Счёт: 0";
            DirectionText.Text = "Направление: →";
            
            // Создаём начальную змейку из 3 сегментов
            // Голова в позиции (100, 100), остальные сегменты левее
            for (int i = 0; i < 3; i++)
            {
                Point pos = new Point(100 - i * CellSize, 100);
                snakeBody.Add(pos);
                
                // Создаём визуальный элемент для сегмента
                var segment = new Rectangle
                {
                    Width = CellSize - 2,   // Небольшой отступ для визуального разделения
                    Height = CellSize - 2,
                    Fill = i == 0 ? Brushes.LawnGreen : Brushes.LimeGreen,  // Голова ярче
                    RadiusX = 3,
                    RadiusY = 3
                };
                
                Canvas.SetLeft(segment, pos.X);
                Canvas.SetTop(segment, pos.Y);
                
                snakeSegments.Add(segment);
                GameCanvas.Children.Add(segment);
            }
            
            // Создаём первую еду
            SpawnFood();
        }

        /// <summary>
        /// Создаёт новую еду в случайной позиции
        /// </summary>
        private void SpawnFood()
        {
            // Удаляем старую еду, если есть
            if (foodElement != null)
                GameCanvas.Children.Remove(foodElement);
            
            // Вычисляем размер игрового поля в ячейках
            int maxX = Math.Max(5, (int)(GameCanvas.ActualWidth / CellSize) - 1);
            int maxY = Math.Max(5, (int)(GameCanvas.ActualHeight / CellSize) - 1);
            
            // Генерируем случайную позицию
            // TODO: можно добавить проверку, чтобы еда не появлялась на змейке
            foodPosition = new Point(
                random.Next(0, maxX) * CellSize,
                random.Next(0, maxY) * CellSize
            );
            
            // Создаём визуальный элемент для еды
            foodElement = new Ellipse
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = Brushes.Red
            };
            
            Canvas.SetLeft(foodElement, foodPosition.X);
            Canvas.SetTop(foodElement, foodPosition.Y);
            GameCanvas.Children.Add(foodElement);
        }

        /// <summary>
        /// Обработка нажатий клавиш — меняем направление движения
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            // Важно: нельзя развернуться на 180 градусов
            // (если едем вправо, нельзя сразу поехать влево)
            Direction newDirection = currentDirection;
            
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    if (currentDirection != Direction.Down)
                        newDirection = Direction.Up;
                    break;
                    
                case Key.Down:
                case Key.S:
                    if (currentDirection != Direction.Up)
                        newDirection = Direction.Down;
                    break;
                    
                case Key.Left:
                case Key.A:
                    if (currentDirection != Direction.Right)
                        newDirection = Direction.Left;
                    break;
                    
                case Key.Right:
                case Key.D:
                    if (currentDirection != Direction.Left)
                        newDirection = Direction.Right;
                    break;
            }
            
            if (newDirection != currentDirection)
            {
                currentDirection = newDirection;
                
                // Обновляем индикатор направления
                string arrow = currentDirection switch
                {
                    Direction.Up => "↑",
                    Direction.Down => "↓",
                    Direction.Left => "←",
                    Direction.Right => "→",
                    _ => ""
                };
                DirectionText.Text = $"Направление: {arrow}";
            }
            
            e.Handled = true;  // Предотвращаем дальнейшую обработку
        }

        /// <summary>
        /// Игровой цикл — вызывается по таймеру
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            // 1. Вычисляем новую позицию головы на основе текущего направления
            Point head = snakeBody[0];
            Point newHead = currentDirection switch
            {
                Direction.Up => new Point(head.X, head.Y - CellSize),
                Direction.Down => new Point(head.X, head.Y + CellSize),
                Direction.Left => new Point(head.X - CellSize, head.Y),
                Direction.Right => new Point(head.X + CellSize, head.Y),
                _ => head
            };

            // 2. Проверяем столкновение с границами
            if (newHead.X < 0 || newHead.Y < 0 ||
                newHead.X >= GameCanvas.ActualWidth ||
                newHead.Y >= GameCanvas.ActualHeight)
            {
                GameOver();
                return;
            }

            // 3. Проверяем столкновение с собственным телом
            for (int i = 0; i < snakeBody.Count; i++)
            {
                if (snakeBody[i] == newHead)
                {
                    GameOver();
                    return;
                }
            }

            // 4. Добавляем новую голову
            snakeBody.Insert(0, newHead);
            
            var newSegment = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = Brushes.LawnGreen,  // Голова яркая
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(newSegment, newHead.X);
            Canvas.SetTop(newSegment, newHead.Y);
            snakeSegments.Insert(0, newSegment);
            GameCanvas.Children.Add(newSegment);
            
            // Делаем старую голову цветом тела
            if (snakeSegments.Count > 1)
                snakeSegments[1].Fill = Brushes.LimeGreen;

            // 5. Проверяем, съели ли еду
            if (Math.Abs(newHead.X - foodPosition.X) < CellSize &&
                Math.Abs(newHead.Y - foodPosition.Y) < CellSize)
            {
                // Съели еду — увеличиваем счёт, создаём новую еду
                // НЕ удаляем хвост — змейка растёт
                score++;
                ScoreText.Text = $"Счёт: {score}";
                SpawnFood();
            }
            else
            {
                // Не съели — удаляем хвост (длина остаётся прежней)
                snakeBody.RemoveAt(snakeBody.Count - 1);
                
                var tailSegment = snakeSegments[snakeSegments.Count - 1];
                GameCanvas.Children.Remove(tailSegment);
                snakeSegments.RemoveAt(snakeSegments.Count - 1);
            }
        }

        /// <summary>
        /// Завершение игры
        /// </summary>
        private void GameOver()
        {
            gameTimer.Stop();
            isRunning = false;
            StartButton.Content = "▶ Старт";
            ScoreText.Text = $"Игра окончена! Счёт: {score}";
        }

        /// <summary>
        /// Кнопка Старт/Пауза
        /// </summary>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                gameTimer.Stop();
                StartButton.Content = "▶ Старт";
            }
            else
            {
                gameTimer.Start();
                StartButton.Content = "⏸ Пауза";
            }
            isRunning = !isRunning;
            this.Focus();  // Возвращаем фокус для управления клавишами
        }

        /// <summary>
        /// Кнопка Заново
        /// </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            isRunning = false;
            StartButton.Content = "▶ Старт";
            InitGame();
            this.Focus();
        }
    }
}
