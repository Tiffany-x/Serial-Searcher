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




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InvoiceScanPage : Page
    {
        public static string invoicePath;
        private DeviceWatcher scannerWatcher;
        public static string invoiceNumber = "";


        public InvoiceScanPage()
        {
            this.InitializeComponent();
            mainStack.Width = Window.Current.Bounds.Width * 0.9;
            stack0.Width = Window.Current.Bounds.Width * 0.9;
            stack1.Width = Window.Current.Bounds.Width * 0.9 / 2;
            stack2.Width = Window.Current.Bounds.Width * 0.9 / 2;



            if (company != null)
            {
                invNo.Text = invoiceNumber;
                comp.Text = company;
                LPONo.Text = LPONumber;
                invDate.Date = invoiceDate;
            }
            InitDeviceWatcher();
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

                    // Enable save and cancel buttons
                    Next.IsEnabled = false;
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



        private void cancelScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the displayed image and disable save and cancel buttons
            ScannedImage.Source = null;
            cancelScanButton.IsEnabled = false;
            Next.IsEnabled = false;
        }

        public TextBox getTextBox()
        {
            return invNo;
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (invNo.Text == "" || invDate.Date == null || comp.Text == "" || LPONo.Text == "")
            {
                error("Please fill in all fields.");
            }
            else
            {
                string name = invoiceNumber + ".jpg";
                if (sameDeli)
                {
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
                    }
                    catch (Exception ex)
                    {
                        // Log or display the exception message
                        error($"Save failed: {ex.Message}");
                    }
                    saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, LPONo.Text);
                    Frame.Navigate(typeof(CreditScanPage));
                    
                } else
                {
                    //different invoice and delivery notes
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
                        saveDetails(invNo.Text, (DateTimeOffset)invDate.Date, comp.Text, LPONo.Text);
                        System.Diagnostics.Debug.WriteLine(invDate.Date);
                        Frame.Navigate(typeof(DeliveryScanPage));

                    }
                    catch (Exception ex)
                    {
                        // Log or display the exception message
                        error($"Save failed: {ex.Message}");
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
        public static string company;
        public static string LPONumber;
        public static bool sameDeli;

        public static void saveDetails(string invoiceNo, DateTimeOffset invDate, string comp, string LPON)
        {
            invoiceNumber = invoiceNo;
            invoiceDate = invDate;
            company = comp;
            LPONumber = LPON;
        }

    }

}


