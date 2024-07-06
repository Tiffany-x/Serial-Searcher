using System.Data.SqlClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using System;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography.Core;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SerialSearcher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private string key = "bidco";
        static public string access;
        public string connectionString = @"Data Source = SPARE3-DIT\SQLEXPRESS; Initial Catalog = serial_searcher; Integrated Security = true;";
        public MainPage()
        {

            this.InitializeComponent();

        }

        private async Task<string> DecryptStringHelper(string encryptedString, string key)
        {
            try
            {
                var hashKey = GetMD5Hash(key);
                IBuffer decryptBuffer = CryptographicBuffer.DecodeFromBase64String(encryptedString);
                var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
                var symmetricKey = AES.CreateSymmetricKey(hashKey);
                var decryptedBuffer = CryptographicEngine.Decrypt(symmetricKey, decryptBuffer, null);
                string decryptedString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedBuffer);
                return decryptedString;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private static IBuffer GetMD5Hash(string key)
        {
            IBuffer bufferUTF8Msg = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer hashBuffer = hashAlgorithmProvider.HashData(bufferUTF8Msg);
            if (hashBuffer.Length != hashAlgorithmProvider.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }
            return hashBuffer;
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
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
                    string decrypted = await DecryptStringHelper(reader["password"].ToString(), key);
                    System.Diagnostics.Debug.WriteLine(decrypted);
                    if (decrypted == password.Password)
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
