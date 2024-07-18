using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Page
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void mainNavView_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainPage.access == "user")
            {
                navItem_addDevicePage.Visibility = Visibility.Collapsed;
                navItem_createUserPage.Visibility = Visibility.Collapsed;
                mainNavView.SelectedItem = mainNavView.MenuItems[0];
                // If navigation occurs on SelectionChanged, this isn't needed.
                // Because we use ItemInvoked to navigate, we need to call Navigate
                // here to load the home page.
                mainNavView_Navigate(typeof(SearchDevice), new EntranceNavigationTransitionInfo());
            } else
            {
                mainNavView.SelectedItem = mainNavView.MenuItems[0];
                // If navigation occurs on SelectionChanged, this isn't needed.
                // Because we use ItemInvoked to navigate, we need to call Navigate
                // here to load the home page.
                mainNavView_Navigate(typeof(InvoiceScanPage), new EntranceNavigationTransitionInfo());
            }

            // mainNavView doesn't load any page by default, so load home page.
            
        }

        private void mainNavView_ItemInvoked(NavigationView sender,
                                         NavigationViewItemInvokedEventArgs args)
        {
            var section = args.InvokedItemContainer;
            switch (section.Name)
            {
                case "navItem_addDevicePage":
                    ContentFrame.Navigate(typeof(InvoiceScanPage));
                    break;
                case "navItem_searchDevicePage":
                    ContentFrame.Navigate(typeof(SearchDevice));
                    break;
                case "navItem_createUserPage":
                    ContentFrame.Navigate(typeof(NewUser));
                    //ContentFrame.Navigate(typeof(NewUser));
                    break;

            }
        }

        private void mainNavView_Navigate(
            Type navPageType,
            NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType != null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }

        private void mainNavView_BackRequested(NavigationView sender,
                                           NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (mainNavView.IsPaneOpen &&
                (mainNavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 mainNavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            mainNavView.IsBackEnabled = ContentFrame.CanGoBack;

            /*if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of mainNavView.MenuItems, and doesn't have a Tag.
                mainNavView.SelectedItem = (NavigationViewItem)mainNavView.SettingsItem;
                mainNavView.Header = "Settings";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                // Select the nav view item that corresponds to the page being navigated to.
                mainNavView.SelectedItem = mainNavView.MenuItems
                            .OfType<NavigationViewItem>()
                            .First(i => i.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()));

                mainNavView.Header =
                    ((NavigationViewItem)mainNavView.SelectedItem)?.Content?.ToString();

            }*/
        }

        private void Log_Out_cmd(object sender, RoutedEventArgs e)
        {
            MainPage.access = "";
            Frame.Navigate(typeof(MainPage));
            InvoiceScanPage.invoicePath = "";
            InvoiceScanPage.invoiceNumber = "";
            InvoiceScanPage.invoiceDate = DateTimeOffset.Now;
            DeliveryScanPage.deliPath = "";
            DeliveryScanPage.deliveryNumber = "";
            DeliveryScanPage.deliveryDate = DateTimeOffset.Now;
            CreditScanPage.creditNumber = "";
            CreditScanPage.creditPath = "";
        }
    }
}
