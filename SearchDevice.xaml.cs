using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchDevice : Page
    {
        public string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";

        public SearchDevice()
        {
            this.InitializeComponent();
            mainStack.Width = Window.Current.Bounds.Width * .9;
            stack0.Width = Window.Current.Bounds.Width * 0.9;
            stack1.Width = Window.Current.Bounds.Width / 2;
            stack2.Width = Window.Current.Bounds.Width / 2;
            stack3.Width = Window.Current.Bounds.Width / 4;

            stack4.Width = Window.Current.Bounds.Width / 4;

        }
        private string invoicePath;
        private string deliveryPath;
        private string creditPath;

        private void search_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SqlConnection searchConn = null;
            SqlDataReader reader = null;
            SqlDataReader invoiceReader;
            SqlDataReader deliveryReader;
            SqlDataReader creditReader;


            try
            {
                searchConn = new SqlConnection(connectionString);

                searchConn.Open();

                SqlCommand searchQuery = new SqlCommand(
    "select deviceType, model, specsNsystem, invoiceNo, deliveryNo, creditNo from device where serialNo = @serialNo", searchConn);
                searchQuery.Parameters.AddWithValue("@serialNo", serialNumber.Text);
                
                reader = searchQuery.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        model.Text = reader["model"].ToString();
                        specsNsystem.Text = reader["specsNsystem"].ToString();
                        invoiceNo.Text = reader["invoiceNo"].ToString();
                        deliveryNo.Text = reader["deliveryNo"].ToString();
                        creditNo.Text = reader["creditNo"].ToString();
                    }
                } else
                {
                    noDevice();
                }                
            }
            finally
            {
                // close reader
                if (reader != null)
                {
                    reader.Close();
                }

                // close connection

            }

            SqlCommand invoiceQuery = new SqlCommand(
    "SELECT docImage FROM invoice where invoiceNo = @invNo", searchConn);
            invoiceQuery.Parameters.AddWithValue("@invNo", invoiceNo.Text);

            invoiceReader = invoiceQuery.ExecuteReader();
            if (invoiceReader.HasRows)
            {
                while (invoiceReader.Read())
                {
                    invoicePath = invoiceReader["docImage"].ToString();
                }
            }
            invoiceReader.Close();

            SqlCommand deliveryQuery = new SqlCommand(
"SELECT docImage FROM delivery where deliveryNo = @deliNo", searchConn);
            deliveryQuery.Parameters.AddWithValue("@deliNo", deliveryNo.Text);
            deliveryReader = deliveryQuery.ExecuteReader();
            if (deliveryReader.HasRows)
            {
                while (deliveryReader.Read())
                {
                    deliveryPath = deliveryReader["docImage"].ToString();
                }
            }
            deliveryReader.Close();

            SqlCommand creditQuery = new SqlCommand(
"SELECT docImage FROM credit where creditNo = @credNo", searchConn);
            creditQuery.Parameters.AddWithValue("@credNo", creditNo.Text);
            creditReader = creditQuery.ExecuteReader();
            if (creditReader.HasRows)
            {
                while (creditReader.Read())
                {
                    creditPath = creditReader["docImage"].ToString();
                }
            }
            creditReader.Close();
            if (searchConn != null)
            {
                searchConn.Close();
            }
        }
        private async void noDevice()
        {
            ContentDialog error = new ContentDialog()
            {
                Title = "No Device",
                Content = new TextBlock { Text = "No device with the serial number " + serialNumber.Text + " exists." },
                CloseButtonText = "Okay"
            };

            await error.ShowAsync();
        }

        private async void invoice_button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            var storageFile = await StorageFile.GetFileFromPathAsync(invoicePath);
            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                await image.SetSourceAsync(stream);
            }
            noteDisplay.Source = image;
        }



        private async void delivery_button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            var storageFile = await StorageFile.GetFileFromPathAsync(deliveryPath);
            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                await image.SetSourceAsync(stream);
            }
            noteDisplay.Source = image;
        }
        private async void credit_button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            var storageFile = await StorageFile.GetFileFromPathAsync(creditPath);
            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                await image.SetSourceAsync(stream);
            }
            noteDisplay.Source = image;
        }
    }
}
