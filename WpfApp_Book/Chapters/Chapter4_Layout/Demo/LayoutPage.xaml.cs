using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp_Book.Chapters.Chapter4_Layout.Demo
{
    public partial class LayoutPage : Page
    {
        // Флаг: идёт ли перетаскивание
        private bool isDragging = false;
        
        // Смещение точки клика от левого верхнего угла элемента
        private Point clickOffset;
        
        // Ссылка на перетаскиваемый элемент
        private UIElement? draggedElement = null;

        public LayoutPage()
        {
            InitializeComponent();
            UpdateDistance();
        }

        /// <summary>
        /// Начало перетаскивания - вызывается при нажатии кнопки мыши
        /// </summary>
        private void Box_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            if (element == null) return;
            
            // Запоминаем какой элемент перетаскиваем
            draggedElement = element;
            isDragging = true;
            
            // Запоминаем где именно кликнули внутри элемента
            // Это нужно чтобы элемент не "прыгал" при начале перетаскивания
            clickOffset = e.GetPosition(element);
            
            // ВАЖНО: Захватываем мышь
            // Без этого при быстром движении курсор может "убежать" от элемента
            // и события MouseMove/MouseUp перестанут приходить
            element.CaptureMouse();
        }

        /// <summary>
        /// Перемещение - вызывается при движении мыши
        /// </summary>
        private void Box_MouseMove(object sender, MouseEventArgs e)
        {
            // Если не в режиме перетаскивания - ничего не делаем
            if (!isDragging || draggedElement == null) return;
            
            // Получаем позицию мыши относительно Canvas
            Point mousePos = e.GetPosition(DemoCanvas);
            
            // Вычисляем новую позицию элемента
            // Учитываем смещение клика, чтобы элемент не "прыгал"
            double newX = mousePos.X - clickOffset.X;
            double newY = mousePos.Y - clickOffset.Y;
            
            // Ограничиваем область перемещения границами Canvas
            var fe = draggedElement as FrameworkElement;
            if (fe != null)
            {
                double maxX = DemoCanvas.ActualWidth - fe.ActualWidth;
                double maxY = DemoCanvas.ActualHeight - fe.ActualHeight;
                newX = Math.Clamp(newX, 0, Math.Max(0, maxX));
                newY = Math.Clamp(newY, 0, Math.Max(0, maxY));
            }
            
            // Устанавливаем новую позицию
            Canvas.SetLeft(draggedElement, newX);
            Canvas.SetTop(draggedElement, newY);
            
            // Обновляем информацию о расстоянии и координатах
            UpdateDistance();
        }

        /// <summary>
        /// Окончание перетаскивания - вызывается при отпускании кнопки мыши
        /// </summary>
        private void Box_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedElement != null)
            {
                // Освобождаем захват мыши
                draggedElement.ReleaseMouseCapture();
            }
            isDragging = false;
            draggedElement = null;
        }

        /// <summary>
        /// Обновление отображения расстояния между элементами
        /// </summary>
        private void UpdateDistance()
        {
            // Вычисляем центры обоих квадратов
            double x1 = Canvas.GetLeft(BlueBox) + BlueBox.ActualWidth / 2;
            double y1 = Canvas.GetTop(BlueBox) + BlueBox.ActualHeight / 2;
            double x2 = Canvas.GetLeft(RedBox) + RedBox.ActualWidth / 2;
            double y2 = Canvas.GetTop(RedBox) + RedBox.ActualHeight / 2;
            
            // Используем размеры по умолчанию, если ActualWidth ещё не определён
            if (BlueBox.ActualWidth == 0)
            {
                x1 = Canvas.GetLeft(BlueBox) + 30;
                y1 = Canvas.GetTop(BlueBox) + 30;
                x2 = Canvas.GetLeft(RedBox) + 30;
                y2 = Canvas.GetTop(RedBox) + 30;
            }
            
            // Формула расстояния: √((x₂-x₁)² + (y₂-y₁)²)
            double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            
            DistanceText.Text = $"Расстояние A↔B: {distance:F1} px";
            CoordinatesText.Text = $"A: ({Canvas.GetLeft(BlueBox):F0}, {Canvas.GetTop(BlueBox):F0})  " +
                                   $"B: ({Canvas.GetLeft(RedBox):F0}, {Canvas.GetTop(RedBox):F0})";
        }
    }
}
