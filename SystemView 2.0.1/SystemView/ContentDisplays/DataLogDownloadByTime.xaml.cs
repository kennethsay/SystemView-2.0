using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

using Prism.Commands;

using AppLogic;

namespace SystemView.ContentDisplays
{
    /// <summary>
    /// Interaction logic for DataLogDownloadByTime.xaml
    /// </summary>
    public partial class DataLogDownloadByTime : UserControl
    {
        private Datalog.DATA_BYTIME_SELECTED _selectedTimeInterval;

        public Datalog.DATA_BYTIME_SELECTED SelectedTimeInterval { get; set; }

        public DataLogDownloadByTime()
        {
            InitializeComponent();
            this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_48;
        }       

        

        public void TimeChangeCommand(object sender, EventArgs e)
        {
            try
            {
                var RadioBtnParams = (sender as RadioButton).CommandParameter;

                switch (RadioBtnParams)
                {
                    case "48HR":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_48;
                        break;

                    case "24HR":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_24;
                        break;

                    case "8HR":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_08;
                        break;

                    case "4HR":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_04;
                        break;

                    case "1HR":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.HR_01;
                        break;

                    case "30MIN":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.MIN_30;
                        break;

                    case "15MIN":
                        this.SelectedTimeInterval = Datalog.DATA_BYTIME_SELECTED.MIN_15;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        
    }
}

