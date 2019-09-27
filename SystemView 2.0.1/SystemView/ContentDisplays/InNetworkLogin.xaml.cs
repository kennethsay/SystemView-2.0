using System;
using System.Windows;
using System.Windows.Forms;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace SystemView
{
    public partial class InNetworkLogin : Window
    {
        private static bool _adminAuthenticated;
        static int attempt = 3;

        public static bool AdminAuthenticated
        {
            get
            {
                return _adminAuthenticated;
            }
            set
            {
                _adminAuthenticated = value;
            }
        }

        public InNetworkLogin()
        {
            Login_btn.IsEnabled = false;
            AdminAuthenticated = false;

            InitializeComponent();

            pswd_TextBox.MaxLength = 30;
            pswd_TextBox.PasswordChar = '*';

            pswd_TextBox.PasswordChanged += pswdChanged;
        }

        public void pswdChanged(object sender, RoutedEventArgs e)
        {
            Login_btn.IsEnabled = true;
        }

        private void AdminLoginBtn(object sender, RoutedEventArgs e)
        {
            AdminAuthenticated = false;

            string username = user_TextBox.Text;
            string password = pswd_TextBox.Password;

            try
            {
                Domain domain = Domain.GetComputerDomain();
                string strPath = "LDAP://" + domain.ToString();

                DirectoryEntry entry = new DirectoryEntry(strPath, username, password);
                object nativeObject = entry.NativeObject;

                AdminAuthenticated = true;
            }
            catch
            {
                AdminAuthenticated = false;
            }

            if (AdminAuthenticated)
            {
                System.Windows.MessageBox.Show("Login Successful!");
                return;
            }
            else if (attempt > 0)
            {
                System.Windows.MessageBox.Show(attempt.ToString() + " attempts remain.");
                --attempt;
                pswd_TextBox.Clear();
            }
            else
            {
                Domain domain = Domain.GetComputerDomain();
                string strPath = "LDAP://" + domain.ToString();
                DirectoryEntry entry = new DirectoryEntry(strPath);
                DirectoryEntry dirEntry = entry.Children.Find("CN=" + username, "user");
                dirEntry.Properties["LockOutTime"].Value = 0x0010;

                dirEntry.Close();

                System.Windows.MessageBox.Show("Contact your Admin to reset password.");
                pswd_TextBox.Clear();
            }
        }

        private void CancelLogin(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.Topmost = false;
        }
    }
}
