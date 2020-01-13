using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using System.ComponentModel;

namespace SystemView
{

    // To call DisplayView = new DisplayView();
    //
    // CLASS: DisplayView
    //
    // Description: This class implements a structure to contain the individual PTE functions of the application. Each function creates a DisplayView
    //              that is wrapped inside a RadPane within the Session docking mechanism. Multiple DisplayViews can be placed within a single Session, 
    //              and the _contentType variables is used to define the particular PTE function presented by the display. 
    //
    // Private Data:
    //      string _header                              - Contains the name of the display to be shown in the content header
    //      Type _contentType                           - Contains the class type of the PTE function contained within the display. This is used to actually generate the display from the class type
    //      DockState _initialPosition                  - Specifies the initial docking position (Top, Bottom, Left, Right, Floating) of the display view within the Session Docking mechanism
    //      DISPLAY_TYPE _displayType                   - Used to specify the type of function contained within the display view for external purposes
    //
    // Public Get/Set Accessors:
    //      DISPLAY_TYPE DisplayType(get/set)           - Accessor for _displayType
    //      Type ContentType(get/set)                   - Accessor for _contentType
    //      string Header(get/set)                      - Accessor for _header
    //      DockState InitialPosition(get/set)          - Accessor for _initialPosition
    //
    // Public Methods:
    //      string ToString()                           - Override of ToString method for DisplayView Class
    //
    // Private Methods:
    //
    // Constructors:
    //      DisplayView(Type contentType)
    //
    // Other DataTypes:
    //
    class DisplayView : INotifyPropertyChanged
    {
        #region PrivateMembers
        private string _header;
        private Type _contentType;
        private DockState _initialPosition;
        private DISPLAY_TYPE _displayType;
        #endregion

        #region Accessors
        public DISPLAY_TYPE DisplayType
        {
            get
            {
                return this._displayType;
            }
            set
            {               
                this._displayType = value;                
            }
        }

        public Type ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                this._contentType = value;
                this.OnPropertyChanged("ContentType");
            }
        }

        public string Header
        {
            get
            {
                return this._header;
            }
            set
            {
                this._header = value;
                this.OnPropertyChanged("Header");
            }
        }

        public DockState InitialPosition
        {
            get
            {
                return this._initialPosition;
            }
            set
            {
                this._initialPosition = value;
                this.OnPropertyChanged("InitialPosition");
            }
        }
        #endregion

        #region Constructors

        
        /// <summary>
        /// DisplayView Constructor that sets the Content Type via specified argument 
        /// </summary>
        /// <param name="contentType">Type of PTE function to be displayed within the DisplayView</param>
        public DisplayView(Type contentType)
        {
            // Set the ContentType
            this.ContentType = _contentType;
        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Override of ToString method for DisplayView Class
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DisplayView Properties - Header: {0}, ContentType: {1}, Initial Position: {2}, Display Type: {3}",
                                        _header, _contentType.ToString(), _initialPosition.ToString(), _displayType.ToString()));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DisplayView::ToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        #endregion

        #region PropertyChangeManager
        // This class implements the property changed manager feature of C#/WPF
        // The property changed manager is a UI paradigm used to automatically forward updates to background logic
        // to their respective presentation devices within the presentation layer. The property changed event handler 
        // must be implemented to cause a property to send a UI update request when the property has been modified

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
