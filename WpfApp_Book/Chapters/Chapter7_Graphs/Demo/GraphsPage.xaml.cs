using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp_Book.Chapters.Chapter7_Graphs.Demo
{
    public partial class GraphsPage : Page
    {
        // Размер одной клетки лабиринта в пикселях
        private const int CellSize = 25;
        
        // Размеры лабиринта в клетках
        private int mazeWidth = 15;
        private int mazeHeight = 10;
        
        // Двумерный массив лабиринта: 0 = проход, 1 = стена
        private int[,] maze = null!;
        
        // Позиции игрока и выхода (в клетках, не в пикселях)
        private Point playerPos;
        private Point exitPos;
        
        // Визуальные элементы игрока и выхода
        private Rectangle playerRect = null!;
        private Rectangle exitRect = null!;
        
        // Список визуальных элементов пути (для очистки)
        private List<Rectangle> pathRects = new List<Rectangle>();
        
        // Генератор случайных чисел
        private Random random = new Random();

        public GraphsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// При загрузке страницы
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();  // Получаем фокус для обработки клавиш
            GenerateMaze();
            DrawMaze();
        }

        /// <summary>
        /// При изменении размера Canvas — пересчитываем размер лабиринта
        /// </summary>
        private void MazeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MazeCanvas.ActualWidth > 0)
                mazeWidth = Math.Max(5, (int)(MazeCanvas.ActualWidth / CellSize));
            if (MazeCanvas.ActualHeight > 0)
                mazeHeight = Math.Max(5, (int)(MazeCanvas.ActualHeight / CellSize));
        }

        /// <summary>
        /// Генерация случайного лабиринта с гарантированным путём
        /// </summary>
        private void GenerateMaze()
        {
            maze = new int[mazeHeight, mazeWidth];
            
            // Шаг 1: Заполняем случайными стенами (30% вероятность)
            for (int y = 0; y < mazeHeight; y++)
            {
                for (int x = 0; x < mazeWidth; x++)
                {
                    maze[y, x] = random.NextDouble() < 0.3 ? 1 : 0;
                }
            }
            
            // Шаг 2: Устанавливаем позиции игрока и выхода
            playerPos = new Point(0, 0);
            exitPos = new Point(mazeWidth - 1, mazeHeight - 1);
            
            // Гарантируем, что старт и выход — проходы
            maze[0, 0] = 0;
            maze[mazeHeight - 1, mazeWidth - 1] = 0;
            
            // Шаг 3: Создаём гарантированный путь от старта до выхода
            // Простой алгоритм: идём случайно вправо или вниз
            int px = 0, py = 0;
            while (px < mazeWidth - 1 || py < mazeHeight - 1)
            {
                maze[py, px] = 0;  // Делаем текущую клетку проходом
                
                // Выбираем направление
                if (px >= mazeWidth - 1)
                    py++;  // Дошли до правого края — идём вниз
                else if (py >= mazeHeight - 1)
                    px++;  // Дошли до нижнего края — идём вправо
                else if (random.NextDouble() < 0.5)
                    px++;  // Случайно вправо
                else
                    py++;  // Случайно вниз
            }
            
            StatusText.Text = "Найди выход! 🏁";
        }

        /// <summary>
        /// Отрисовка лабиринта на Canvas
        /// </summary>
        private void DrawMaze()
        {
            MazeCanvas.Children.Clear();
            pathRects.Clear();
            
            // Рисуем все клетки лабиринта
            for (int y = 0; y < mazeHeight; y++)
            {
                for (int x = 0; x < mazeWidth; x++)
                {
                    var cell = new Rectangle
                    {
                        Width = CellSize - 1,
                        Height = CellSize - 1,
                        Fill = new SolidColorBrush(
                            maze[y, x] == 1 
                                ? Color.FromRgb(52, 73, 94)   // Стена — тёмная
                                : Color.FromRgb(44, 62, 80)   // Проход — чуть светлее
                        )
                    };
                    
                    Canvas.SetLeft(cell, x * CellSize);
                    Canvas.SetTop(cell, y * CellSize);
                    MazeCanvas.Children.Add(cell);
                }
            }
            
            // Рисуем выход (зелёный)
            exitRect = new Rectangle
            {
                Width = CellSize - 4,
                Height = CellSize - 4,
                Fill = Brushes.LimeGreen,
                RadiusX = 3,
                RadiusY = 3
            };
            Canvas.SetLeft(exitRect, exitPos.X * CellSize + 2);
            Canvas.SetTop(exitRect, exitPos.Y * CellSize + 2);
            MazeCanvas.Children.Add(exitRect);
            
            // Рисуем игрока (жёлтый)
            playerRect = new Rectangle
            {
                Width = CellSize - 6,
                Height = CellSize - 6,
                Fill = Brushes.Yellow,
                RadiusX = 10,
                RadiusY = 10
            };
            Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3);
            Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);
            MazeCanvas.Children.Add(playerRect);
        }

        /// <summary>
        /// Обработка нажатий клавиш — перемещение игрока
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
                    ShowPath();
                    e.Handled = true;
                    return;
            }
            
            // Проверяем, можно ли переместиться
            if (CanMove((int)newPos.X, (int)newPos.Y))
            {
                ClearPath();  // Убираем подсказку пути
                playerPos = newPos;
                
                // Обновляем позицию игрока на Canvas
                Canvas.SetLeft(playerRect, playerPos.X * CellSize + 3);
                Canvas.SetTop(playerRect, playerPos.Y * CellSize + 3);
                
                // Проверяем победу
                if (playerPos == exitPos)
                {
                    StatusText.Text = "🎉 Победа! Выход найден!";
                }
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// Проверяет, можно ли переместиться в указанную клетку
        /// </summary>
        private bool CanMove(int x, int y)
        {
            // Проверяем границы
            if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight)
                return false;
            
            // Проверяем, не стена ли это
            return maze[y, x] == 0;
        }

        /// <summary>
        /// Показывает кратчайший путь от игрока до выхода
        /// </summary>
        private void ShowPath()
        {
            ClearPath();
            
            // Находим путь с помощью BFS
            var path = FindPathBFS(playerPos, exitPos);
            
            if (path == null)
            {
                StatusText.Text = "❌ Путь не найден!";
                return;
            }
            
            StatusText.Text = $"📍 Путь найден: {path.Count} шагов";
            
            // Визуализируем путь
            foreach (var point in path)
            {
                // Пропускаем позиции игрока и выхода
                if (point == playerPos || point == exitPos)
                    continue;
                
                var pathCell = new Rectangle
                {
                    Width = CellSize - 8,
                    Height = CellSize - 8,
                    Fill = new SolidColorBrush(Color.FromArgb(150, 52, 152, 219)),  // Полупрозрачный синий
                    RadiusX = 5,
                    RadiusY = 5
                };
                
                Canvas.SetLeft(pathCell, point.X * CellSize + 4);
                Canvas.SetTop(pathCell, point.Y * CellSize + 4);
                MazeCanvas.Children.Add(pathCell);
                pathRects.Add(pathCell);
            }
        }

        /// <summary>
        /// BFS — Поиск кратчайшего пути от start до end
        /// Возвращает список точек пути или null если путь не найден
        /// </summary>
        private List<Point>? FindPathBFS(Point start, Point end)
        {
            // Очередь для обхода в ширину (FIFO)
            var queue = new Queue<Point>();
            
            // Словарь: для каждой посещённой клетки храним,
            // откуда мы в неё пришли (для восстановления пути)
            var cameFrom = new Dictionary<Point, Point>();
            
            // Начинаем со стартовой позиции
            queue.Enqueue(start);
            cameFrom[start] = start;  // Старт "пришёл сам из себя"
            
            // Направления движения: вверх, вниз, влево, вправо
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };
            
            // Основной цикл BFS
            while (queue.Count > 0)
            {
                // Берём первую клетку из очереди
                Point current = queue.Dequeue();
                
                // Если достигли цели — восстанавливаем путь
                if (current == end)
                {
                    var path = new List<Point>();
                    Point c = end;
                    
                    // Идём от конца к началу по словарю cameFrom
                    while (cameFrom[c] != c)
                    {
                        path.Add(c);
                        c = cameFrom[c];
                    }
                    path.Add(c);  // Добавляем стартовую точку
                    
                    path.Reverse();  // Разворачиваем: теперь от старта к концу
                    return path;
                }
                
                // Проверяем всех соседей (4 направления)
                for (int i = 0; i < 4; i++)
                {
                    Point next = new Point(current.X + dx[i], current.Y + dy[i]);
                    
                    // Если клетка проходима И ещё не посещена
                    if (CanMove((int)next.X, (int)next.Y) && !cameFrom.ContainsKey(next))
                    {
                        queue.Enqueue(next);       // Добавляем в очередь для проверки
                        cameFrom[next] = current;  // Запоминаем, откуда пришли
                    }
                }
            }
            
            // Очередь пуста, но цель не достигнута — пути нет
            return null;
        }

        /// <summary>
        /// Очищает визуализацию пути
        /// </summary>
        private void ClearPath()
        {
            foreach (var rect in pathRects)
            {
                MazeCanvas.Children.Remove(rect);
            }
            pathRects.Clear();
        }

        /// <summary>
        /// Кнопка "Новый лабиринт"
        /// </summary>
        private void NewMaze_Click(object sender, RoutedEventArgs e)
        {
            GenerateMaze();
            DrawMaze();
            this.Focus();
        }

        /// <summary>
        /// Кнопка "Показать путь"
        /// </summary>
        private void ShowPath_Click(object sender, RoutedEventArgs e)
        {
            ShowPath();
            this.Focus();
        }
    }
}
