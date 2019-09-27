using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemView
{
    public class PackageType
    {
        /// <summary>
        /// Determines the Package Number based on the given Type and Size.
        /// </summary>
        /// <param name="type">Type of Package</param>
        /// <param name="size">Size of Package</param>
        /// <returns>Package Number</returns>
        public static string FindPackage(int type, int size)
        {
            try
            {
                string package;

                switch (type)
                {
                    case 0:
                        if (size == 1)
                        {
                            package = "Package26";
                        }
                        else if (size == 0)
                        {
                            package = "No Package Found";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 2:
                        if (size == 2)
                        {
                            package = "Package27";
                        }
                        else if (size == 4)
                        {
                            package = "Package28";
                        }
                        else if (size == 5)
                        {
                            package = "Package29";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 3:
                        if (size == 0)
                        {
                            package = "Package07";
                        }
                        else if (size == 1)
                        {
                            package = "Package09";
                        }
                        else if (size == 2)
                        {
                            package = "Package10";
                        }
                        else if (size == 3)
                        {
                            package = "Package11";
                        }
                        else if (size == 4)
                        {
                            package = "Package08";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 4:
                        if (size == 2)
                        {
                            package = "Package12";
                        }
                        else if (size == 3)
                        {
                            package = "Package13";
                        }
                        else if (size == 5)
                        {
                            package = "Package14";
                        }
                        else if (size == 6)
                        {
                            package = "Package37";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 5:
                        if (size == 2)
                        {
                            package = "Package15";
                        }
                        else if (size == 3)
                        {
                            package = "Package16";
                        }
                        else if (size == 4)
                        {
                            package = "Package17";
                        }
                        else if (size == 5)
                        {
                            package = "Package18";
                        }
                        else if (size == 6)
                        {
                            package = "Package19";
                        }
                        else if (size == 7)
                        {
                            package = "Package20";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 6:
                        if (size == 1)
                        {
                            package = "Package36";
                        }
                        else if (size == 5)
                        {
                            package = "Package44";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 7:
                        if (size == 0)
                        {
                            package = "Package33";
                        }
                        else if (size == 1)
                        {
                            package = "Package32";
                        }
                        else if (size == 2)
                        {
                            package = "Package42";
                        }
                        else if (size == 3)
                        {
                            package = "Package35";
                        }
                        else if (size == 4)
                        {
                            package = "Package05";
                        }
                        else if (size == 5)
                        {
                            package = "Package04";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 12:
                        if (size == 0)
                        {
                            package = "Package21";
                        }
                        else if (size == 1)
                        {
                            package = "Package22";
                        }
                        else if (size == 2)
                        {
                            package = "Package23";
                        }
                        else if (size == 3)
                        {
                            package = "Package34";  //Could also be package 24 for other train companies
                        }
                        else if (size == 4)
                        {
                            package = "Package25";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    case 14:
                        if (size == 5)
                        {
                            package = "Package38";
                        }
                        else if (size == 7)
                        {
                            package = "Package39";
                        }
                        else if (size == 8)
                        {
                            package = "Package40";
                        }
                        else
                        {
                            package = "Error";
                        }
                        break;
                    default:
                        package = "Error";
                        break;
                }

                return package;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("PackageType-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return "Exception";
            }
        }
    }
}