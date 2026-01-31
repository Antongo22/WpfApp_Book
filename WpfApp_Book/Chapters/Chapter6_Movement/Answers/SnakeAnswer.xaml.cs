using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter6_Movement.Answers
{
    public partial class SnakeAnswer : Page
    {
        private enum Direction { Up, Down, Left, Right }
        
        private const int CellSize = 20;
        private DispatcherTimer gameTimer;
        private Random random = new Random();
        
        private List<Point> snakeBody = new List<Point>();
        private List<Rectangle> snakeSegments = new List<Rectangle>();
        private Direction currentDirection = Direction.Right;
        private Direction nextDirection = Direction.Right;
        
        private Point foodPosition;
        private Ellipse? foodElement;
        
        private int score = 0;
        private int highScore = 0;
        private bool isRunning = false;
        private bool gameOver = false;

        public SnakeAnswer()
        {
            InitializeComponent();
            
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(150);
            gameTimer.Tick += GameLoop;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            InitGame();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
        }

        /// <summary>
        /// Инициализация игры
        /// </summary>
        private void InitGame()
        {
            gameTimer.Stop();
            isRunning = false;
            gameOver = false;
            
            GameCanvas.Children.Clear();
            snakeBody.Clear();
            snakeSegments.Clear();
            
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            ScoreText.Text = "Счёт: 0";
            StatusText.Text = "Используйте ↑↓←→ или WASD";
            StartButton.Content = "▶ Старт";
            
            // Сбрасываем скорость
            gameTimer.Interval = TimeSpan.FromMilliseconds(150);

            // Начальная змейка (3 сегмента)
            double startX = 100;
            double startY = 100;
            
            for (int i = 0; i < 3; i++)
            {
                Point pos = new Point(startX - i * CellSize, startY);
                snakeBody.Add(pos);
                
                var segment = new Rectangle
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    Fill = i == 0 ? Brushes.LimeGreen : Brushes.Green,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(segment, pos.X);
                Canvas.SetTop(segment, pos.Y);
                snakeSegments.Add(segment);
                GameCanvas.Children.Add(segment);
            }
            
            SpawnFood();
        }

        /// <summary>
        /// Создание еды в случайной позиции
        /// </summary>
        private void SpawnFood()
        {
            if (foodElement != null)
            {
                GameCanvas.Children.Remove(foodElement);
            }

            int maxX = Math.Max(1, (int)(GameCanvas.ActualWidth / CellSize) - 1);
            int maxY = Math.Max(1, (int)(GameCanvas.ActualHeight / CellSize) - 1);

            // Генерируем позицию, не занятую змейкой
            Point newPos;
            do
            {
                newPos = new Point(random.Next(1, maxX) * CellSize, random.Next(1, maxY) * CellSize);
            } while (snakeBody.Contains(newPos));

            foodPosition = newPos;
            
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
        /// Игровой цикл
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            // Применяем следующее направление
            currentDirection = nextDirection;
            
            // Вычисляем новую позицию головы
            Point head = snakeBody[0];
            Point newHead = currentDirection switch
            {
                Direction.Up => new Point(head.X, head.Y - CellSize),
                Direction.Down => new Point(head.X, head.Y + CellSize),
                Direction.Left => new Point(head.X - CellSize, head.Y),
                Direction.Right => new Point(head.X + CellSize, head.Y),
                _ => head
            };

            // Проверка столкновения со стеной
            if (newHead.X < 0 || newHead.Y < 0 || 
                newHead.X >= GameCanvas.ActualWidth || 
                newHead.Y >= GameCanvas.ActualHeight)
            {
                GameOver();
                return;
            }

            // Проверка столкновения с собой
            if (snakeBody.Contains(newHead))
            {
                GameOver();
                return;
            }

            // Добавляем новую голову
            snakeBody.Insert(0, newHead);
            
            var newSegment = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = Brushes.LimeGreen,
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(newSegment, newHead.X);
            Canvas.SetTop(newSegment, newHead.Y);
            snakeSegments.Insert(0, newSegment);
            GameCanvas.Children.Add(newSegment);
            
            // Обновляем цвет старой головы
            if (snakeSegments.Count > 1)
            {
                snakeSegments[1].Fill = Brushes.Green;
            }

            // Проверяем, съела ли змейка еду
            if (Math.Abs(newHead.X - foodPosition.X) < CellSize && 
                Math.Abs(newHead.Y - foodPosition.Y) < CellSize)
            {
                score++;
                ScoreText.Text = $"Счёт: {score}";
                SpawnFood();
                
                // Увеличиваем скорость каждые 5 очков
                if (score % 5 == 0 && gameTimer.Interval.TotalMilliseconds > 50)
                {
                    gameTimer.Interval = TimeSpan.FromMilliseconds(gameTimer.Interval.TotalMilliseconds - 10);
                    StatusText.Text = "Скорость увеличена!";
                }
            }
            else
            {
                // Удаляем хвост
                snakeBody.RemoveAt(snakeBody.Count - 1);
                var tail = snakeSegments[snakeSegments.Count - 1];
                GameCanvas.Children.Remove(tail);
                snakeSegments.RemoveAt(snakeSegments.Count - 1);
            }
        }

        /// <summary>
        /// Обработка клавиатуры
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            // Запрещаем разворот на 180°
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    if (currentDirection != Direction.Down)
                        nextDirection = Direction.Up;
                    break;
                case Key.Down:
                case Key.S:
                    if (currentDirection != Direction.Up)
                        nextDirection = Direction.Down;
                    break;
                case Key.Left:
                case Key.A:
                    if (currentDirection != Direction.Right)
                        nextDirection = Direction.Left;
                    break;
                case Key.Right:
                case Key.D:
                    if (currentDirection != Direction.Left)
                        nextDirection = Direction.Right;
                    break;
                case Key.Space:
                    if (gameOver) Reset_Click(sender, e);
                    else Start_Click(sender, e);
                    break;
            }
            e.Handled = true;
        }

        private void GameOver()
        {
            gameTimer.Stop();
            isRunning = false;
            gameOver = true;
            
            if (score > highScore)
            {
                highScore = score;
                HighScoreText.Text = highScore.ToString();
            }
            
            StatusText.Text = $"Game Over! Счёт: {score}. Нажмите Заново";
            StartButton.Content = "▶ Старт";
            
            MessageBox.Show($"Игра окончена!\n\nВаш счёт: {score}\nРекорд: {highScore}", 
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver)
            {
                InitGame();
            }
            
            if (isRunning)
            {
                gameTimer.Stop();
                StartButton.Content = "▶ Старт";
                StatusText.Text = "Пауза";
            }
            else
            {
                gameTimer.Start();
                StartButton.Content = "⏸ Пауза";
                StatusText.Text = "Играем!";
            }
            isRunning = !isRunning;
            this.Focus();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            InitGame();
            this.Focus();
        }
    }
}
