using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
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
        List<Company> CompanyList = new List<Company>();
        public string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";
        List<Models> ModelList = new List<Models>();
        HashSet<string> modelDescriptions = new HashSet<string>();
        public DataTable dataTable;
        ObservableCollection<Devices> deviceList;

        public SearchDevice()
        {
            this.InitializeComponent();

            DeviceGrid.IsReadOnly = true;
            DeviceGrid.MaxHeight = Window.Current.Bounds.Height * .8;

            main.Width = Window.Current.Bounds.Width * .98;
            mainStack.Width = Window.Current.Bounds.Width * .98;

            stack0.Width = Window.Current.Bounds.Width * .95 / 2;


            stack1.Width = Window.Current.Bounds.Width * .95 / 2;
            SearchBySN.Width = Window.Current.Bounds.Width * .95 / 2;
            stack3.Width = Window.Current.Bounds.Width * .95 / 4;
            ModelsAvailable.Visibility = Visibility.Collapsed;


            SqlConnection companyConn = null;
            SqlDataReader reader = null;
            HashSet<string> companyDescriptions = new HashSet<string>();


            try
            {
                companyConn = new SqlConnection(connectionString);

                companyConn.Open();

                SqlCommand modelQuery = new SqlCommand("SELECT company FROM device", companyConn);
                reader = modelQuery.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string companyDesc = reader["company"].ToString();

                        // Check if the companyDesc is already in the HashSet
                        if (!companyDescriptions.Contains(companyDesc))
                        {
                            // Add the companyDesc to the HashSet
                            companyDescriptions.Add(companyDesc);

                            // Add the new Models object to the ModelList
                            CompanyList.Add(new Company
                            {
                                companyDesc = companyDesc
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

            stack4.Width = Window.Current.Bounds.Width / 4;
            SearchOption.SelectedIndex = 0;
            SqlConnection modelConn = null;
            reader = null;
            

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
            FetchData(serialNumber.Text);
                
        }

        private void compSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Fetch the suggestions based on user input
                var suggestions = GetSuggestions(sender.Text);

                // Set the suggestions
                sender.ItemsSource = suggestions;
            }
        }

        private List<Company> GetSuggestions(string query)
        {
            // Filter the models based on the query
            return CompanyList.Where(company => company.companyDesc.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        String selected;
        private void compSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Accept the input as the selected value
            selected = args.QueryText;
            populateGrid(selected);
        }
        private void comp_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Get the selected item
            selected = args.SelectedItem.ToString();
            populateGrid(selected);
            

        }

        private void populateGrid(string chosen)
        {
            if (selected == null)
            {
                System.Diagnostics.Debug.WriteLine("No company selected.");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Selected company: " + selected);

            SqlConnection deviceConn = null;
            SqlDataReader reader = null;

            try
            {
                using (deviceConn = new SqlConnection(connectionString))
                {
                    deviceConn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var command = deviceConn.CreateCommand();
                    command.CommandText = "SELECT deviceType, model, specsNsystem, serialNo, invoiceNo, deliveryNo, creditNo FROM device WHERE company = @company";
                    command.Parameters.AddWithValue("@company", selected);

                    DataTable dataTable = new DataTable();
                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }

                    if (dataTable.Rows.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No data found for the selected company.");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("Data retrieved successfully.");

                    // Clear the DataGrid
                    DeviceGrid.ItemsSource = null;

                    // Create a collection of Device objects to use as the ItemsSource
                    deviceList = new ObservableCollection<Devices>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var device = new Devices
                        {
                            deviceType = row["deviceType"].ToString(),
                            model = row["model"].ToString(),
                            specsNsystem = row["specsNsystem"].ToString(),
                            SerialNo = row["serialNo"].ToString(),
                            InvoiceNo = row["invoiceNo"].ToString(),
                            deliveryNo = row["deliveryNo"].ToString(),
                            creditNo = row["creditNo"].ToString()
                        };
                        deviceList.Add(device);
                    }

                    DeviceGrid.ItemsSource = deviceList;
                    System.Diagnostics.Debug.WriteLine("Data bound to DataGrid successfully.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                reader?.Close();
            }
        }

        private void FetchData(string device)
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
                searchQuery.Parameters.AddWithValue("@serialNo", device);

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
            switch (SearchOption.SelectedIndex)
            {
                case 0:
                    serialNumber.Visibility = Visibility.Visible;
                    ModelsAvailable.Visibility = Visibility.Collapsed;
                    comp.Visibility = Visibility.Collapsed;
                    SearchBySN.Visibility = Visibility.Visible;
                    SearchByModel.Visibility = Visibility.Collapsed;
                    search_button.Visibility = Visibility.Visible;
                    break;
                case 1:
                    DeviceGrid.ItemsSource = null;
                    search_button.Visibility = Visibility.Collapsed;
                    ModelsAvailable.Visibility = Visibility.Visible;
                    comp.Visibility = Visibility.Collapsed;
                    serialNumber.Visibility = Visibility.Collapsed;
                    SearchByModel.Visibility = Visibility.Visible;
                    SearchBySN.Visibility = Visibility.Collapsed;
                    stack1.Visibility = Visibility.Collapsed;
                    stack0.Width = Window.Current.Bounds.Width * .95;
                    notes.Visibility = Visibility.Collapsed;
                    break;
                default:
                    DeviceGrid.ItemsSource = null;
                    search_button.Visibility = Visibility.Collapsed;
                    ModelsAvailable.Visibility = Visibility.Collapsed;
                    comp.Visibility = Visibility.Visible;
                    serialNumber.Visibility = Visibility.Collapsed;
                    SearchByModel.Visibility = Visibility.Visible;
                    SearchBySN.Visibility = Visibility.Collapsed;
                    stack1.Visibility = Visibility.Collapsed;
                    stack0.Width = Window.Current.Bounds.Width * .95;
                    notes.Visibility = Visibility.Collapsed;

                    break;
            }

            
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Fetch the suggestions based on user input
                var suggestions = GetModelSuggestions(sender.Text);

                // Set the suggestions
                sender.ItemsSource = suggestions;
            }
        }

        private List<Models> GetModelSuggestions(string query)
        {
            // Filter the models based on the query
            return ModelList.Where(model => model.modelDesc.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Accept the input as the selected value
            string userInput = args.QueryText;
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Get the selected item
            var selected = args.SelectedItem.ToString();

            if (selected == null)
            {
                System.Diagnostics.Debug.WriteLine("No model selected.");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Selected model: " + selected);

            SqlConnection deviceConn = null;
            SqlDataReader reader = null;

            try
            {
                using (deviceConn = new SqlConnection(connectionString))
                {
                    deviceConn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var command = deviceConn.CreateCommand();
                    command.CommandText = "SELECT deviceType, model, specsNsystem, serialNo, invoiceNo, deliveryNo, creditNo FROM device WHERE model = @model";
                    command.Parameters.AddWithValue("@model", selected);

                    DataTable dataTable = new DataTable();
                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }

                    if (dataTable.Rows.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No data found for the selected model.");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("Data retrieved successfully.");

                    // Clear the DataGrid
                    DeviceGrid.ItemsSource = null;

                    // Create a collection of Device objects to use as the ItemsSource
                    deviceList = new ObservableCollection<Devices>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var device = new Devices
                        {
                            deviceType = row["deviceType"].ToString(),
                            model = row["model"].ToString(),
                            specsNsystem = row["specsNsystem"].ToString(),
                            SerialNo = row["serialNo"].ToString(),
                            InvoiceNo = row["invoiceNo"].ToString(),
                            deliveryNo = row["deliveryNo"].ToString(),
                            creditNo = row["creditNo"].ToString()

                        };
                        deviceList.Add(device);
                    }

                    DeviceGrid.ItemsSource = deviceList;
                    System.Diagnostics.Debug.WriteLine("Data bound to DataGrid successfully.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                reader?.Close();
            }
        }

        private void DeviceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = DeviceGrid.SelectedIndex;
            if (selectedIndex >= 0)
            {
                selectedDevice = deviceList[selectedIndex].GetSerial();
                System.Diagnostics.Debug.WriteLine(selectedDevice);
                FetchData(selectedDevice);
            }
            
        }
        string selectedDevice;
    }

    public class Devices
    {
        public string deviceType { get; set; }
        public string model { get; set; }
        public string specsNsystem { get; set; }
        public string SerialNo { get; set; }
        public string InvoiceNo { get; set; }
        public string deliveryNo { get; set; }
        public string creditNo { get; set; }
        public string GetSerial()
        {
            return SerialNo;
        }
        public string GetInvoice()
        {
            return InvoiceNo;
        }
    }


}
