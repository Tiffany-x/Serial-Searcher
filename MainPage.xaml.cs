using System.Data.SqlClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        static public string access;
        public string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";
        public MainPage()
        {

            this.InitializeComponent();

        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(password.Password);
            SqlConnection loginConn = null;
            SqlDataReader reader = null;
            try
            {
                loginConn = new SqlConnection(connectionString);

                loginConn.Open();

                SqlCommand loginQuery = new SqlCommand(
    "select password, privilege from users where employeeID = @empID", loginConn);
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@empID";
                param.Value = empID.Text;
                loginQuery.Parameters.Add(param);

                reader = loginQuery.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["password"].ToString() == password.Password)
                    {
                        label.Text = "Successfully logged in!";
                        access = reader["privilege"].ToString();
                        Frame.Navigate(typeof(MainWindow));
                    }
                    else
                    {
                        label.Text = "Wrong password";
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
                if (loginConn != null)
                {
                    loginConn.Close();
                }
            }

        }

    }

}
