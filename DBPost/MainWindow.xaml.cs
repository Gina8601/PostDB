using DBPost.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DBPost
{
    // Главный класс окна приложения, реализующий интерфейс уведомления об изменении свойств
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserControl? currentControl; // Текущий отображаемый пользовательский контрол
        // Свойство с уведомлением об изменении, чтобы UI обновлялся при смене контрола
        public UserControl? CurrentControl 
        {
            get { return currentControl; }
            set
            {
                currentControl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentControl))); 
            } 
        }

        // Конструктор главного окна
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext= this;
            CurrentControl = new SubscribersView();
        }

        // Обработчик движения мыши по окну — для возможности перемещения окна при зажатой левой кнопке
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Обработчик нажатия кнопки закрытия окна
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Ниже идут обработчики для переключения отображаемого контрола при выборе соответствующей радио-кнопки
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentControl = new SubscribersView();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            CurrentControl = new PostmensView();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            CurrentControl = new PeriodicalsView();
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            CurrentControl = new SubscriptionsView();
        }
    }
}
