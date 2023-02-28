using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
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
using System.Collections;
using DBPost.AddEditWindow;
using DBPost.Windows;

namespace DBPost.Views
{
    /// <summary>
    /// Логика взаимодействия для SubscribersView.xaml
    /// </summary>
    public partial class SubscribersView : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? subscribers;
        public SubscribersView()
        {
            InitializeComponent();
            FillDataGrid();
        }

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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDSubscriber"].ToString()!;
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            subscribers ubscriberWindow = new subscribers(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(ubscriberWindow);
        }

        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
