using NuGet.Protocol.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class CreditScanPage : Page
    {
        public static string creditPath;
        private DeviceWatcher scannerWatcher;
        public static string creditNumber = "";
        public static int devices = 1;


        public CreditScanPage()
        {
            this.InitializeComponent();
            mainStack.Width = Window.Current.Bounds.Width * .9;
            stack0.Width = Window.Current.Bounds.Width * 0.9;
            stack1.Width = Window.Current.Bounds.Width * 0.9 / 2;
            stack2.Width = Window.Current.Bounds.Width * 0.9 / 2;
            credNo.Text = creditNumber;

            invDate.MaxDate = DateTime.Now;
            invDate.Date = InvoiceScanPage.invoiceDate;


            

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

                    // Enable save and cancel buttons
                    Next.IsEnabled = true;
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


        private void clearData()
        {
            credNo.Text = "";
        }
        private void cancelScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the displayed image and disable save and cancel buttons
            ScannedImage.Source = null;
            Next.IsEnabled = false;
            cancelScanButton.IsEnabled = false;
        }

        async Task<StorageFolder> PickFolderAsync()
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            }
            return folder;
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            creditNumber = credNo.Text;
            string name = creditNumber + ".jpg";
            try
            {
                string folderName = "Credit Notes";
                StorageFolder documentsFolder = KnownFolders.DocumentsLibrary;
                StorageFolder folder = await documentsFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                // Perform the full scan to the specified folder
                StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                creditPath = file.Path;
                System.Diagnostics.Debug.WriteLine("Image path: " + creditPath);

                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                    encoder.SetSoftwareBitmap(scannedBitmap);
                    await encoder.FlushAsync();
                }


                var dialog1 = new DeviceRequest();
                var result = await dialog1.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    System.Diagnostics.Debug.WriteLine("cancelled");
                    dialog1.Hide();
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    devices = Int32.Parse(dialog1.Text);
                    clearData();
                    Frame.Navigate(typeof(DeviceDetails), devices);
                    dialog1.Hide();
                }


                // Disable save and cancel buttons
            }
            catch (Exception ex)
            {
                // Log or display the exception message
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
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

        
        private void backButton_Click(object sender, RoutedEventArgs e)
        { 
            if (InvoiceScanPage.sameDeli)
            {
                Frame.Navigate(typeof(InvoiceScanPage));
            } else
            {
                Frame.Navigate(typeof(DeliveryScanPage));
            }
            
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (invDate != null)
            {
                StorageFile imageFile = await StorageFile.GetFileFromPathAsync(creditPath);
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
        }
    }

}


