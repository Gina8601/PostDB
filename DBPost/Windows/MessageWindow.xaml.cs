using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DBPost.Windows
{
    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        MessageBoxResult Result = MessageBoxResult.None;
        public MessageWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                this.DragMove();
            }
        }

        private void ButtonCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.Close();
        }

        void AddButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK);
                    AddButton("Отмена", MessageBoxResult.Cancel, isCancel: true);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Да", MessageBoxResult.Yes);
                    AddButton("Нет", MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Да", MessageBoxResult.Yes);
                    AddButton("Нет", MessageBoxResult.No);
                    AddButton("Отмена", MessageBoxResult.Cancel, isCancel: true);
                    break;
                default:
                    throw new ArgumentException("Unknown button value", "buttons");
            }
        }

        void AddButton(string text, MessageBoxResult result, bool isCancel = false)
        {
            var button = new Button() { Content = text, IsCancel = isCancel };
            button.Click += (o, args) => { Result = result; DialogResult = true; };
            ButtonContainer.Children.Add(button);
        }
        public static MessageBoxResult Show(string caption, string message, MessageBoxButton buttons)
        {
            MessageWindow dialog = new() { Title = caption };
            dialog.messageTextBox.Text = message;
            dialog.AddButtons(buttons);
            dialog.ShowDialog();
            return dialog.Result;
        }
        public static MessageBoxResult Show(string caption, string message)
        {
            MessageWindow dialog = new() { Title = caption };
            dialog.messageTextBox.Text = message;
            dialog.AddButtons(MessageBoxButton.OK);
            dialog.ShowDialog();
            return dialog.Result;
        }
        public static MessageBoxResult Show(string message)
        {
            MessageWindow dialog = new();
            dialog.messageTextBox.Text = message;
            dialog.AddButtons(MessageBoxButton.OK);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
