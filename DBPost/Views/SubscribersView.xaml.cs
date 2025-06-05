using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using DBPost.AddEditWindow;
using DBPost.Windows;

namespace DBPost.Views
{
    // Представление (UserControl) для работы с подписчиками
    public partial class SubscribersView : UserControl
    {
        // Получаем строку подключения из конфигурационного файла
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? subscribers; // Список подписчиков (хранится для фильтрации)
        public SubscribersView()
        {
            InitializeComponent();
            FillDataGrid();
        }

        // Метод для заполнения DataGrid данными из таблицы Subscribers
        public void FillDataGrid()
        {
            
            string CmdString = string.Empty;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * From Subscribers", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Subscribers");
                sda.Fill(dt);
                SubscribersDataGrid.ItemsSource = dt.DefaultView;
                subscribers = SubscribersDataGrid.ItemsSource;
            }
        }

        // Обработчик изменения текста в поле поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
            {
                SearchTextBlock.Visibility = Visibility.Hidden;
                SearchIcon.Visibility = Visibility.Hidden;
                SubscribersDataGrid.ItemsSource = subscribers!.Cast<object>().ToList().Where<object>(o => (o as DataRowView)!.Row["FIO"].ToString()!.ToLower().Contains(SearchTextBox.Text.ToLower()));
            }
            else
            {
                SearchTextBlock.Visibility = Visibility.Visible;
                SearchIcon.Visibility = Visibility.Visible;
                SubscribersDataGrid.ItemsSource = subscribers;
            }
        }

        // Обработчик нажатия кнопки удаления подписчика
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDSubscriber"].ToString()!;
                bool HasSubsc = false;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand($"SELECT * From Subscriptions where FKSubscriber = {id}", con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Subscriptions");
                    sda.Fill(dt);
                    HasSubsc = dt.DefaultView.Count > 0;
                }
                if (HasSubsc)
                    MessageWindow.Show("Информационное окно", "Перед удалением подписчика необходимо удалить все его подписки.", MessageBoxButton.OK);
                else
                {
                    int rowsAffected;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        SqlCommand cmd = new SqlCommand("Delete from Subscribers where IDSubscriber=" + id, con);
                        con.Open();
                        rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    if (rowsAffected > 0)
                    {
                        MessageWindow.Show("Информационное окно", "Успешно удалено", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageWindow.Show("Информационное окно", "Произошла ошибка, попробуйте снова", MessageBoxButton.OK);
                    }
                    FillDataGrid();
                }
                
            }
        }

        // Обработчик кнопки редактирования подписчика
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            subscribers subscriberWindow = new subscribers(this, ((DataRowView)((Button)sender).Tag));
            subscriberWindow.Title.Text = "Изменить";
            subscriberWindow.AddButton.Content = "Сохранить";

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(subscriberWindow);
        }

        // Обработчик кнопки добавления нового подписчика
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            subscribers ubscriberWindow = new subscribers(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(ubscriberWindow);
        }

        // Метод для повторного включения меню и активного окна после закрытия окна добавления/редактирования
        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
