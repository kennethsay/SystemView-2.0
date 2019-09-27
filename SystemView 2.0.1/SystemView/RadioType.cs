using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemView
{
    class RadioType
    {
        /// <summary>
        /// Finds the Radio Package Type based on the label and code of the radio message.
        /// </summary>
        /// <param name="label">Label for Radio Message</param>
        /// <param name="code">Manufacturer's Code for Radio Message</param>
        /// <returns>MessageType</returns>
        public static string FindRadioMessage(UInt16 label, UInt16 code)
        {
            try
            {
                string radioType;

                if (code == 21063)  //5247
                {
                    switch (label)
                    {
                        case 47170: //B842
                            radioType = "MessageType11";
                            break;
                        case 47235: //B883
                            radioType = "MessageType12";
                            break;
                        case 47236: //B884
                            radioType = "MessageType19";
                            break;
                        case 47169: //B841
                            radioType = "MessageType21";
                            break;
                        case 47233: //B881
                            radioType = "MessageType22";
                            break;
                        case 47234: //B882
                            radioType = "MessageType23";
                            break;
                        case 47367: //B907
                            radioType = "MessageType24";
                            break;
                        case 47362: //B902
                            radioType = "MessageType34";
                            break;
                        case 47363: //B903
                            radioType = "MessageType35";
                            break;
                        default:
                            radioType = "Error";
                            break;
                    }
                }
                else
                {
                    radioType = "Error";
                }

                return radioType;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("SystemView.Radio_Message::FindRadioMessage-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }
    }
}
