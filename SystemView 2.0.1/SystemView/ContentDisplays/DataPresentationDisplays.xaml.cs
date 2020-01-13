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

using SystemView.ContentDisplays;
using AppLogic;

namespace SystemView.ContentDisplays
{
    // To call DataPresentationDisplays = new DataPresentationDisplays();
    //
    // CLASS: DataPresentationDisplays
    //
    // Description: This class implements UI and back-end logic for the DataPresentation parameter filtering function of the PTE. This class initializes a WPF 
    //              component that builds the front end display of the DataPresentation parameter filter. The class is instanciated from a DisplayView instance that places
    //              this WPF display inside the Session Docking mechanism. This class is instanciated by the DataPresentation object when the user selects the option to modify
    //              the list of parameters shown. The UI for this class presents the user with a list of DataPresentation parameters that can be selected via a checkbox. 
    //              Each parameter can be added/removed from the DataPresentation screen by checking/unchecking the checkbox associated with the
    //              parameter.
    //
    // Private Data:
    //      TagList _displayTagList                                 - Variable containing a reference TagList to aid in generating the Display parameter selection screen
    //      List<byte> _activeDisplays                              - List containing the Tag ID of the currently selected Data Presentation parameters to be displayed in the
    //                                                                Data Presentation window.
    //
    // Public Get/Set Accessors:
    //      List<byte> ActiveDisplays                               - Accessor method for _activeDisplays variable 
    //
    // Public Methods:
    //      string ToString()                                       - Override of ToString method for DatalogDownloadBySize Class
    //
    // Private Methods:
    //
    // Constructors:
    //      DatalogDownloadBySize()
    //
    // Other DataTypes:
    //
    public partial class DataPresentationDisplays : UserControl
    {
        #region PrivateMembers
        private TagList _displayTagList;
        private List<byte> _activeDisplays;
        #endregion

        public List<byte> ActiveDisplays
        {
            get
            {
                return _activeDisplays;
            }            
        }

        public DataPresentationDisplays()
        {
            InitializeComponent();
            _displayTagList = new TagList();

            activateDefaultDisplays();
            addDisplaySelectors();
        }

        private void addDisplaySelectors()
        {          

            _displayTagList.Tags.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var tag in _displayTagList.Tags)
            {
                //these tag IDs correspond to tags that are not used as data parameters
                if (!(tag.TagID == 13 | tag.TagID == 14 | tag.TagID == 15 | tag.TagID == 32 | tag.TagID == 63 | tag.TagID == 64 | tag.TagID == 70))
                {
                    CheckBox display = new CheckBox
                    {
                        Content = tag.Name,
                        Margin = new Thickness(5),
                    };

                    display.Checked += displaySelect;
                    display.Unchecked += displayDeSelect;

                    if ((_activeDisplays.Contains(_displayTagList.TagIDByName(tag.Name))))
                    {
                        display.IsChecked = true;
                    }
                    else
                    {
                        display.IsChecked = false;
                    }

                    DisplayPanel.Children.Add(display);
                }
            }            
        }
  

        /// <summary>
        /// Activates the default presentation view od the Data Presentation window.
        /// This places the selected default tags in the write 
        /// </summary>
        private void activateDefaultDisplays()
        {

            try
            {
                _activeDisplays = new List<byte>();

                _activeDisplays.Add(_displayTagList.TagIDByName("DATETIME"));
                _activeDisplays.Add(_displayTagList.TagIDByName("LOCATION"));
                _activeDisplays.Add(_displayTagList.TagIDByName("CHAINAGE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("SPEED"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ALERT CURVE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("BRAKE CURVE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ACSES MODE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("VEHICLE MAX SPEED"));
                _activeDisplays.Add(_displayTagList.TagIDByName("RR LINE #"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TRACK #"));
                _activeDisplays.Add(_displayTagList.TagIDByName("LINK TARGET"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TRACK LIMIT"));
                _activeDisplays.Add(_displayTagList.TagIDByName("PTS ALERT CURVE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("PTS BRAKE CURVE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("PTSZONE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ACTIVECAB"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ALARM"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ACKNOWLEDGE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("OVERSPEED"));
                _activeDisplays.Add(_displayTagList.TagIDByName("SIGNAL STATUS"));
                _activeDisplays.Add(_displayTagList.TagIDByName("DECEL VALUE"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ACSES CIO"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ACSES CO OUT"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ATC CIO"));
                _activeDisplays.Add(_displayTagList.TagIDByName("ATC CO"));
                _activeDisplays.Add(_displayTagList.TagIDByName("PERMSUPPR"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TPFOUND"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TPMISSING"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TPOUTOFWIN"));
                _activeDisplays.Add(_displayTagList.TagIDByName("RADIOTXRXENC"));
                _activeDisplays.Add(_displayTagList.TagIDByName("RADIOTXRXTSR"));
                _activeDisplays.Add(_displayTagList.TagIDByName("TSRLISTOK"));
                _activeDisplays.Add(_displayTagList.TagIDByName("SERVICE PEN"));
                _activeDisplays.Add(_displayTagList.TagIDByName("EM PENALTY"));
                _activeDisplays.Add(_displayTagList.TagIDByName("REVHANDLE"));
            }
            catch
            {

            }
        }

        /// <summary>
        /// Event handler for Display selection checked event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displaySelect(object sender, EventArgs e)
        {
            try
            {
               
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox display = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveDisplays. 
                if (_activeDisplays.Contains(_displayTagList.TagIDByName(display.Content.ToString())))
                {
                    // Write an error to console stating that the Display has already been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", display.Content);
                }
                else
                {
                   
                    // Otherwise, add the display to the active display list. This change will be reflected in the next RTM update. 
                    _activeDisplays.Add(_displayTagList.TagIDByName(display.Content.ToString()));
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DataPresentationDisplays::displaySelect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Event handler for display selection unchecked event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayDeSelect(object sender, EventArgs e)
        {
            try
            {
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox display = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveDisplays. 
                if (_activeDisplays.Contains(_displayTagList.TagIDByName(display.Content.ToString())))
                {
                    // Otherwise, remove the Display from the active Display list. This change will be reflected in the next RTM update. 
                    _activeDisplays.Remove(_displayTagList.TagIDByName(display.Content.ToString()));
                }
                else
                {
                    // Write an error to console stating that the Display has not been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", display.Content);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DataPresentationDisplays::DisplayDeSelect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }        
    }
}
