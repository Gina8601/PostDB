using DBPost.Views;
using DBPost.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBPost.AddEditWindow
{
    /// <summary>
    /// Логика взаимодействия для subscribers.xaml
    /// </summary>
    public partial class subscribers : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private SubscribersView SubscribersView { get; set; }
        bool isAdding = true;
        string? idSubscriber = string.Empty;
        public subscribers (SubscribersView subscribersView)
        {
            InitializeComponent();
            SubscribersView = subscribersView;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                con.Open();
                SqlDataAdapter adapter = new ($"Select* from" +
                    $"(" +
                    $"Select p.IDPostmen, p.FIO, count(s.FKPostmen) as c from Postmen as p " +
                    $"inner join " +
                    $"Subscribers as s on s.FKPostmen = p.IDPostmen group by p.IDPostmen, p.FIO, s.FKPostmen " +
                    $"Union " +
                    $"Select IDPostmen, FIO, 0 as c from Postmen where IDPostmen not in (Select FKPostmen from Subscribers)) as u " +
                    $"where u.c < (Select avg(FKPostmen) from Subscribers)", con);
                DataTable table = new("Postmen");
                adapter.Fill(table);
                FKPostmen.ItemsSource = table.DefaultView;
                FKPostmen.DisplayMemberPath = "FIO";
                con.Close();
            }
        }
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

                SqlDataAdapter adapter = new($"Select* from" +
                    $"(" +
                    $"Select p.IDPostmen, p.FIO, count(s.FKPostmen) as c from Postmen as p " +
                    $"inner join " +
                    $"Subscribers as s on s.FKPostmen = p.IDPostmen group by p.IDPostmen, p.FIO, s.FKPostmen " +
                    $"Union " +
                    $"Select IDPostmen, FIO, 0 as c from Postmen where IDPostmen not in (Select FKPostmen from Subscribers)" +
                    $") as u " +
                    $"where u.c < (Select avg(FKPostmen) from Subscribers) or u.IDPostmen = "+ dataRowView.Row["FKPostmen"], con);
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

        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            if (ValidateSubscriber()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
