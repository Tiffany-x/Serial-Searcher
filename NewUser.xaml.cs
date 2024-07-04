using System;
using System.Data.SqlClient;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class NewUser : Page
    {
        private string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";
        private SqlConnection createUserConn = null;

        public NewUser()
        {
            this.InitializeComponent();
        }

        private void create_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                createUserConn = new SqlConnection(connectionString);

                createUserConn.Open();

                SqlCommand invoiceQuery = new SqlCommand(
    "INSERT INTO user(employeeID, privilege, password, createdAt) " +
    "VALUES(@employeeID, @privilege, @password, @createdAt)", createUserConn);

                invoiceQuery.CommandType = System.Data.CommandType.Text;

                invoiceQuery.Parameters.AddWithValue("@employeeID", empID.Text);
                if (privilegeCombo.SelectedIndex == 0)
                {
                    invoiceQuery.Parameters.AddWithValue("@privilege", "Manager");

                }
                else if (privilegeCombo.SelectedIndex == 1)
                {
                    invoiceQuery.Parameters.AddWithValue("@privilege", "user");

                }

                invoiceQuery.Parameters.AddWithValue("@password", password.Text);


                invoiceQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);
            } finally
            {
                createUserConn.Close();
            }

            }
    }
}
