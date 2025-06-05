using DBPost.Views;
using DBPost.Windows;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DBPost.AddEditWindow
{
    public partial class subscribers : UserControl
    {
        // Строка подключения к базе данных из конфигурационного файла
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private SubscribersView SubscribersView { get; set; }
        bool isAdding = true; // Флаг режима: добавление (true) или редактирование (false)
        string? idSubscriber = string.Empty;

        // Конструктор для добавления нового подписчика
        public subscribers (SubscribersView subscribersView)
        {
            InitializeComponent();
            SubscribersView = subscribersView;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                con.Open();
                SqlDataAdapter adapter = new("Select * from Postmen", con);
                DataTable table = new("Postmen");
                adapter.Fill(table);
                FKPostmen.ItemsSource = table.DefaultView;
                FKPostmen.DisplayMemberPath = "FIO";
                con.Close();
            }
        }

        // Конструктор для редактирования существующего подписчика
        public subscribers(SubscribersView subscribersView, DataRowView dataRowView)
        {
            InitializeComponent();
            isAdding= false;
            SubscribersView = subscribersView;
            this.idSubscriber = dataRowView.Row["IDSubscriber"].ToString();
            this.FIO.Text = dataRowView.Row["FIO"].ToString();
            this.Address.Text = dataRowView.Row["Address"].ToString();
            this.PhoneNumber.Text = dataRowView.Row["PhoneNumber"].ToString();
            using (SqlConnection con = new SqlConnection(ConString))
            {

                SqlDataAdapter adapter = new("Select * from Postmen", con);
                DataTable table = new("Postmen");
                adapter.Fill(table);
                FKPostmen.ItemsSource = table.DefaultView;
                FKPostmen.DisplayMemberPath = "FIO";
                con.Open();
                SqlCommand cmd = new("Select FIO from Postmen where IDPostmen=" + dataRowView.Row["FKPostmen"], con);
                this.FKPostmen.Text = cmd.ExecuteScalar().ToString();
                con.Close();
            }
        }

        // Обработчик нажатия кнопки (Добавить / Сохранить)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            (FindResource("CloseMenu") as Storyboard)!.Completed += (s, a) => {
                (this.Parent as Grid)!.Children.Remove(this);
                SubscribersView.EnableWindow();
                if ((sender as Button) == AddButton)
                {
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand cmd1 = new($"Select IDPostmen from Postmen where FIO='{FKPostmen.Text}'", con);
                        string id = cmd1.ExecuteScalar().ToString()!;
                        //MessageBox.Show(id);
                        SqlCommand cmd;
                        if (isAdding) 
                        { 
                            cmd = new SqlCommand($"INSERT INTO Subscribers (FIO, Address, PhoneNumber, FKPostmen) Values ('{FIO.Text}','{Address.Text}','{PhoneNumber.Text}', {id})", con); 
                        }
                        else
                        {
                            cmd = new SqlCommand($"Update Subscribers Set FIO='{FIO.Text}', Address='{Address.Text}', PhoneNumber='{PhoneNumber.Text}', FKPostmen={id} where IDSubscriber={idSubscriber}", con);
                        }
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    SubscribersView.FillDataGrid();
                }
            };

        }

        // Обработчик обновления разметки — запускает анимацию открытия окна
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

        // Ограничение ввода для текстовых полей (ФИО и т.п.) — разрешаем только буквы
        private void Text_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-ЯёЁ]+$")) e.Handled = true;
        }

        // Ограничение ввода для номера телефона — разрешаем цифры, +, -, скобки
        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^0-9+\-()]+")) e.Handled = true;
        }

        // Проверка корректности заполнения всех обязательных полей перед сохранением
        private bool ValidateSubscriber()
        {
            if(FIO.Text.Length < 1)
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
            if (FKPostmen.SelectedIndex < 0)
            {
                MessageWindow.Show("Ошибка ввода", "Выберите почтальона!", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        // Обработчик нажатия мыши на кнопку добавления — сначала проверяем валидацию, затем вызываем Click
        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            if (ValidateSubscriber()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
