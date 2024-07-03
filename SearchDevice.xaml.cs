using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        List<Models> ModelList = new List<Models>();
        List<Devices> DeviceList = new List<Devices>();
        HashSet<string> modelDescriptions = new HashSet<string>();

        public SearchDevice()
        {
            this.InitializeComponent();
       
            search.Width = Window.Current.Bounds.Width / 2;
            notes.Width = Window.Current.Bounds.Width / 2;
            stack0.Width = Window.Current.Bounds.Width / 2;
            stack1.Width = Window.Current.Bounds.Width / 2;
            SearchByModel.Width = Window.Current.Bounds.Width / 2;
            SearchBySN.Width = Window.Current.Bounds.Width / 2;
            stack3.Width = Window.Current.Bounds.Width / 4;
            ModelsAvailable.Visibility = Visibility.Collapsed;
            mainStack.Height = Window.Current.Bounds.Height * 0.8;

            stack4.Width = Window.Current.Bounds.Width / 4;
            SearchOption.SelectedIndex = 0;
            SqlConnection modelConn = null;
            SqlDataReader reader = null;


            try
            {
                modelConn = new SqlConnection(connectionString);

                modelConn.Open();

                SqlCommand modelQuery = new SqlCommand("SELECT model FROM device", modelConn);
                reader = modelQuery.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string modelDesc = reader["model"].ToString();

                        // Check if the modelDesc is already in the HashSet
                        if (!modelDescriptions.Contains(modelDesc))
                        {
                            // Add the modelDesc to the HashSet
                            modelDescriptions.Add(modelDesc);

                            // Add the new Models object to the ModelList
                            ModelList.Add(new Models
                            {
                                modelDesc = modelDesc
                            });
                        }
                    }
                    
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

        }


        private string invoicePath;
        private string deliveryPath;
        private string creditPath;

        private void search_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SearchOption.SelectedIndex == 0)
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
                    }
                    else
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

                if (invoiceNo.Text == deliveryNo.Text)
                {
                    deliveryPath = invoicePath;
                }
                else
                {
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
                }
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
            } else
            {
                string selected = ((Models)ModelsAvailable.SelectedItem).ToString();
                System.Diagnostics.Debug.WriteLine(selected);
                SqlConnection deviceConn;
                SqlDataReader reader = null;

                try
                {
                    deviceConn = new SqlConnection(connectionString);

                    deviceConn.Open();

                    SqlCommand modelQuery = new SqlCommand("SELECT serialNo, invoiceNo FROM device WHERE model = @model", deviceConn);

                    modelQuery.Parameters.AddWithValue("@model", selected);
                    reader = modelQuery.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            System.Diagnostics.Debug.WriteLine("a device");
                            string SN = reader["serialNo"].ToString();
                            string IN = reader["invoiceNo"].ToString();



                            // Add the new Models object to the ModelList
                            DeviceList.Add(new Devices
                            {
                                serialNumber = SN,
                                    invoiceDate = IN
                            });
                            }

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
            System.Diagnostics.Debug.WriteLine(invoicePath);

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

        private void SearchOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchOption.SelectedIndex == 0)
            {
                serialNumber.Visibility = Visibility.Visible;
                ModelsAvailable.Visibility = Visibility.Collapsed;
                SearchBySN.Visibility = Visibility.Visible;
                SearchByModel.Visibility = Visibility.Collapsed;
            } else
            {
                ModelsAvailable.Visibility = Visibility.Visible;
                serialNumber.Visibility = Visibility.Collapsed;
                SearchByModel.Visibility = Visibility.Visible;
                SearchBySN.Visibility = Visibility.Collapsed;
            }
        }

    }

   
}
