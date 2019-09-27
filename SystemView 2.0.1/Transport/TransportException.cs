using System;
using System.Collections.Generic;
using System.Text;

namespace Transport
{
    using System;
    public class TransportException : ApplicationException
    {
        public TransportException()
        {
        }

        public TransportException(string message) : base(message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("TransportException - threw exception {0}", message.ToString()));

            Console.WriteLine(sb.ToString());
        }

        public TransportException(string message, Exception inner) : base(message, inner)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Transport Exception - threw exception {0} {1}", inner.ToString(), message.ToString()));

            Console.WriteLine(sb.ToString());
        }
    }
}
