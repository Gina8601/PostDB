using DBPost.Views;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Configuration;
using System.Text.RegularExpressions;
using DBPost.Windows;
using System.Windows.Controls.Primitives;

namespace DBPost.AddEditWindow
{
    public partial class postmen : UserControl
    {
        private PostmensView PostmensView { get; set; }
        bool isAdding = true; // Флаг режима: добавление(true) или редактирование(false)
        string? idPostment = string.Empty; //// ID почтальона, если редактируем существующую запись
        public postmen(PostmensView postmensView)
        {
            InitializeComponent();
            PostmensView= postmensView;
        }
       
        // Конструктор для редактирования существующего почтальона с заполнением полей
        public postmen(PostmensView postmensView, DataRowView dataRowView)
        {
          InitializeComponent();
            isAdding = false; // Устанавливаем режим редактирования
            PostmensView = postmensView;
            idPostment = dataRowView.Row["IDPostmen"].ToString();
            this.FIO.Text = dataRowView.Row["FIO"].ToString();
            this.Address.Text = dataRowView.Row["Address"].ToString();
            this.PhoneNumber.Text = dataRowView.Row["PhoneNumber"].ToString();
        }

        // Обработчик нажатия кнопки Добавить/Сохранить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender as Button == AddButton && !ValidatePostMen()) return;

            this.IsEnabled = false;
            (FindResource("CloseMenu") as Storyboard)!.Completed += (s, a) => {
                (this.Parent as Grid)!.Children.Remove(this);
                PostmensView.EnableWindow();
                if((sender as Button) == AddButton)
                {
                    string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand cmd;
                        if (isAdding)
                        {
                            cmd = new SqlCommand($"INSERT INTO Postmen (FIO, Address, PhoneNumber) Values ('{FIO.Text}','{Address.Text}','{PhoneNumber.Text}')", con);
                        }
                        else
                        {
                            cmd = new SqlCommand($"Update Postmen set FIO='{FIO.Text}', Address='{Address.Text}',PhoneNumber = '{PhoneNumber.Text}' where IDPostmen={idPostment}", con);
                        }
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    PostmensView.FillDataGrid();
                }
            };
        }

        // Обработчик события обновления разметки — запускает анимацию открытия окна
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            (FindResource("OpenMenu") as Storyboard)!.Completed += (s, ev) =>
            {
                this.LayoutUpdated -= UserControl_LayoutUpdated!;
                this.IsEnabled = true;
            };
            (FindResource("OpenMenu") as Storyboard)!.Begin();
        }

        // Ограничение ввода для поля ФИО — разрешены только буквы (латиница и кириллица)
        private void Text_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-ЯёЁ]+$")) e.Handled = true;
        }

        // Ограничение ввода для номера телефона — разрешены цифры, +, -, скобки
        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^0-9+\-()]+")) e.Handled = true;
        }

        // Валидация данных перед сохранением — проверяем заполнение всех полей
        private bool ValidatePostMen()
        {
            if (FIO.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите имя!", MessageBoxButton.OK);
                return false;
            }
            if (Address.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите адрес!", MessageBoxButton.OK);
                return false;
            }
            if (PhoneNumber.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите номер телефона!", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        // Обработчик нажатия мыши на кнопку добавления — запускает проверку и событие Click
        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ValidatePostMen()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }

}
