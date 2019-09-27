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
    public partial class DataPresentationTriggers : UserControl
    {
        private TagList triggerTagList;
        private List<byte> ActiveTriggers;
        
        public DataPresentationTriggers()
        {
            InitializeComponent();
            triggerTagList = new TagList();

            activateDefaultTriggers();
            addTriggerSelectors();
        }

        private void addTriggerSelectors()
        {           

            triggerTagList.Tags.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var tag in triggerTagList.Tags)
            {
                CheckBox trigger = new CheckBox
                {
                    Content = tag.Name,
                    Margin = new Thickness(5),
                };

                trigger.Checked += triggerSelect;
                trigger.Unchecked += triggerDeSelect;


                if (ActiveTriggers.Contains(triggerTagList.TagIDByName( tag.Name)))
                {
                    trigger.IsChecked = true;
                }
                else
                {
                    trigger.IsChecked = false;
                }

                TriggerPanel.Children.Add(trigger);
            }
        }


        /// <summary>
        /// Activates the default presentation view trigger list
        /// </summary>
        private void activateDefaultTriggers()
        {
            try
            {
                ActiveTriggers = new List<byte>();

                ActiveTriggers.Add(triggerTagList.TagIDByName("Milepost"));
                ActiveTriggers.Add(triggerTagList.TagIDByName("Chainage"));
                ActiveTriggers.Add(triggerTagList.TagIDByName("Speed"));                
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
        private void triggerSelect(object sender, EventArgs e)
        {
            try
            {
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox trigger = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveTriggers. 
                if (ActiveTriggers.Contains(triggerTagList.TagIDByName(trigger.Content.ToString())))
                {
                    // Write an error to console stating that the trigger has already been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", trigger.Content);
                }
                else
                {
                    // Otherwise, add the trigger to the active trigger list. This change will be reflected in the next RTM update. 
                    ActiveTriggers.Add(triggerTagList.TagIDByName(trigger.Content.ToString()));
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
        private void triggerDeSelect(object sender, EventArgs e)
        {
            try
            {
                // Convert the sender object to the Checkbox type. This lets us get to the content property of the control. 
                CheckBox trigger = sender as CheckBox;

                // Check to see if this checkbox has already been checked by the user. If so it will show up in ActiveTriggers. 
                if (ActiveTriggers.Contains(triggerTagList.TagIDByName(trigger.Content.ToString())))
                {
                    // Otherwise, remove the trigger from the active trigger list. This change will be reflected in the next RTM update. 
                    ActiveTriggers.Remove(triggerTagList.TagIDByName(trigger.Content.ToString()));
                }
                else
                {
                    // Write an error to console stating that the trigger has not been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", trigger.Content);                    
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DataPresentationTriggers::triggerSelect method threw exception: {0}", ex.ToString()));
                Console.WriteLine(sb.ToString());
            }
        }

        public List<byte> GetTriggers()
        {
            return ActiveTriggers;
        }
    }
}
