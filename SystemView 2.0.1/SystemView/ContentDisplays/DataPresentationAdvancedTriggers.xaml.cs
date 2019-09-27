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
    public partial class DataPresentationAdvancedTriggers : UserControl
    {
        private TagList advancedTriggerTL;
        private List<byte> activeAdvancedTriggers;

        public DataPresentationAdvancedTriggers()
        {
            InitializeComponent();
            advancedTriggerTL = new TagList();

            activateDefaultTriggers();
            addTriggerSelectors();
        }

        private void addTriggerSelectors()
        {

            advancedTriggerTL.Tags.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var tag in advancedTriggerTL.Tags)
            {
                CheckBox trigger = new CheckBox
                {
                    Content = tag.Name,
                    Margin = new Thickness(5),
                };

                trigger.Checked += triggerSelect;
                trigger.Unchecked += triggerDeSelect;


                if (activeAdvancedTriggers.Contains(advancedTriggerTL.TagIDByName(tag.Name)))
                {
                    trigger.IsChecked = true;
                }
                else
                {
                    trigger.IsChecked = false;
                }

                AdvancedTriggerPanel.Children.Add(trigger);
            }
        }

        /// <summary>
        /// Activates the default presentation view trigger list
        /// </summary>
        private void activateDefaultTriggers()
        {
            try
            {
                activeAdvancedTriggers = new List<byte>();

                activeAdvancedTriggers.Add(advancedTriggerTL.TagIDByName("Milepost"));
                activeAdvancedTriggers.Add(advancedTriggerTL.TagIDByName("Chainage"));
                activeAdvancedTriggers.Add(advancedTriggerTL.TagIDByName("Speed"));
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
                if (activeAdvancedTriggers.Contains(advancedTriggerTL.TagIDByName(trigger.Content.ToString())))
                {
                    // Write an error to console stating that the trigger has already been selected by the user.
                    Console.WriteLine("The selected trigger: {0} is already active!", trigger.Content);
                }
                else
                {
                    // Otherwise, add the trigger to the active trigger list. This change will be reflected in the next RTM update. 
                    activeAdvancedTriggers.Add(advancedTriggerTL.TagIDByName(trigger.Content.ToString()));
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
                if (activeAdvancedTriggers.Contains(advancedTriggerTL.TagIDByName(trigger.Content.ToString())))
                {
                    // Otherwise, remove the trigger from the active trigger list. This change will be reflected in the next RTM update. 
                    activeAdvancedTriggers.Remove(advancedTriggerTL.TagIDByName(trigger.Content.ToString()));
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
            return activeAdvancedTriggers;
        }
    }
}
