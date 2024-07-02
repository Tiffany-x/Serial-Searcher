using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

                    

            if (SerialNumber != null)
            {
                serialNo.Text = SerialNumber;
                modelName.Text = model;
                specs.Text = specifications;
                systInstall.Text = system;
            }


            
                label.Text = reps+ " out of " +CreditScanPage.devices;
        }

        Dictionary<string, string>[] devices = new Dictionary<string, string>[CreditScanPage.devices];

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (reps == CreditScanPage.devices)
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


                try
                {
                    saveAllConn = new SqlConnection(connectionString);

                    saveAllConn.Open();

                    SqlCommand invoiceQuery = new SqlCommand(
        "INSERT INTO invoice(invoiceNo, invoiceDate, company, LPONumber, createdAt, docImage) " +
        "VALUES(@invNo, @invDate, @company, @LPONum, @createdAt, @invPath)", saveAllConn);

                    invoiceQuery.CommandType = System.Data.CommandType.Text;

                    invoiceQuery.Parameters.AddWithValue("@invNo", InvoiceScanPage.invoiceNumber);

                    invoiceQuery.Parameters.AddWithValue("@invDate", InvoiceScanPage.invoiceDate);

                    invoiceQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                    invoiceQuery.Parameters.AddWithValue("@LPONum", InvoiceScanPage.LPONumber);

                    invoiceQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    invoiceQuery.Parameters.AddWithValue("@invPath", InvoiceScanPage.invoicePath);

                    SqlCommand deliveryQuery = new SqlCommand(
        "INSERT INTO delivery(deliveryNo, invoiceDate, company, LPONumber, createdAt, docImage) " +
        "VALUES(@deliNo, @invDate, @company, @LPONum, @createdAt, @deliPath)", saveAllConn);

                    deliveryQuery.CommandType = System.Data.CommandType.Text;

                    deliveryQuery.Parameters.AddWithValue("@deliNo", DeliveryScanPage.deliveryNumber);

                    deliveryQuery.Parameters.AddWithValue("@invDate", DeliveryScanPage.deliveryDate);

                    deliveryQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                    deliveryQuery.Parameters.AddWithValue("@LPONum", InvoiceScanPage.LPONumber);

                    deliveryQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    deliveryQuery.Parameters.AddWithValue("@deliPath", DeliveryScanPage.deliPath);


                    SqlCommand creditQuery = new SqlCommand(
        "INSERT INTO credit(creditNo, invoiceDate, company, createdAt, docImage) " +
        "VALUES(@credNo, @invDate, @company, @createdAt, @credPath)", saveAllConn);

                    creditQuery.CommandType = System.Data.CommandType.Text;

                    creditQuery.Parameters.AddWithValue("@credNo", CreditScanPage.creditNumber);

                    creditQuery.Parameters.AddWithValue("@invDate", InvoiceScanPage.invoiceDate);

                    creditQuery.Parameters.AddWithValue("@company", InvoiceScanPage.company);

                    creditQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    creditQuery.Parameters.AddWithValue("@credPath", DeliveryScanPage.deliPath);

                    SqlCommand deviceQuery;

                    

                    for (int i = 0; i < devices.Length; i++)
                    {
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
                                invoiceQuery.ExecuteNonQuery();
                                deliveryQuery.ExecuteNonQuery();
                                creditQuery.ExecuteNonQuery();
                                deviceQuery.ExecuteNonQuery();
                                ShowToastNotification("Success", "Device has been recorded.");
                                clearAll();
                                Save.Content = "Save All";
                            }
                            catch (Exception ex)
                            {
                                // Display specific error message
                                error("Error: " + ex.Message);
                                System.Diagnostics.Debug.WriteLine(ex.Message);
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
                }
                clearAll();
                reps++;
                label.Text = reps + " out of " + CreditScanPage.devices;
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

        private void DeviceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceCombo.SelectedIndex >= 0)
            {
                ComboBoxItem ComboItem = (ComboBoxItem)DeviceCombo.SelectedItem;
                deviceType = ComboItem.Name;
                System.Diagnostics.Debug.WriteLine("name: " + deviceType);
                if (deviceType == "Desktop" || deviceType == "Laptop")
                {
                    systemInstall.Visibility = Visibility.Visible;
                    systInstall.Visibility = Visibility.Visible;
                }
                else if (deviceType != "Desktop" || deviceType != "Laptop")
                {
                    systemInstall.Visibility = Visibility.Collapsed;
                    systInstall.Visibility = Visibility.Collapsed;
                }
            }
            
        }

        private void clearAll()
        {
            DeviceCombo.SelectedIndex = -1;
            serialNo.Text = "";
            modelName.Text = "";
            specs.Text = "";
            systInstall.Text = "";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreditScanPage));
        }
    }

}
