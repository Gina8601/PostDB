using DBPost.AddEditWindow;
using DBPost.Windows;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

namespace DBPost.Views
{
    /// <summary>
    /// Логика взаимодействия для PostmensView.xaml
    /// </summary>
    public partial class PostmensView : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? postMens;
        public PostmensView()
        {
            InitializeComponent();
            FillDataGrid();
        }

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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDPostmen"].ToString()!;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            postmen postmanC = new postmen(this);
            
            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(postmanC);
        }

        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
