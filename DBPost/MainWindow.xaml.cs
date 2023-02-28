using DBPost.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBPost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserControl? currentControl;
        public UserControl? CurrentControl 
        {
            get { return currentControl; }
            set
            {
                currentControl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentControl))); 
            } 
        }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext= this;
            CurrentControl = new SubscribersView();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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
