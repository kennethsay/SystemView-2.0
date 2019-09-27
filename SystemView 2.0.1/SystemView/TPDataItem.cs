using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.ComponentModel;

using AppLogic;
using SystemView.ContentDisplays;

namespace SystemView
{
    public class TPDataItem : DynamicObject, INotifyPropertyChanged
    {
        private UInt32 _eventNum;
        private DateTime _timeStamp;
        private int _msgSize;
        private string _msgData;
        private string _info;
        private TPMessage _tpMsg;
        readonly IDictionary<string, object> data;

        public TPDataItem()
        {
            data = new Dictionary<string, object>();
        }

        public TPDataItem(TPMessage Msg)
        {
            if (Msg != null)
            {
                this.data = new Dictionary<string, object>();

                this._tpMsg = Msg;
                this._eventNum = Msg.EventNum;
                this._msgSize = Msg.Length;
                this._timeStamp = Msg.Timestamp;
                this._msgData = BitConverter.ToString(Msg.Data).Replace("-", string.Empty);

                data.Add("Event #", _eventNum.ToString());
                data.Add("Timestamp", _timeStamp.ToString());

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Size: {0}", _msgSize.ToString()));

                data.Add("Info", sb);
                data.Add("Message", _msgData);
            }
        }       

        public TPMessage Message
        {
            get
            {
                return this._tpMsg;
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
