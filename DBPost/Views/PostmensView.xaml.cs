using DBPost.AddEditWindow;
using DBPost.Windows;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DBPost.Views
{
    // Представление для работы с почтальонами
    public partial class PostmensView : UserControl
    {
        // Строка подключения к базе данных из конфигурационного файла
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        // Переменная для хранения списка почтальонов (используется при поиске)
        private IEnumerable? postMens;
        public PostmensView()
        {
            InitializeComponent();
            FillDataGrid();
        }
        
        // Метод загрузки данных почтальонов из базы данных
        public void FillDataGrid()
        {
            using (SqlConnection con = new SqlConnection(ConString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * From Postmen", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Postmen");
                sda.Fill(dt);
                PostmensDataGrid.ItemsSource = dt.DefaultView;
                postMens = PostmensDataGrid.ItemsSource;
            }
        }

        // Удаление выбранного почтальона
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDPostmen"].ToString()!;
                bool hasPostmen = false;
                // Проверка, обслуживает ли почтальон кого-то из подписчиков
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand($"SELECT * From Subscribers where FKPostmen = {id}", con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Subscribers");
                    sda.Fill(dt);
                    hasPostmen = dt.DefaultView.Count > 0;
                }
                if (hasPostmen)
                    MessageWindow.Show("Информационное окно", "Перед удалением почтальона необходимо удалить всех подписчиков которых он обслуживает.", MessageBoxButton.OK);
                else
                {
                    int rowsAffected;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        SqlCommand cmd = new SqlCommand("Delete from Postmen where IDPostmen=" + id, con);
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

        // Открытие окна редактирования существующего почтальона
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            postmen periodicalsWindow = new postmen(this, ((DataRowView)((Button)sender).Tag));
            periodicalsWindow.TitleTextBlock.Text = "Изменить";
            periodicalsWindow.AddButton.Content = "Сохранить";

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(periodicalsWindow);
        }

        // Поиск почтальона по ФИО
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
            {
                SearchTextBlock.Visibility = Visibility.Hidden;
                SearchIcon.Visibility = Visibility.Hidden;
                PostmensDataGrid.ItemsSource = postMens!.Cast<object>().ToList().Where<object>(o => (o as DataRowView)!.Row["FIO"].ToString()!.ToLower().Contains(SearchTextBox.Text.ToLower()));
            }
            else
            {
                SearchTextBlock.Visibility = Visibility.Visible;
                SearchIcon.Visibility = Visibility.Visible;
                PostmensDataGrid.ItemsSource = postMens;
            }
        }

        // Добавление нового почтальона
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            postmen postmanC = new postmen(this);
            
            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(postmanC);
        }

        // Метод для повторного включения интерфейса после закрытия дочернего окна
        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
