using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class DeviceDetails : Page
    {
        public static string deviceType;
        List<Models> ModelList = new List<Models>();
        List<string> DeviceList;

        HashSet<string> modelDescriptions = new HashSet<string>();
        private int reps = 1;
        Dictionary<string,
               string> deviceMap = new Dictionary<string,
                                            string>();
        private string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";
        private SqlConnection saveAllConn = null;

        public DeviceDetails()
        {
            this.InitializeComponent();

            invNo.Text = InvoiceScanPage.invoiceNumber;
            deliNo.Text = DeliveryScanPage.deliveryNumber;
            credNo.Text = CreditScanPage.creditNumber;

            if (reps == CreditScanPage.devices)
            {
                Save.Content = "Save All";
            }

            SqlConnection modelConn = null;
            SqlDataReader reader = null;

            DeviceList = new List<string> { "Laptop", "Monitor", "CPU", "Printer", "Mouse", "Toner" , "Battery (Laptop)", "Keyboard (Laptop)" , "UPS",
            "Keyboard (Desktop)", "Switch", "Camera", "Telephone", "HDD", "SSD", "Hard Disk"};


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



            if (SerialNumber != null)
            {
                serialNo.Text = SerialNumber;
                modelName.Text = model;
                specs.Text = specifications;
                systInstall.Text = system;
            }


            
                label.Text = reps+ " out of " +CreditScanPage.devices;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Fetch the suggestions based on user input
                var suggestions = GetSuggestions(sender.Text);

                // Set the suggestions
                sender.ItemsSource = suggestions;
            }
        }

        private List<Models> GetSuggestions(string query)
        {
            // Filter the models based on the query
            return ModelList.Where(model => model.modelDesc.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<string> GetDeviceSuggestions(string query)
        {
            // Filter the models based on the query
            return DeviceList.Where(i => i.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public string userInput;
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Accept the input as the selected value
            userInput = args.QueryText;
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Get the selected item
            userInput = args.SelectedItem.ToString();

            
        }


        Dictionary<string, string>[] devices = new Dictionary<string, string>[CreditScanPage.devices];

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (reps == CreditScanPage.devices)
            {
                Save.Content = "Save All";
                saveDetails(serialNo.Text, userInput, specs.Text, systemInstall.Text);

                devices[reps - 1] = new Dictionary<string, string>
                    {
                        {"devType", deviceType },
                        {"serialNo", serialNo.Text},
                        {"model", userInput},
                        {"specifications", specs.Text},
                        {"systemInstall", systInstall.Text},
                        {"invoiceNo", InvoiceScanPage.invoiceNumber},
                        {"deliveryNo", DeliveryScanPage.deliveryNumber},
                        {"creditNo", CreditScanPage.creditNumber}
                    };


                try
                {
                    saveAllConn = new SqlConnection(connectionString);

                    saveAllConn.Open();

                    SqlCommand invoiceQuery = new SqlCommand(
        "INSERT INTO invoice(invoiceNo, invoiceDate, company, createdAt, docImage) " +
        "VALUES(@invNo, @invDate, @company, @createdAt, @invPath)", saveAllConn);

                    invoiceQuery.CommandType = System.Data.CommandType.Text;

                    invoiceQuery.Parameters.AddWithValue("@invNo", InvoiceScanPage.invoiceNumber);

                    invoiceQuery.Parameters.AddWithValue("@invDate", InvoiceScanPage.invoiceDate);

                    invoiceQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);


                    invoiceQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    invoiceQuery.Parameters.AddWithValue("@invPath", InvoiceScanPage.invoicePath);

                    SqlCommand deliveryQuery;

                    if (InvoiceScanPage.sameDeli)
                    {
                        deliveryQuery = new SqlCommand(
        "INSERT INTO delivery(deliveryNo, invoiceDate, company, createdAt, docImage) " +
        "VALUES(@deliNo, @invDate, @company, @createdAt, @deliPath)", saveAllConn);

                        deliveryQuery.CommandType = System.Data.CommandType.Text;

                        deliveryQuery.Parameters.AddWithValue("@deliNo", DeliveryScanPage.deliveryNumber);

                        deliveryQuery.Parameters.AddWithValue("@invDate", InvoiceScanPage.invoiceDate);

                        deliveryQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                        deliveryQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                        deliveryQuery.Parameters.AddWithValue("@deliPath", InvoiceScanPage.invoicePath);

                    }
                    else
                    {
                        deliveryQuery = new SqlCommand(
        "INSERT INTO delivery(deliveryNo, invoiceDate, company, createdAt, docImage) " +
        "VALUES(@deliNo, @invDate, @company, @createdAt, @deliPath)", saveAllConn);

                        deliveryQuery.CommandType = System.Data.CommandType.Text;

                        deliveryQuery.Parameters.AddWithValue("@deliNo", DeliveryScanPage.deliveryNumber);

                        deliveryQuery.Parameters.AddWithValue("@invDate", DeliveryScanPage.deliveryDate);

                        deliveryQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                        deliveryQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                        deliveryQuery.Parameters.AddWithValue("@deliPath", DeliveryScanPage.deliPath);

                    }


                    SqlCommand creditQuery = new SqlCommand(
        "INSERT INTO credit(creditNo, invoiceDate, company, createdAt, docImage) " +
        "VALUES(@credNo, @invDate, @company, @createdAt, @credPath)", saveAllConn);

                    creditQuery.CommandType = System.Data.CommandType.Text;

                    creditQuery.Parameters.AddWithValue("@credNo", CreditScanPage.creditNumber);

                    creditQuery.Parameters.AddWithValue("@invDate", InvoiceScanPage.invoiceDate);

                    creditQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                    creditQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    creditQuery.Parameters.AddWithValue("@credPath", CreditScanPage.creditPath);

                    invoiceQuery.ExecuteNonQuery();
                    deliveryQuery.ExecuteNonQuery();
                    creditQuery.ExecuteNonQuery();

                    SqlCommand deviceQuery;

                    for (int i = 0; i < devices.Length; i++)
                    {
                        if (reps == CreditScanPage.devices)
                        {
                            Save.Content = "Save All";
                        }
                        // Create a new SqlCommand for each device to avoid parameter conflicts
                        using (deviceQuery = new SqlCommand(
                            "INSERT INTO device(deviceType, model, serialNo, specsNsystem, invoiceNo, deliveryNo, creditNo, createdAt) " +
        "VALUES(@type, @model, @serialNo, @specsNsystem, @invNo, @deliNo, @credNo, @createdAt)", saveAllConn))
                        {
                            deviceQuery.CommandType = System.Data.CommandType.Text;
                            // Assign parameters based on the current device details
                            var currentDevice = devices[i];

                            

                            deviceQuery.Parameters.AddWithValue("@type", currentDevice["devType"]);
                            deviceQuery.Parameters.AddWithValue("@model", currentDevice["model"]);
                            deviceQuery.Parameters.AddWithValue("@serialNo", currentDevice["serialNo"]);

                            if (deviceType == "Desktop" || deviceType == "Laptop")
                            {
                                deviceQuery.Parameters.AddWithValue("@specsNsystem", currentDevice["specifications"] + " running " + currentDevice["systemInstall"]);
                            }
                            else
                            {
                                deviceQuery.Parameters.AddWithValue("@specsNsystem", currentDevice["specifications"]);
                            }

                            deviceQuery.Parameters.AddWithValue("@invNo", currentDevice["invoiceNo"]);
                            deviceQuery.Parameters.AddWithValue("@deliNo", currentDevice["deliveryNo"]);
                            deviceQuery.Parameters.AddWithValue("@credNo", currentDevice["creditNo"]);
                            deviceQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                            // Execute the query
                            try
                            {
                                deviceQuery.ExecuteNonQuery();
                                ShowToastNotification("Success", "Device has been recorded.");
                                clearAll();
                                InvoiceScanPage.invoicePath = "";
                                InvoiceScanPage.invoiceNumber = "";
                                InvoiceScanPage.invoiceDate = DateTimeOffset.Now;
                                DeliveryScanPage.deliPath = "";
                                DeliveryScanPage.deliveryNumber = "";
                                DeliveryScanPage.deliveryDate = DateTimeOffset.Now;
                                CreditScanPage.creditNumber = "";
                                CreditScanPage.creditPath = "";
                            }
                            catch (Exception ex)
                            {
                                // Display specific error message
                                //error("Error: " + ex.Message);
                                System.Diagnostics.Debug.WriteLine("Not recorded: " +ex.Message);
                            }
                        }
                    }

                    


                }
                finally
                {

                    // close connection
                    if (saveAllConn != null)
                    {
                        saveAllConn.Close();
                    }
                }
                Frame.Navigate(typeof(InvoiceScanPage));
                return;
            } else if (reps < CreditScanPage.devices)
            {
                if (deviceType == "" || serialNo.Text == "" || modelName.Text == "")
                {
                    error("PLease fill in all the values.");
                }
                else
                {
                    saveDetails(serialNo.Text, modelName.Text, specs.Text, systemInstall.Text);
                    devices[reps - 1] = new Dictionary<string, string>
                    {
                        {"devType", deviceType },
                        {"serialNo", serialNo.Text},
                        {"model", modelName.Text},
                        {"specifications", specs.Text},
                        {"systemInstall", systInstall.Text},
                        {"invoiceNo", InvoiceScanPage.invoiceNumber},
                        {"deliveryNo", DeliveryScanPage.deliveryNumber},
                        {"creditNo", CreditScanPage.creditNumber}
                    };

                    System.Diagnostics.Debug.WriteLine(deviceType);
                    clearAll();
                    reps++;
                    label.Text = reps + " out of " + CreditScanPage.devices;
                }
                
            }

        }

        private void ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);
        }

        public static string SerialNumber;
        public static string model;
        public static string specifications;
        public static string system;

        public static void saveDetails(string serialNoText, string modelName, string specs,
            string systemInstall)
        {
            SerialNumber = serialNoText;
            model = modelName;
            specifications = specs;
            system = systemInstall;
        }


        private async void error(string details)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Error",
                Content = new TextBlock { Text = details },
                Height = 500,
                CloseButtonText = "Okay"
            };

            await dialog.ShowAsync();
        }

        
        private void clearAll()
        {
            Devices.Text = "";
            serialNo.Text = "";
            modelName.Text = "";
            specs.Text = "";
            systInstall.Text = "";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreditScanPage));
        }

        private void Devices_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            chosenDevice = args.SelectedItem.ToString();
            if (chosenDevice == "Desktop" || deviceType == "Laptop")
            {
                systemInstall.Visibility = Visibility.Visible;
                systInstall.Visibility = Visibility.Visible;
                Specifications.Visibility = Visibility.Visible;
                specs.Visibility = Visibility.Visible;

            }
            else if (chosenDevice != "Desktop" || deviceType != "Laptop")
            {
                systemInstall.Visibility = Visibility.Collapsed;
                systInstall.Visibility = Visibility.Collapsed;
                Specifications.Visibility = Visibility.Collapsed;
                specs.Visibility = Visibility.Collapsed;
            }
        }
        public string chosenDevice;
        private void DeviceSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            chosenDevice = args.QueryText;
            if (chosenDevice == "Desktop" || deviceType == "Laptop")
            {
                systemInstall.Visibility = Visibility.Visible;
                systInstall.Visibility = Visibility.Visible;
                Specifications.Visibility = Visibility.Visible;
                specs.Visibility = Visibility.Visible;

            }
            else if (chosenDevice != "Desktop" || deviceType != "Laptop")
            {
                systemInstall.Visibility = Visibility.Collapsed;
                systInstall.Visibility = Visibility.Collapsed;
                Specifications.Visibility = Visibility.Collapsed;
                specs.Visibility = Visibility.Collapsed;
            }
        }

        private void DeviceSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Fetch the suggestions based on user input
                var suggestions = GetDeviceSuggestions(sender.Text);

                // Set the suggestions
                sender.ItemsSource = suggestions;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (reps == CreditScanPage.devices)
            {
                Save.Content = "Save All";
            }
        }
    }

}
