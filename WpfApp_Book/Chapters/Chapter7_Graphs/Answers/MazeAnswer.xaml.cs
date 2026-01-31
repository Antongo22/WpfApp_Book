using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp_Book.Chapters.Chapter7_Graphs.Answers
{
    public partial class MazeAnswer : Page
    {
        private const int CellSize = 30;
        private int mazeWidth = 15;
        private int mazeHeight = 12;
        private int[,] maze = null!;
        
        private Point playerPos;
        private Point exitPos;
        private Rectangle playerRect = null!;
        private Rectangle exitRect = null!;
        private List<Rectangle> pathRects = new List<Rectangle>();
        
        private int steps = 0;
        private Random random = new Random();
        private DateTime startTime;

        public MazeAnswer()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            UpdateMazeSize();
            GenerateMaze();
            DrawMaze();
        }

        private void MazeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MazeCanvas.ActualWidth > 0 && MazeCanvas.ActualHeight > 0)
            {
                UpdateMazeSize();
                if (maze != null)
                {
                    GenerateMaze();
                    DrawMaze();
                }
            }
        }

        private void UpdateMazeSize()
        {
            mazeWidth = Math.Max(5, (int)(MazeCanvas.ActualWidth / CellSize));
            mazeHeight = Math.Max(5, (int)(MazeCanvas.ActualHeight / CellSize));
        }

        /// <summary>
        /// Генерация лабиринта с гарантированным путём
        /// </summary>
        private void GenerateMaze()
        {
            maze = new int[mazeHeight, mazeWidth];
            
            // Заполняем стенами (30% вероятность)
            for (int y = 0; y < mazeHeight; y++)
                for (int x = 0; x < mazeWidth; x++)
                    maze[y, x] = random.NextDouble() < 0.3 ? 1 : 0;

            // Старт и финиш
            playerPos = new Point(0, 0);
            exitPos = new Point(mazeWidth - 1, mazeHeight - 1);
            maze[0, 0] = 0;
            maze[mazeHeight - 1, mazeWidth - 1] = 0;

            // Создаём гарантированный путь (случайный обход)
            int px = 0, py = 0;
            while (px < mazeWidth - 1 || py < mazeHeight - 1)
            {
                maze[py, px] = 0;
                if (px >= mazeWidth - 1) py++;
                else if (py >= mazeHeight - 1) px++;
                else if (random.NextDouble() < 0.5) px++;
                else py++;
            }
            maze[mazeHeight - 1, mazeWidth - 1] = 0;

            steps = 0;
            StepsText.Text = "0";
            StatusText.Text = "Найдите выход! Используйте ↑↓←→";
            startTime = DateTime.Now;
        }

        /// <summary>
        /// Отрисовка лабиринта
        /// </summary>
        private void DrawMaze()
        {
            MazeCanvas.Children.Clear();
            pathRects.Clear();

            // Рисуем клетки
            for (int y = 0; y < mazeHeight; y++)
            {
                for (int x = 0; x < mazeWidth; x++)
                {
                    var cell = new Rectangle
                    {
                        Width = CellSize - 1,
                        Height = CellSize - 1,
                        Fill = new SolidColorBrush(maze[y, x] == 1 
                            ? Color.FromRgb(44, 62, 80)    // Стена
                            : Color.FromRgb(52, 73, 94))   // Проход
                    };
                    Canvas.SetLeft(cell, x * CellSize);
                    Canvas.SetTop(cell, y * CellSize);
                    MazeCanvas.Children.Add(cell);
                }
            }

            // Выход (зелёный)
            exitRect = new Rectangle
            {
                Width = CellSize - 4,
                Height = CellSize - 4,
                Fill = Brushes.LimeGreen,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(exitRect, exitPos.X * CellSize + 2);
            Canvas.SetTop(exitRect, exitPos.Y * CellSize + 2);
            MazeCanvas.Children.Add(exitRect);

            // Игрок (жёлтый)
            playerRect = new Rectangle
            {
                Width = CellSize - 6,
                Height = CellSize - 6,
                Fill = Brushes.Gold,
                RadiusX = 10,
                RadiusY = 10
            };
            Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3);
            Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);
            MazeCanvas.Children.Add(playerRect);
        }

        /// <summary>
        /// Обработка клавиатуры
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            Point newPos = playerPos;
            
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    newPos.Y--;
                    break;
                case Key.Down:
                case Key.S:
                    newPos.Y++;
                    break;
                case Key.Left:
                case Key.A:
                    newPos.X--;
                    break;
                case Key.Right:
                case Key.D:
                    newPos.X++;
                    break;
                case Key.Space:
                    ShowPath_Click(sender, e);
                    return;
            }

            if (CanMove((int)newPos.X, (int)newPos.Y))
            {
                ClearPath();
                playerPos = newPos;
                steps++;
                StepsText.Text = steps.ToString();
                
                Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3);
                Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);

                if (playerPos == exitPos)
                {
                    var time = DateTime.Now - startTime;
                    StatusText.Text = "🎉 Победа!";
                    TimerText.Text = $"Время: {time.TotalSeconds:F1} сек";
                    MessageBox.Show($"Поздравляем!\n\nШагов: {steps}\nВремя: {time.TotalSeconds:F1} сек",
                        "Победа!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            e.Handled = true;
        }

        private bool CanMove(int x, int y)
        {
            return x >= 0 && y >= 0 && x < mazeWidth && y < mazeHeight && maze[y, x] == 0;
        }

        /// <summary>
        /// Показать кратчайший путь (BFS)
        /// </summary>
        private void ShowPath_Click(object sender, RoutedEventArgs e)
        {
            ClearPath();
            var path = FindPathBFS(playerPos, exitPos);

            if (path == null)
            {
                StatusText.Text = "❌ Путь не найден!";
                return;
            }

            StatusText.Text = $"📍 Путь найден: {path.Count} шагов";

            // Визуализируем путь
            foreach (var p in path)
            {
                if (p == playerPos || p == exitPos) continue;
                
                var rect = new Rectangle
                {
                    Width = CellSize - 8,
                    Height = CellSize - 8,
                    Fill = new SolidColorBrush(Color.FromArgb(180, 52, 152, 219)),
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(rect, p.X * CellSize + 4);
                Canvas.SetTop(rect, p.Y * CellSize + 4);
                MazeCanvas.Children.Add(rect);
                pathRects.Add(rect);
            }

            this.Focus();
        }

        /// <summary>
        /// BFS — поиск кратчайшего пути
        /// </summary>
        private List<Point>? FindPathBFS(Point start, Point end)
        {
            var queue = new Queue<Point>();
            var cameFrom = new Dictionary<Point, Point>();

            queue.Enqueue(start);
            cameFrom[start] = start;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == end)
                {
                    // Восстанавливаем путь
                    var path = new List<Point>();
                    var c = end;
                    while (cameFrom[c] != c)
                    {
                        path.Add(c);
                        c = cameFrom[c];
                    }
                    path.Add(c);
                    path.Reverse();
                    return path;
                }

                for (int i = 0; i < 4; i++)
                {
                    var next = new Point(current.X + dx[i], current.Y + dy[i]);
                    
                    if (CanMove((int)next.X, (int)next.Y) && !cameFrom.ContainsKey(next))
                    {
                        queue.Enqueue(next);
                        cameFrom[next] = current;
                    }
                }
            }

            return null;
        }

        private void ClearPath()
        {
            foreach (var r in pathRects)
                MazeCanvas.Children.Remove(r);
            pathRects.Clear();
        }

        private void NewMaze_Click(object sender, RoutedEventArgs e)
        {
            GenerateMaze();
            DrawMaze();
            this.Focus();
        }
    }
}
