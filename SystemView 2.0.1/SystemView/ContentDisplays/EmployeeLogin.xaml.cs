using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for DatalogDownload.xaml
    /// </summary>
    public partial class EmployeeLogin : Window
    {
        private static bool _userAuthenticated;

        public static bool UserAuthenticated
        {
            get
            {
                return _userAuthenticated;
            }
            set
            {
                _userAuthenticated = value;
            }
        }

        public EmployeeLogin()
        {
            UserAuthenticated = false;

            InitializeComponent();

            Verify_btn.IsEnabled = false;

            pkiPin_TextBox.MaxLength = 8;
            pkiPin_TextBox.PasswordChar = '*';

            pkiPin_TextBox.PasswordChanged += PKIpinEntered;
        }

        public void PKIpinEntered(object sender, RoutedEventArgs e)
        {
            Verify_btn.IsEnabled = true;
        }

        public void ContinueEmployeeLogin(object sender, RoutedEventArgs e)
        {
            Verify_btn.IsEnabled = false;
            CertificateLogin.Pin = pkiPin_TextBox.Password;
            CertificateLogin.ClientLogin();

            if (UserAuthenticated == true)
            {
                this.Close();
            }
            else
            {
                pkiPin_TextBox.Clear();
                Verify_btn.IsEnabled = true;
            }
        }

        public void CancelLogin(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.Topmost = false;
        }
    }
}
