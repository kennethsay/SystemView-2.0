using System;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Text;


namespace SystemView
{
    // CertificateLogin.ClientLogin();


    // CLASS: CertificateLogin
    //
    // Description: This class reads the certificate from a smart card and authenticates the user.
    //
    // Usability: User must be on their own computer or create a certificate for use on a different computer.
    // 
    // Private Data
    //      _pin - Holds the users PKI pin
    //      _cert  - Holds the users certificate
    //
    // Public Get/Set Accessors
    //      string Domain (Get & Set)
    //      string Username (Get & Set)
    //      SecureString Password (Get & Set)
    //
    // Public Methods
    //      ClientLogin()   - Aunthenicates the user
    //
    // Private Methods
    //      getClientCertificate()  - Retrieves certificate from smart card
    //      certificateToString(X509Certificate2 cert)  - Turns the certificate to strings and stores as UserLoginInfo
    //
    // Constructors
    //      CertificateLogin()      - Default constructor, set _pin and _cert to null
    //
    // Public Overrides
    //

    /// <summary>
    /// CertificateLogin is a public class that reads the information from a certificate
    /// and authenticates the user with their entered PKI password.
    /// </summary>
    public class CertificateLogin
    {
        #region Members/Built-In Methods
        /// <summary>
        /// This is a Built-In Method. It takes a Certificate and converts it to a String.
        /// This is essentially a "ToString" Method. Uses advapi32.dll for the conversion.
        /// The names are the same as on Microsoft Docs.
        /// </summary>
        /// <param name="credType">Type of Credential to marshal; Ours is 1!</param>
        /// <param name="credential">Pointer to the Certificate Credentials</param>
        /// <param name="marshaledCredential">Pointer to the Null terminated string of the marshalled credential</param>
        /// <returns>True if conversion is successful, otherwise false.</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredMarshalCredential
            (
                int credType,
                IntPtr credential,
                out IntPtr marshaledCredential
            );

        /// <summary>
        /// This is a Built-In Structure for Certificates. It simply references the Certificate.
        /// The names are the same as on Microsoft Docs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_CREDENTIAL_INFO
        {
            // Size of structure in bytes
            public uint cbSize;
            // Defines the number of elements in the hash array below as unmanaged memory
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            // SHA-1 hash of certificate
            public byte[] rgbHashOfCert;
        }

        // Users PKI password
        private static string _pin;

        public static string Pin
        {
            get { return _pin; }
            set { _pin = value; }
        }
        // Users Certificate
        private static X509Certificate2 _cert;

        /// <summary>
        /// Structure to hold User Login Information; All three parts are needed to keep the Stack Happy.
        /// All members are private with Accessor Functions.
        /// </summary>
        internal struct UserLoginInfo
        {
            #region Member Fields
            private string _domain;
            private string _username;
            private SecureString _password;
            #endregion

            #region Accessor Functions
            public string Domain
            {
                get { return this._domain; }
                set { this._domain = value; }
            }

            public string Username
            {
                get { return this._username; }
                set { this._username = value; }
            }

            public SecureString Password
            {
                get { return this._password; }
                set { this._password = value; }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Default constructor for Certificate Login. Sets _pin & _cert to null.
        /// </summary>
        public CertificateLogin()
        {
            Pin = null;
            _cert = null;
        }

        #region Methods
        /// <summary>
        /// This is the beating heart of the PKI authentication. It retrieves the certificate by calling the getClientCertificate.
        /// It transforms the Certificate to Strings by calling certificateToString method.
        /// Finally it has an automatic process to authenticate the user. 
        /// </summary>
        public static void ClientLogin()
        {
            try
            {
                // Called when card has been detected.
                _cert = getClientCertificate();

                if (_cert != null && Pin != string.Empty)
                {
                    UserLoginInfo user = certificateToString(_cert);

                    if (user.Username != string.Empty)
                    {
                        try
                        {
                            // Initiate the start of the automated password check process, thanks to Microsoft!
                            ProcessStartInfo psi = new ProcessStartInfo();
                            // Define the next path the Window should open. Executable Files only!
                            psi.FileName = "SuccessfulSignIn.exe";
                            psi.UseShellExecute = false;
                            psi.Domain = user.Domain;
                            psi.UserName = user.Username;
                            psi.Password = user.Password;
                            Process processChild = Process.Start(psi);

                            ContentDisplays.EmployeeLogin.UserAuthenticated = true;
                            MainWindow thisCardState = new MainWindow();
                            thisCardState.CurrentCardState = MainWindow.SmartcardState.Inserted;
                            thisCardState.CardReaderState();
                        }
                        catch
                        {
                            MessageBox.Show("Error Please Try Again.");

                            ContentDisplays.EmployeeLogin.UserAuthenticated = false;

                            MainWindow thisCardState = new MainWindow();
                            thisCardState.CurrentCardState = MainWindow.SmartcardState.Inserted;
                            thisCardState.CardReaderState();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CertificateLogin::ClientLogin-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }
        /// <summary>
        /// getClientCertificate retrives the Certificates in the "Personal" Certificate store. The first Certificate
        /// that has the Client Authentication Policy (associated with OID # "1.3.6.1.5.5.7.3.2") is returned.
        /// </summary>
        /// <returns>The Client Certificate, not yet a string.</returns>
        private static X509Certificate2 getClientCertificate()
        {
            try
            {
                X509Certificate2 certificate = null;
                // Get Client Certificate from Personal Store
                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                    // Find the Certificate that will Authenticate the User
                    X509Certificate2Collection CAPcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByApplicationPolicy, "1.3.6.1.5.5.7.3.2", true);
                    // Can add all of the Certificates that match this criteria to array and have user choose the certificate
                    if (CAPcollection != null)
                    {
                        certificate = CAPcollection[0];
                    }
                }
                finally
                {
                    store.Close();
                }
                return certificate;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CertificateLogin::getClientCertificate-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                return null;
            }
        }
        /// <summary>
        /// This method converts the retreived Certificate to the UserLoginInfo Struct for the application.
        /// UserLoignInfo contains the string Domain, string Username, and SecureString Password.
        /// This is essentially the ToString Method for Certificates.
        /// </summary>
        /// <param name="cert">The client certificate straight from the Smart Card</param>
        /// <returns>UserLoginInfo Structure is conversion successful, else null</returns>
        private static UserLoginInfo certificateToString(X509Certificate2 cert)
        {
            try
            {
                UserLoginInfo uli = new UserLoginInfo();
                try
                {
                    CERT_CREDENTIAL_INFO certInfo = new CERT_CREDENTIAL_INFO();
                    //Setting fields of the built-in structure to hold Certificates
                    certInfo.cbSize = (uint)Marshal.SizeOf(typeof(CERT_CREDENTIAL_INFO));
                    certInfo.rgbHashOfCert = cert.GetCertHash();

                    int size = Marshal.SizeOf(certInfo);
                    // CertInfo pointer points to allocated heap memory from unmanaged memory of the process; locks allocated memory, and fills with non-zeroes
                    IntPtr ptrCertInfo = Marshal.AllocHGlobal(size);

                    // Inserts the Certificate Structure to the unmanaged part of memory, without deleting original struct
                    Marshal.StructureToPtr(certInfo, ptrCertInfo, false);
                    // Constant Value that represents a NULL pointer
                    IntPtr marshaledCredential = IntPtr.Zero;
                    bool result = CredMarshalCredential(1, ptrCertInfo, out marshaledCredential);
                    // True for Success/ False for Failure

                    SecureString sp = new SecureString();
                    foreach (char c in Pin)
                    { // Copying PKI key to SecurePassword
                        sp.AppendChar(c);
                    }

                    if (result)
                    { // Copying Strings from Certificate if successfully converted to string
                        uli.Domain = Environment.UserDomainName;
                        uli.Username = Marshal.PtrToStringUni(marshaledCredential);
                        uli.Password = sp;
                    }
                    else
                    { // If not null, then these fields could potentially be filled with junk values, this is done for accuracy
                        uli.Domain = null;
                        uli.Username = null;
                        uli.Password = null;
                    }
                    // Free allocated heap memory
                    Marshal.FreeHGlobal(ptrCertInfo);
                    return uli;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return uli;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("CertificateLogin::certificateToString-Threw Exception {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
                UserLoginInfo badInfo = new UserLoginInfo();
                badInfo.Domain = null;
                badInfo.Username = null;
                badInfo.Password = null;
                return badInfo;
            }
        }
        #endregion
    }
}
