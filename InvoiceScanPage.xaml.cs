using System;
//using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Drawing;
using Windows.Media.Ocr;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InvoiceScanPage : Page
    {
        List<Company> CompanyList = new List<Company>();
        public static string invoicePath = "";
        private DeviceWatcher scannerWatcher;
        public static string invoiceNumber = "";
        private string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";


        public InvoiceScanPage()
        {
            this.InitializeComponent();

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

        }

        private void InitDeviceWatcher()
        {
            scannerWatcher = DeviceInformation.CreateWatcher(DeviceClass.ImageScanner);

            scannerWatcher.Added += OnScannerAdded;
            scannerWatcher.Removed += OnScannerRemoved;
            scannerWatcher.EnumerationCompleted += OnScannerEnumerationComplete;

            scannerWatcher.Start();
            NotificationTextBlock.Text = "Starting scanner watcher...";
            System.Diagnostics.Debug.WriteLine("Starting scanner watcher...");
        }

        private async void OnScannerAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NotificationTextBlock.Text = $"Scanner {deviceInfo.Name} has been added";
                System.Diagnostics.Debug.WriteLine($"Scanner added: {deviceInfo.Name} ({deviceInfo.Id})");
            });
        }

        private void OnScannerRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            System.Diagnostics.Debug.WriteLine($"Scanner removed: {deviceInfoUpdate.Id}");
        }

        private void OnScannerEnumerationComplete(DeviceWatcher sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Scanner enumeration complete");
        }

        private ImageScanner myScanner;
        private SoftwareBitmap scannedBitmap;


        private async void GetScanButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationTextBlock.Text = "Finding scanners...";
            System.Diagnostics.Debug.WriteLine("Finding scanners...");
            var scanners = await DeviceInformation.FindAllAsync(DeviceClass.ImageScanner);
            NotificationTextBlock.Text = $"Using: {scanners[0].Name}";

            if (scanners.Count == 0)
            {
                NotificationTextBlock.Text = "No scanners found.";
                System.Diagnostics.Debug.WriteLine("No scanners found.");
                return;
            }

            var deviceId = scanners[0].Id;

            // Get the scanner object
            try
            {
                myScanner = await ImageScanner.FromIdAsync(deviceId);
                if (myScanner.IsScanSourceSupported(ImageScannerScanSource.Default))
                {
                    // Perform the scan to an in-memory stream
                    var stream = new InMemoryRandomAccessStream();

                    await myScanner.ScanPreviewToStreamAsync(ImageScannerScanSource.Default, stream);
                    stream.Seek(0);

                    // Decode the image from the stream
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    // Convert the image to a compatible format (BGRA8, premultiplied alpha)
                    scannedBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);


                    // Display the image in the UI
                    var bitmapSource = new SoftwareBitmapSource();
                    await bitmapSource.SetBitmapAsync(scannedBitmap);
                    ScannedImage.Source = bitmapSource;
                    
                    var engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US"));
                    
                   
                    var ocrResult = await engine.RecognizeAsync(softwareBitmap);
                    string data = ocrResult.Text;
                    System.Diagnostics.Debug.WriteLine(data);

                    // Define the pattern to match "ID" followed by any number of digits
                    string pattern = @"Invoice\s*No.[\r\n]+\s*([A-Za-z0-9]+)";
                    string pattern2 = @"Invoice\s*No:\s*(?<idNumber>\d+)";
                    // Use Regex to find matches
                    Match match = Regex.Match(data, pattern, RegexOptions.Multiline);
                    Match match2 = Regex.Match(data, pattern2);

                    if (match.Success)
                    {
                        // Extract the number after "ID"
                        string idNumber = match.Groups[1].Value;
                        System.Diagnostics.Debug.WriteLine("Invoice: " + idNumber);
                    } else if (match2.Success)
                    {
                        string idNumber = match.Groups[1].Value;
                        System.Diagnostics.Debug.WriteLine("Invoice: " + idNumber);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("not found.");
                    }

                    // Enable save and cancel button
                    cancelScanButton.IsEnabled = true;
                    Next.IsEnabled = true;
                }
                else
                {
                    throw new Exception("Default scan source is not supported by this scanner.");
                }
            }
            catch (Exception ex)
            {
                // Log or display the exception message
                System.Diagnostics.Debug.WriteLine($"Scan failed: {ex.Message}");
            }
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

        public string userInput;
        private void compSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Accept the input as the selected value
            userInput = args.QueryText;
        }


        private void comp_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Get the selected item
            userInput = args.SelectedItem.ToString();


        }

        private void cancelScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the displayed image and disable save and cancel buttons
            ScannedImage.Source = null;
            cancelScanButton.IsEnabled = false;
            Next.IsEnabled = false;
        }


        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (invNo.Text == "" || invDate.Date == null || comp.Text == "")
            {
                error("Please fill in all fields.");
            }
            else
            {
                string name = invNo.Text + ".jpg";
                if ((bool)invSameDeli_Check.IsChecked)
                {
                    if (scannedBitmap != null) {
                        try
                        {
                            string folderName = "Invoice Notes";
                            StorageFolder documentsFolder = KnownFolders.DocumentsLibrary;
                            StorageFolder folder = await documentsFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                            // Perform the full scan to the specified folder
                            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                            invoicePath = file.Path;
                            System.Diagnostics.Debug.WriteLine(invoicePath);

                            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                                encoder.SetSoftwareBitmap(scannedBitmap);
                                await encoder.FlushAsync();
                            }
                            saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, (bool)invSameDeli_Check.IsChecked);
                            DeliveryScanPage.deliveryNumber = invoiceNumber;
                            System.Diagnostics.Debug.WriteLine("invoice: " + invoiceNumber);
                            System.Diagnostics.Debug.WriteLine("delivery: " + DeliveryScanPage.deliveryNumber);
                            clearData();
                            Frame.Navigate(typeof(CreditScanPage));
                        }
                        catch (Exception ex)
                        {
                            // Log or display the exception message
                            error($"Save failed: {ex.Message}");
                        }
                    } else
                    {
                        saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, (bool)invSameDeli_Check.IsChecked);
                        DeliveryScanPage.deliveryNumber = invoiceNumber;
                        System.Diagnostics.Debug.WriteLine("invoice: " + invoiceNumber);
                        System.Diagnostics.Debug.WriteLine("delivery: " + DeliveryScanPage.deliveryNumber);
                        clearData();
                        Frame.Navigate(typeof(CreditScanPage));

                    }



                } else
                {
                    //different invoice and delivery notes
                    if (scannedBitmap != null)
                    {
                        try
                        {
                            string folderName = "Invoice Notes";
                            StorageFolder documentsFolder = KnownFolders.DocumentsLibrary;
                            StorageFolder folder = await documentsFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                            // Perform the full scan to the specified folder
                            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                            invoicePath = file.Path;
                            System.Diagnostics.Debug.WriteLine("Image path: " + invoicePath);

                            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                                encoder.SetSoftwareBitmap(scannedBitmap);
                                await encoder.FlushAsync();
                            }
                            saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, (bool)invSameDeli_Check.IsChecked);
                            clearData();
                            Frame.Navigate(typeof(DeliveryScanPage));

                        }
                        catch (Exception ex)
                        {
                            // Log or display the exception message
                            error($"Save failed: {ex.Message}");
                        }
                    } else
                    {
                        saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, (bool)invSameDeli_Check.IsChecked);
                        clearData();
                        Frame.Navigate(typeof(DeliveryScanPage));
                    }
                }                
            }
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

        private void invSameDeli_Check_Checked(object sender, RoutedEventArgs e)
        {
            DeliveryScanPage.deliveryNumber = invoiceNumber;
            sameDeli = true;
        }

        private void invSameDeli_Check_Unchecked(object sender, RoutedEventArgs e)
        {
            DeliveryScanPage.deliveryNumber = "";
            sameDeli = false;
        }

        public static DateTimeOffset invoiceDate;
        public static string company = "";
        public static bool sameDeli;

        private void clearData()
        {
            invNo.Text = "";
            comp.Text = "";
        }

        public static void saveDetails(string invoiceNo, DateTimeOffset invDate, string comp, bool same)
        {
            invoiceNumber = invoiceNo;
            invoiceDate = invDate;
            company = comp;
            sameDeli = same;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            mainStack.Width = Window.Current.Bounds.Width;
            stack0.Width = Window.Current.Bounds.Width;
            stack1.Width = Window.Current.Bounds.Width / 2;
            stack2.Width = Window.Current.Bounds.Width / 2;
            invDate.MaxDate = DateTime.Now;
            if (sameDeli)
            {
                invSameDeli_Check.IsChecked = true;
            }

            if (invoicePath != "")
            {
                invNo.Text = invoiceNumber;
                comp.Text = company;
                invDate.Date = invoiceDate;
                StorageFile imageFile = await StorageFile.GetFileFromPathAsync(invoicePath);
                using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    // Create a BitmapImage
                    BitmapImage bitmapImage = new BitmapImage();

                    // Set the source of the BitmapImage to the stream
                    await bitmapImage.SetSourceAsync(fileStream);

                    // Set the image control's source to the BitmapImage
                    ScannedImage.Source = bitmapImage;
                    Next.IsEnabled = true;
                }

            }
            InitDeviceWatcher();
        }
    }

}


