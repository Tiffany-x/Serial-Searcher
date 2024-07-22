using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using System;
using System.Data.SqlClient;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml;

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
        private string key = "bidco";
        public NewUser()
        {
            this.InitializeComponent();

        }
        private async Task<string> EncryptStringHelper(string plainString, string key)
        {
            try
            {
                var hashKey = GetMD5Hash(key);
                var decryptBuffer = CryptographicBuffer.ConvertStringToBinary(plainString, BinaryStringEncoding.Utf8);
                var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
                var symmetricKey = AES.CreateSymmetricKey(hashKey);
                var encryptedBuffer = CryptographicEngine.Encrypt(symmetricKey, decryptBuffer, null);
                var encryptedString = CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
                return encryptedString;
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


        private async void create_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string encrypted = await EncryptStringHelper(password.Password, key);
            System.Diagnostics.Debug.WriteLine(encrypted);
            try
            {
                createUserConn = new SqlConnection(connectionString);

                createUserConn.Open();

                SqlCommand userQuery = new SqlCommand(
    "INSERT INTO users(employeeID, privilege, password, createdAt) " +
    "VALUES(@employeeID, @privilege, @password, @createdAt)", createUserConn);

                userQuery.CommandType = System.Data.CommandType.Text;

                userQuery.Parameters.AddWithValue("@employeeID", empID.Text);
                if (privilegeCombo.SelectedIndex == 0)
                {
                    userQuery.Parameters.AddWithValue("@privilege", "Manager");

                }
                else 
                {
                    userQuery.Parameters.AddWithValue("@privilege", "user");

                }

                userQuery.Parameters.AddWithValue("@password", await EncryptStringHelper(password.Password, key));


                userQuery.Parameters.AddWithValue("@createdAt", DateTime.Now);
                try
                {
                    userQuery.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Not recorded: " + ex.Message);
                }
                System.Diagnostics.Debug.WriteLine("New user has been created");
            }
            
            finally
            {
                empID.Text = "";
                password.Password = "";
                privilegeCombo.SelectedIndex = -1;
                createUserConn.Close();
            }

            }
    }
}
