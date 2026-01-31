using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp_Book.Chapters.Chapter8_Final.Answers
{
    public partial class PacManAnswer : Page
    {
        private enum Direction { None, Up, Down, Left, Right }
        
        private const int CellSize = 24;
        private int mapWidth = 19;
        private int mapHeight = 15;
        
        // 0 = путь, 1 = стена, 2 = точка, 3 = power-up
        private int[,] map = null!;
        
        private DispatcherTimer gameTimer = null!;
        private DispatcherTimer ghostTimer = null!;
        private Random random = new Random();
        
        // Pac-Man
        private Point pacmanPos;
        private Direction pacmanDir = Direction.None;
        private Direction nextDir = Direction.None;
        private Ellipse pacmanElement = null!;
        
        // Призраки
        private List<Point> ghostPositions = new List<Point>();
        private List<Ellipse> ghostElements = new List<Ellipse>();
        private Color[] ghostColors = { Colors.Red, Colors.Cyan, Colors.Pink, Colors.Orange };
        
        // Точки
        private List<Ellipse> dotElements = new List<Ellipse>();
        private int totalDots = 0;
        private int collectedDots = 0;
        
        // Состояние игры
        private int score = 0;
        private int lives = 3;
        private bool isRunning = false;
        private bool isPoweredUp = false;
        private int powerUpTicks = 0;

        public PacManAnswer()
        {
            InitializeComponent();
            
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            gameTimer.Tick += GameLoop;
            
            ghostTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            ghostTimer.Tick += MoveGhosts;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            InitGame();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            ghostTimer.Stop();
        }

        /// <summary>
        /// Инициализация игры
        /// </summary>
        private void InitGame()
        {
            gameTimer.Stop();
            ghostTimer.Stop();
            isRunning = false;
            
            GameCanvas.Children.Clear();
            dotElements.Clear();
            ghostElements.Clear();
            ghostPositions.Clear();
            
            score = 0;
            lives = 3;
            collectedDots = 0;
            isPoweredUp = false;
            pacmanDir = Direction.None;
            nextDir = Direction.None;
            
            ScoreText.Text = "0";
            LivesText.Text = "❤❤❤";
            StatusText.Text = "Нажмите СТАРТ!";
            StartButton.Content = "▶ СТАРТ";
            
            CreateMap();
            DrawMap();
            SpawnPacman();
            SpawnGhosts();
        }

        /// <summary>
        /// Создание карты лабиринта
        /// </summary>
        private void CreateMap()
        {
            map = new int[mapHeight, mapWidth];
            
            // Базовый шаблон лабиринта
            string[] template = {
                "1111111111111111111",
                "1222222121222212221",
                "1211121121211211211",
                "1222222222222222221",
                "1211111211211111211",
                "1222221222222122221",
                "1111121111111211111",
                "0000121222222120000",
                "1111121211112121111",
                "1222222212212222221",
                "1211111211211111211",
                "1212222222222222121",
                "1211211111111121211",
                "1222212222222212221",
                "1111111111111111111"
            };
            
            totalDots = 0;
            for (int y = 0; y < mapHeight && y < template.Length; y++)
            {
                for (int x = 0; x < mapWidth && x < template[y].Length; x++)
                {
                    map[y, x] = template[y][x] - '0';
                    if (map[y, x] == 2) totalDots++;
                }
            }
            
            // Добавляем power-up'ы в углах
            if (mapHeight > 2 && mapWidth > 2)
            {
                map[1, 1] = 3;
                map[1, mapWidth - 2] = 3;
                map[mapHeight - 2, 1] = 3;
                map[mapHeight - 2, mapWidth - 2] = 3;
            }
        }

        /// <summary>
        /// Отрисовка карты
        /// </summary>
        private void DrawMap()
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (map[y, x] == 1)
                    {
                        // Стена
                        var wall = new Rectangle
                        {
                            Width = CellSize,
                            Height = CellSize,
                            Fill = new SolidColorBrush(Color.FromRgb(33, 33, 222))
                        };
                        Canvas.SetLeft(wall, x * CellSize);
                        Canvas.SetTop(wall, y * CellSize);
                        GameCanvas.Children.Add(wall);
                    }
                    else if (map[y, x] == 2)
                    {
                        // Точка
                        var dot = new Ellipse
                        {
                            Width = 6,
                            Height = 6,
                            Fill = Brushes.White
                        };
                        Canvas.SetLeft(dot, x * CellSize + CellSize / 2 - 3);
                        Canvas.SetTop(dot, y * CellSize + CellSize / 2 - 3);
                        GameCanvas.Children.Add(dot);
                        dotElements.Add(dot);
                    }
                    else if (map[y, x] == 3)
                    {
                        // Power-up
                        var power = new Ellipse
                        {
                            Width = 14,
                            Height = 14,
                            Fill = Brushes.White
                        };
                        Canvas.SetLeft(power, x * CellSize + CellSize / 2 - 7);
                        Canvas.SetTop(power, y * CellSize + CellSize / 2 - 7);
                        GameCanvas.Children.Add(power);
                        dotElements.Add(power);
                    }
                }
            }
        }

        private void SpawnPacman()
        {
            pacmanPos = new Point(9, 7);
            
            pacmanElement = new Ellipse
            {
                Width = CellSize - 4,
                Height = CellSize - 4,
                Fill = Brushes.Yellow
            };
            Canvas.SetLeft(pacmanElement, pacmanPos.X * CellSize + 2);
            Canvas.SetTop(pacmanElement, pacmanPos.Y * CellSize + 2);
            GameCanvas.Children.Add(pacmanElement);
        }

        private void SpawnGhosts()
        {
            Point[] ghostStarts = { new Point(8, 7), new Point(9, 6), new Point(10, 7), new Point(9, 8) };
            
            for (int i = 0; i < 4; i++)
            {
                var ghost = new Ellipse
                {
                    Width = CellSize - 4,
                    Height = CellSize - 4,
                    Fill = new SolidColorBrush(ghostColors[i])
                };
                
                Point pos = ghostStarts[i];
                ghostPositions.Add(pos);
                ghostElements.Add(ghost);
                
                Canvas.SetLeft(ghost, pos.X * CellSize + 2);
                Canvas.SetTop(ghost, pos.Y * CellSize + 2);
                GameCanvas.Children.Add(ghost);
            }
        }

        /// <summary>
        /// Обработка клавиатуры
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    nextDir = Direction.Up;
                    break;
                case Key.Down:
                case Key.S:
                    nextDir = Direction.Down;
                    break;
                case Key.Left:
                case Key.A:
                    nextDir = Direction.Left;
                    break;
                case Key.Right:
                case Key.D:
                    nextDir = Direction.Right;
                    break;
                case Key.Space:
                    Start_Click(sender, e);
                    break;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Игровой цикл
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            // Power-up таймер
            if (isPoweredUp)
            {
                powerUpTicks--;
                if (powerUpTicks <= 0)
                {
                    isPoweredUp = false;
                    foreach (var ghost in ghostElements)
                        ghost.Fill = new SolidColorBrush(ghostColors[ghostElements.IndexOf(ghost)]);
                }
            }
            
            // Пробуем применить следующее направление
            if (nextDir != Direction.None && CanMove(GetNextPos(pacmanPos, nextDir)))
            {
                pacmanDir = nextDir;
            }
            
            // Двигаем Pac-Man
            if (pacmanDir != Direction.None)
            {
                Point nextPos = GetNextPos(pacmanPos, pacmanDir);
                if (CanMove(nextPos))
                {
                    pacmanPos = nextPos;
                    Canvas.SetLeft(pacmanElement, pacmanPos.X * CellSize + 2);
                    Canvas.SetTop(pacmanElement, pacmanPos.Y * CellSize + 2);
                    
                    CheckDotCollision();
                    CheckGhostCollision();
                }
            }
        }

        private Point GetNextPos(Point pos, Direction dir)
        {
            return dir switch
            {
                Direction.Up => new Point(pos.X, pos.Y - 1),
                Direction.Down => new Point(pos.X, pos.Y + 1),
                Direction.Left => new Point(pos.X - 1, pos.Y),
                Direction.Right => new Point(pos.X + 1, pos.Y),
                _ => pos
            };
        }

        private bool CanMove(Point pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            return x >= 0 && y >= 0 && x < mapWidth && y < mapHeight && map[y, x] != 1;
        }

        private void CheckDotCollision()
        {
            int x = (int)pacmanPos.X;
            int y = (int)pacmanPos.Y;
            
            if (map[y, x] == 2)
            {
                map[y, x] = 0;
                score += 10;
                collectedDots++;
                RemoveDotAt(x, y);
                ScoreText.Text = score.ToString();
                
                if (collectedDots >= totalDots)
                {
                    Win();
                }
            }
            else if (map[y, x] == 3)
            {
                map[y, x] = 0;
                score += 50;
                RemoveDotAt(x, y);
                ScoreText.Text = score.ToString();
                
                // Активируем power-up
                isPoweredUp = true;
                powerUpTicks = 50;
                foreach (var ghost in ghostElements)
                    ghost.Fill = Brushes.Blue;
            }
        }

        private void RemoveDotAt(int x, int y)
        {
            double dotX = x * CellSize + CellSize / 2;
            double dotY = y * CellSize + CellSize / 2;
            
            for (int i = dotElements.Count - 1; i >= 0; i--)
            {
                double dx = Canvas.GetLeft(dotElements[i]) + dotElements[i].Width / 2;
                double dy = Canvas.GetTop(dotElements[i]) + dotElements[i].Height / 2;
                
                if (Math.Abs(dx - dotX) < CellSize && Math.Abs(dy - dotY) < CellSize)
                {
                    GameCanvas.Children.Remove(dotElements[i]);
                    dotElements.RemoveAt(i);
                    break;
                }
            }
        }

        private void CheckGhostCollision()
        {
            for (int i = 0; i < ghostPositions.Count; i++)
            {
                if (ghostPositions[i] == pacmanPos)
                {
                    if (isPoweredUp)
                    {
                        // Съедаем призрака
                        score += 200;
                        ScoreText.Text = score.ToString();
                        ghostPositions[i] = new Point(9, 7);
                        Canvas.SetLeft(ghostElements[i], 9 * CellSize + 2);
                        Canvas.SetTop(ghostElements[i], 7 * CellSize + 2);
                    }
                    else
                    {
                        LoseLife();
                    }
                }
            }
        }

        /// <summary>
        /// Движение призраков (простой ИИ с BFS)
        /// </summary>
        private void MoveGhosts(object? sender, EventArgs e)
        {
            for (int i = 0; i < ghostPositions.Count; i++)
            {
                Point ghostPos = ghostPositions[i];
                Point target = isPoweredUp 
                    ? new Point(random.Next(mapWidth), random.Next(mapHeight)) // Убегают
                    : pacmanPos; // Преследуют
                
                // Простой BFS для первого шага
                var path = FindPathBFS(ghostPos, target);
                if (path != null && path.Count > 1)
                {
                    ghostPositions[i] = path[1];
                }
                else
                {
                    // Случайное движение если путь не найден
                    var dirs = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                    foreach (var dir in dirs)
                    {
                        var next = GetNextPos(ghostPos, dir);
                        if (CanMove(next))
                        {
                            ghostPositions[i] = next;
                            break;
                        }
                    }
                }
                
                Canvas.SetLeft(ghostElements[i], ghostPositions[i].X * CellSize + 2);
                Canvas.SetTop(ghostElements[i], ghostPositions[i].Y * CellSize + 2);
            }
            
            CheckGhostCollision();
        }

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
                    if (CanMove(next) && !cameFrom.ContainsKey(next))
                    {
                        queue.Enqueue(next);
                        cameFrom[next] = current;
                    }
                }
            }
            return null;
        }

        private void LoseLife()
        {
            lives--;
            LivesText.Text = new string('❤', lives);
            
            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                // Респавн
                pacmanPos = new Point(9, 7);
                pacmanDir = Direction.None;
                nextDir = Direction.None;
                Canvas.SetLeft(pacmanElement, 9 * CellSize + 2);
                Canvas.SetTop(pacmanElement, 7 * CellSize + 2);
            }
        }

        private void GameOver()
        {
            gameTimer.Stop();
            ghostTimer.Stop();
            isRunning = false;
            StatusText.Text = "GAME OVER";
            StartButton.Content = "▶ СТАРТ";
            MessageBox.Show($"Игра окончена!\n\nСчёт: {score}", "Game Over", MessageBoxButton.OK);
        }

        private void Win()
        {
            gameTimer.Stop();
            ghostTimer.Stop();
            isRunning = false;
            StatusText.Text = "🎉 ПОБЕДА!";
            StartButton.Content = "▶ СТАРТ";
            MessageBox.Show($"Поздравляем! Вы выиграли!\n\nСчёт: {score}", "Победа!", MessageBoxButton.OK);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                gameTimer.Stop();
                ghostTimer.Stop();
                StartButton.Content = "▶ СТАРТ";
                StatusText.Text = "Пауза";
            }
            else
            {
                gameTimer.Start();
                ghostTimer.Start();
                StartButton.Content = "⏸ ПАУЗА";
                StatusText.Text = "Собирайте точки!";
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
