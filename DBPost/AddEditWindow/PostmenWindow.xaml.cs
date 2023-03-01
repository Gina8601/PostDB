using DBPost.Views;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Text.RegularExpressions;
using DBPost.Windows;
using System.Windows.Controls.Primitives;

namespace DBPost.AddEditWindow
{
    /// <summary>
    /// Логика взаимодействия для postmen.xaml
    /// </summary>
    public partial class postmen : UserControl
    {
        private PostmensView PostmensView { get; set; }
        bool isAdding = true;
        string? idPostment = string.Empty;
        public postmen(PostmensView postmensView)
        {
            InitializeComponent();
            PostmensView= postmensView;
        }

        public postmen(PostmensView postmensView, DataRowView dataRowView)
        {
          InitializeComponent();
            isAdding = false;
            PostmensView = postmensView;
            idPostment = dataRowView.Row["IDPostmen"].ToString();
            this.FIO.Text = dataRowView.Row["FIO"].ToString();
            this.Address.Text = dataRowView.Row["Address"].ToString();
            this.PhoneNumber.Text = dataRowView.Row["PhoneNumber"].ToString();
        }

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

        private void Text_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^a-zA-Z]+")) e.Handled = true;
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^0-9+\-()]+")) e.Handled = true;
        }

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

        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ValidatePostMen()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
