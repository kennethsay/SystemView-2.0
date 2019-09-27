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
    /// <summary>
    /// Interaction logic for DataPresentationTriggers.xaml
    /// </summary>
    public partial class DataPresentationDisplays : UserControl
    {
        private TagList displayTagList;
        private List<byte> ActiveDisplays;

        public DataPresentationDisplays()
        {
            InitializeComponent();
            displayTagList = new TagList();

            activateDefaultDisplays();
            addDisplaySelectors();
        }

        private void addDisplaySelectors()
        {          

            displayTagList.Tags.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var tag in displayTagList.Tags)
            {
                CheckBox display = new CheckBox
                {
                    Content = tag.Name,
                    Margin = new Thickness(5),
                };

                display.Checked += displaySelect;
                display.Unchecked += displayDeSelect;


                if ((ActiveDisplays.Contains(displayTagList.TagIDByName(tag.Name))))
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
  

        /// <summary>
        /// Activates the default presentation view
        /// </summary>
        private void activateDefaultDisplays()
        {

            try
            {
                ActiveDisplays = new List<byte>();

                ActiveDisplays.Add(displayTagList.TagIDByName("Date Time"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Milepost"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Chainage"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Speed"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Governing Alert Speed"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Governing Brake Speed"));
                ActiveDisplays.Add(displayTagList.TagIDByName("ACSES Mode"));
                ActiveDisplays.Add(displayTagList.TagIDByName("Vehicle Max Speed"));
            }
            catch
            {

            }

        }

        /// <summary>
        /// Event handler for trigger selection checked event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displaySelect(object sender, EventArgs e)
        {
            try
            {
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox display = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveTriggers. 
                if (ActiveDisplays.Contains(displayTagList.TagIDByName(display.Content.ToString())))
                {
                    // Write an error to console stating that the trigger has already been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", display.Content);
                }
                else
                {
                    // Otherwise, add the trigger to the active trigger list. This change will be reflected in the next RTM update. 
                    ActiveDisplays.Add(displayTagList.TagIDByName(display.Content.ToString()));
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DataPresentationTriggers::triggerSelect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Event handler for trigger selection unchecked event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayDeSelect(object sender, EventArgs e)
        {
            try
            {
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox display = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveTriggers. 
                if (ActiveDisplays.Contains(displayTagList.TagIDByName(display.Content.ToString())))
                {
                    // Otherwise, remove the trigger from the active trigger list. This change will be reflected in the next RTM update. 
                    ActiveDisplays.Remove(displayTagList.TagIDByName(display.Content.ToString()));
                }
                else
                {
                    // Write an error to console stating that the trigger has not been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", display.Content);
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DataPresentationTriggers::triggerSelect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        public List<byte> GetDisplays()
        {
            return ActiveDisplays;
        }
    }
}
