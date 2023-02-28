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
    }
}
