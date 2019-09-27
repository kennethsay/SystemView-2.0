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
    public class DataItem : DynamicObject, INotifyPropertyChanged
    {
        private TagList _dataTagList;
        private UInt32 _dataIndex;
        readonly IDictionary<string, object> data;

        public DataItem()
        {
            data = new Dictionary<string, object>();
        }

        public DataItem(TagList Tags, UInt32 Index)
        {
            if (Tags != null && Index != null)
            {
                data = new Dictionary<string, object>();

                _dataIndex = Index;
                _dataTagList = Tags;

                data.Add("Index", _dataIndex);

                foreach (Tag tag in _dataTagList.Tags)
                {
                    data.Add(tag.Name, tag.Data());
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
    }




}
