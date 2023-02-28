using DBPost.Views;
using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBPost.AddEditWindow
{
    /// <summary>
    /// Логика взаимодействия для periodicals.xaml
    /// </summary>
    public partial class periodicals : UserControl
    {
        private PeriodicalsView PeriodicalsView { get; set; }
        bool isAdding = true;
        string? idPeriodical = string.Empty;
        public periodicals(PeriodicalsView periodicalsView)
        {
            InitializeComponent();
            PeriodicalsView = periodicalsView;
        }

        public periodicals(PeriodicalsView periodicalsView, DataRowView dataRowView)
        {
            InitializeComponent();
            isAdding = false;
            PeriodicalsView = periodicalsView;
            idPeriodical = dataRowView.Row["IDPeriodical"].ToString();
            this.Title.Text = dataRowView.Row["Title"].ToString();
            this.PriceMonth.Text = dataRowView.Row["PriceMonth"].ToString();
            this.PriceThreeMonths.Text = dataRowView.Row["PriceThreeMonths"].ToString();
            this.PriceSixMonths.Text = dataRowView.Row["PriceSixMonths"].ToString();
            this.PriceTwelveMonths.Text = dataRowView.Row["PriceTwelveMonths"].ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            (FindResource("CloseMenu") as Storyboard)!.Completed += (s, a) => {
                (this.Parent as Grid)!.Children.Remove(this);
                PeriodicalsView.EnableWindow();
                if ((sender as Button) == AddButton)
                {
                    string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand cmd;
                        if (isAdding)
                        {
                            cmd = new SqlCommand($"INSERT INTO Periodicals (Title, PriceMonth, PriceThreeMonths, PriceSixMonths, PriceTwelveMonths) Values ('{Title.Text}',{PriceMonth.Text},{PriceThreeMonths.Text}, {PriceThreeMonths.Text}, {PriceTwelveMonths.Text})", con);
                        }
                        else
                        {
                            cmd = new SqlCommand($"Update Periodicals Set Title='{Title.Text}', PriceMonth={PriceMonth.Text}, PriceThreeMonths={PriceThreeMonths.Text}, PriceSixMonths={PriceSixMonths.Text}, PriceTwelveMonths={PriceTwelveMonths.Text} where IDPeriodical={idPeriodical}", con);
                        }
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    PeriodicalsView.FillDataGrid();
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
