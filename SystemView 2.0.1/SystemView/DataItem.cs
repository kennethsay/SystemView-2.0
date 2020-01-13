using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.ComponentModel;

using AppLogic;

namespace SystemView
{

    // To call DataItem = new DataItem();
    //
    // CLASS: DataItem
    //
    // Description: This class implements the data structure used to generate the Data Presentation table real-time records. 
    //              Each structure represents a single OBC Datalog record that is parsed from the TagList passed into the constructor.
    //              A data binding to the presentation data table is used to add records to the datalog view table. 
    //
    // Private Data:
    //      TagList _dataTagList                        - TagList from which the individual data record is generated
    //      UInt32 _dataIndex                           - Index corresponding to the position of the current record within the stream of data records. 
    //      IDictionary<string, object> _data           - Dictionary used to store data table information in the form of a column header and row data
    //
    // Public Get/Set Accessors:
    //      TagList DataTagList                         - Accessor for _dataTagList variable
    //      UInt32 Index                                - Accessor for _dataIndex variable
    //
    // Public Methods:
    //      string ToString()                           - Override of ToString method for DataItem Class
    //
    // Private Methods:
    //
    // Constructors:
    //      DataItem()
    //      DataItem(TagList Tags, UInt32 Index)
    //
    // Other DataTypes:
    //
    public class DataItem : DynamicObject, INotifyPropertyChanged
    {
        #region PrivateMembers
        private TagList _dataTagList;
        private UInt32 _dataIndex;
        readonly IDictionary<string, object> data;
        #endregion

        #region Accessors
        public TagList DataTagList
        {
            get
            {
                return _dataTagList;
            }
        }

        public UInt32 Index
        {
            get
            {
                return _dataIndex;
            }
        }
        #endregion

        #region Constructors

        // This is the default constructor. Just create a new blank data object to hold our data record information
        // The dictionary created here is used to tie a piece of data with a data table column header
        // The string placed in this dictionary index is used to create a column header that corresponds with the name
        // of each tag in the TagList.
        // The object associated with each string is the value of the parameter specified by the string. This data is
        // populated in each row of the data table with its corresponding column. 

        /// <summary>
        /// Default constructor that creates an empty data record Dictionary
        /// </summary>
        public DataItem()
        {
            data = new Dictionary<string, object>();
        }

        // This constructor takes a TagList and an index as arguments and generates a displayable record.
        // The TagList is populated only with tags that should be displayed in the current record. These are parameters
        // that are currently be triggered by the user. The index given is a running index specifying the number of records 
        // that have been recorded.

        /// <summary>
        /// This constructor takes a TagList and an index as arguments and generates a displayable record.
        /// </summary>
        /// <param name="Tags">TagList from which the Data Item will be generated</param>
        /// <param name="Index">Current count of records that have been recorded</param>
        public DataItem(TagList Tags, UInt32 Index)
        {
            try
            {
                // First verify that neither of the arguments are null
                if (Tags != null && Index != null)
                {
                    // Looks good, so create a new Dictionary to populate
                    data = new Dictionary<string, object>();

                    // Save the Index and TagList for future use
                    _dataIndex = Index;
                    _dataTagList = Tags;

                    // Add a specific column entry for the record index. 
                    data.Add("Index", _dataIndex);

                    // Finally, add column entries for each Tag in the TagList. 
                    // This is using the Tag name and the data associated with the individual tag.
                    foreach (Tag tag in _dataTagList.Tags)
                    {
                        data.Add(tag.Name, tag.Data());
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataItem::Constructor-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
            }

        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Override of ToString method for DataItem Class
        /// </summary>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("Data Item Index: {0}", _dataIndex));
                foreach (Tag t in _dataTagList.Tags)
                {
                    sb.Append(String.Format("Tag Info: {0}", t.ToString()));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("RadioDataItem::ToString-threw exception {0}", ex.ToString()));

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

        #region DataStructureOverrides
        // The following data structure overrides are required by Telerik to successfully bind a data dictionary with
        // the UI data table implemented in the UI

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.data.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        public object this[string columnName]
        {
            get
            {
                if (this.data.ContainsKey(columnName))
                {
                    return this.data[columnName];
                }

                return null;
            }
            set
            {
                if (!this.data.ContainsKey(columnName))
                {
                    this.data.Add(columnName, value);

                    this.OnPropertyChanged(columnName);
                }
                else
                {
                    if (this.data[columnName] != value)
                    {
                        this.data[columnName] = value;

                        this.OnPropertyChanged(columnName);
                    }
                }
            }
        }
        #endregion
    }
}
