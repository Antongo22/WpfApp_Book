using System.Windows;
using WpfApp_Book.Chapters.Chapter1_Introduction.Demo;
using WpfApp_Book.Chapters.Chapter2_PagesAndWindows.Demo;
using WpfApp_Book.Chapters.Chapter3_Keyboard.Demo;
using WpfApp_Book.Chapters.Chapter4_Layout.Demo;
using WpfApp_Book.Chapters.Chapter5_Animation.Demo;
using WpfApp_Book.Chapters.Chapter6_Movement.Demo;
using WpfApp_Book.Chapters.Chapter7_Graphs.Demo;
using WpfApp_Book.Chapters.Chapter8_Final.Demo;

namespace WpfApp_Book
{
    /// <summary>
    /// Главное окно приложения - Учебник WPF
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Загружаем страницы всех глав
            Chapter1Frame.Navigate(new IntroductionPage());
            Chapter2Frame.Navigate(new PagesAndWindowsPage());
            Chapter3Frame.Navigate(new KeyboardPage());
            Chapter4Frame.Navigate(new LayoutPage());
            Chapter5Frame.Navigate(new AnimationPage());
            Chapter6Frame.Navigate(new MovementPage());
            Chapter7Frame.Navigate(new GraphsPage());
            Chapter8Frame.Navigate(new FinalPage());
        }
    }
}
