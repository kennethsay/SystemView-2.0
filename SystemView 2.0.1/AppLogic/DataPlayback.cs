using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using static System.Math;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace AppLogic
{
    public class DataPlayback
    {
        #region Member Variables
        private string _datFileName;

        private TagList _thisTagList;
        private TagList referenceList;

        public OpenFileDialog OpenDATFile;

        private TagList firstTagInList;
        public ConcurrentQueue<TagList> TagListQueue = new ConcurrentQueue<TagList>();
        public List<StringBuilder> datHeader;
        public List<string> sdfHeader;

        const int NUM_EXTRA_PARAMS = 76;
        const int RADIO_LENGTH = 39;
        private static int _railroadID;
        private static int passCounter;
        private static int _numEvents;

        int dataLengthIndex;
        int msgIndex;
        int DATrecordIndex;
        int dataEnd;
        byte[] data;
        byte[] datFile;
        string[] sdfFile;
        byte[] header;
        #endregion

        public string DATFileName
        {
            get { return _datFileName; }
            set
            {
                _datFileName = value;
            }
        }

        public int NumEvents
        {
            get { return _numEvents;  }
            set
            {
                _numEvents = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DataPlayback()
        {
            referenceList = new TagList();
            dataLengthIndex = 0;
            
            sdfFile = null;
            datFile = null;
        }

        /// <summary>
        /// Creates dialog for user to choose input file. 
        /// </summary>
        public bool selectFile()
        {
            try
            {
                OpenDATFile = new OpenFileDialog();
                OpenDATFile.Filter = "data files | *.dat; *.sdf";
                OpenDATFile.ShowDialog();
                DATFileName = OpenDATFile.FileName;

                string extension = Path.GetExtension(DATFileName);

                if (extension == ".dat" || extension == ".DAT")
                {
                    datFile = File.ReadAllBytes(DATFileName);

                    return true;
                }
                else if (extension == ".sdf" || extension == ".SDF")
                {
                    sdfFile = File.ReadAllLines(DATFileName);

                    return true;
                }
                else
                {
                    Console.WriteLine("File Extension is not supported!");
                    return false;
                }
            }
            catch (IOException ex)
            {
                // error message if another program is using the selected file (e.g. ACSESview)
                MessageBox.Show("The process cannot access the file because it is being used by another process.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        public void FileToReadAndPresent()
        {
            if (sdfFile != null && datFile == null)
            {
                readSDFFile();
            }
            else if (sdfFile == null && datFile != null)
            {
                //readDATFile();
                openDATFile();
            }
            else
            {
                Console.WriteLine("Something went wrong.");
            }

        }

        private void readSDFFile()
        {
            sdfHeader = new List<string>();

            foreach (string line in sdfFile)
            {
                if (line == "Data:")
                {
                    break;
                }
                else
                {
                    sdfHeader.Add(line);
                }
            }

            BackgroundWorker _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += readSDFtoTags;
            _worker.RunWorkerCompleted += endSDFfile;
            _worker.RunWorkerAsync();
        }

        private void readSDFtoTags(object sender, DoWorkEventArgs e)
        {
            int i = (Array.FindIndex(sdfFile, row => row.Contains("Index")) + 1);
            while(i < sdfFile.Length)
            {
                _thisTagList = new TagList();

                _thisTagList = processRecord(sdfFile[i].Replace(" " , ""));

                TagListQueue.Enqueue(_thisTagList);
                i++;
            }
        }

        private void endSDFfile(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private TagList processRecord(string st)
        {
            try
            {
                var records = st.Split(',');

                TagList processed = new TagList();

                for (int k = 0; k < 93; k++)
                {
                    byte[] dataB = Enumerable.Range(0, records[k + 1].Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(records[k + 1].Substring(x, 2), 16)).ToArray();
                    processed.GetTag((byte)k).AbsoluteDataWrite(dataB);
                }

                return processed;
            }         

            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::33333333333-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
                // error message if another program is usin
            }
        }

        private void openDATFile()
        {
            _railroadID = (int)(datFile[27]);
            int dataSize = (datFile.Length - 736);
            NumEvents = getNumEvents();
            data = new byte[dataSize];
            Buffer.BlockCopy(datFile, 736, data, 0, dataSize);
            DATrecordIndex = 0;
            passCounter = 0;
            dataEnd = getDataEnd(data);
        }

        private int getNumEvents()
        {
            int numEvents = 0;

            numEvents += datFile[703] << 24;
            numEvents += datFile[702] << 16;
            numEvents += datFile[701] << 8;
            numEvents += datFile[700];

            return numEvents;
        }

        public void queueOneRecord(int eventNum)
        {
            if (DATrecordIndex < eventNum)
            {
                while (DATrecordIndex < eventNum)
                {
                    while (data[dataLengthIndex] != 0xFF)
                    {
                        dataLengthIndex++;
                    }

                    dataLengthIndex++;
                    DATrecordIndex++;
                }
            }

            else if (DATrecordIndex > eventNum)
            {
                while (DATrecordIndex > eventNum)
                {
                    while (data[dataLengthIndex] != 0xFF)
                    {
                        dataLengthIndex--;
                    }

                    DATrecordIndex--;
                }
            }

            //dataLengthIndex++;
            //Console.WriteLine(dataLengthIndex);

            TagList Tags = new TagList();
            _thisTagList = new TagList();

            int tagIDindex;
            int lengthOfTag;
            int recordsBacktracked = 0;
            msgIndex = dataLengthIndex;

            do
            {
                Tags = new TagList();

                while (data[msgIndex] != 0xFF)
                {
                    try
                    {
                        tagIDindex = data[msgIndex];
                        msgIndex++;

                        lengthOfTag = Tags.Tags.Find(X => X.TagID == tagIDindex).Length;

                        byte[] dataToAdd = new byte[lengthOfTag];

                        for (int i = 0; i < lengthOfTag; i++)
                        {
                            if (data[msgIndex] == 0xFF)
                            {
                                break;
                            }
                            else if (data[msgIndex] == 0xF0)
                            {
                                byte highNibble = (byte)(data[msgIndex] & 0xF0);
                                msgIndex++;
                                byte lowNibble = (byte)(data[msgIndex] & 0x0F);
                                msgIndex++;
                                // Conjoin the nibbles to form byte of data
                                dataToAdd[i] = (byte)(highNibble | lowNibble);
                            }
                            else
                            {
                                dataToAdd[i] = data[msgIndex];
                                msgIndex++;
                            }
                        }

                        Tags.Tags.Find(X => X.TagID == tagIDindex).Data(dataToAdd);
                    }
                    catch (NullReferenceException ex)
                    {
                        //Console.WriteLine(dataLengthIndex);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(String.Format("DataPlayback::BLAHHHHH-threw exception {0}", ex.ToString()));

                        Console.WriteLine(sb.ToString());
                        // Skip over bad datHeader
                        while (data[msgIndex] != 0xFF)
                        {
                            msgIndex++;
                        }
                    }
                } // End While for each Tag message read until 0xFF!!

                if (!fullRecord(Tags) & msgIndex != 0)
                {
                    recordsBacktracked++;
                    msgIndex--;

                    while (data[msgIndex] != 0xFF & msgIndex != 0)
                    {
                        msgIndex--;
                    }

                    if (msgIndex != 0)
                    {
                        msgIndex--;
                    }
                  

                    while (data[msgIndex] != 0xFF & msgIndex != 0)
                    {
                        msgIndex--;
                    }

                    if (msgIndex != 0)
                    {
                        msgIndex++;
                    }
                }
            } while (!fullRecord(Tags) & msgIndex != 0);

            TagList nextRecord = new TagList();
            msgIndex++;

            for (int i = 0; i < recordsBacktracked; i++)
            {
                while (data[msgIndex] != 0xFF)
                {
                    try
                    {
                        tagIDindex = data[msgIndex];
                        msgIndex++;

                        lengthOfTag = nextRecord.Tags.Find(X => X.TagID == tagIDindex).Length;

                        byte[] dataToAdd = new byte[lengthOfTag];

                        for (int j = 0; j < lengthOfTag; j++)
                        {
                            if (data[msgIndex] == 0xFF)
                            {
                                break;
                            }
                            else if (data[msgIndex] == 0xF0)
                            {
                                byte highNibble = (byte)(data[msgIndex] & 0xF0);
                                msgIndex++;
                                byte lowNibble = (byte)(data[msgIndex] & 0x0F);
                                msgIndex++;
                                // Conjoin the nibbles to form byte of data
                                dataToAdd[j] = (byte)(highNibble | lowNibble);
                            }
                            else
                            {
                                dataToAdd[j] = data[msgIndex];
                                msgIndex++;
                            }
                        }

                        nextRecord.Tags.Find(X => X.TagID == tagIDindex).Data(dataToAdd);
                    }
                    catch (NullReferenceException ex)
                    {
                        //Console.WriteLine(dataLengthIndex);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(String.Format("DataPlayback::BLAHHHHH-threw exception {0}", ex.ToString()));

                        Console.WriteLine(sb.ToString());
                        // Skip over bad datHeader
                        while (data[msgIndex] != 0xFF)
                        {
                            msgIndex++;
                        }
                    }
                } // End While for each Tag message read until 0xFF!!

                for (int k = 0; k < 92; k++)
                {
                    if (nextRecord.Tags.Find(X => X.TagID == k).HasData)
                    {
                        // UPDATE DATA IF DATA EXISTS
                        Tags.Tags.Find(X => X.TagID == k).AbsoluteDataWrite(nextRecord.Tags.Find(X => X.TagID == k).Data());
                    }
                }

                nextRecord = new TagList();
            }

            Console.WriteLine("Should Enqueue");   
            TagListQueue.Enqueue(Tags);
        }

        private bool fullRecord(TagList record)
        {
            for (int i = 0; i < 90; i++)
            {
                if (!(i == 13 | i == 14 | i == 15 | i == 32 | i == 55 | i == 63 | i == 64 | i == 70))
                {
                    if (!record.Tags.Find(X => X.TagID == i).HasData)
                    {
                        Console.WriteLine(i);

                        return false;
                    }
                } 
            }

            return true;
        }


        /// <summary>
        /// Called by runProgram if all error checking is passed. Creates byte arrays for header and data, and calls newReader to begin processing data
        /// </summary>
        private void readDATFile()
        {
            try
            {
                _railroadID = (int)(datFile[27]);
                int dataSize = (datFile.Length - 736);
                data = new byte[dataSize];
                Buffer.BlockCopy(datFile, 736, data, 0, dataSize);

                passCounter = 0;
                dataEnd = getDataEnd(data);

                BackgroundWorker _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;
                _worker.DoWork += readDATtoTags;
                _worker.RunWorkerCompleted += getDATfileHeader;
                _worker.RunWorkerAsync();
            }

            catch (IOException ex)
            {
                // error message if another program is using the selected file (e.g. ACSESview)
                MessageBox.Show("The process cannot access the file because it is being used by another process.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Method that converts the bytes read from DAT file to TagLists and adds them to the 
        /// Concurrent Queue for the UI to process and display to user.
        /// </summary>
        private void readDATtoTags(object sender, DoWorkEventArgs e)
        {
            firstTagInList = new TagList();
            TagList fullTags = new TagList();
            bool firstTag = true;
            bool secondTag = false;

            // Read Records and add to TagListQueue
            while (dataLengthIndex < dataEnd)
            {
                TagList Tags = new TagList();
                _thisTagList = new TagList();

                int tagIDindex;
                int lengthOfTag;
                int msgIndex = dataLengthIndex;

                while (data[msgIndex] != 0xFF)
                {
                    try
                    {
                        tagIDindex = data[msgIndex];
                        msgIndex++;

                        lengthOfTag = Tags.Tags.Find(X => X.TagID == tagIDindex).Length;

                        byte[] dataToAdd = new byte[lengthOfTag];

                        for (int i = 0; i < lengthOfTag; i++)
                        {
                            if (data[msgIndex] == 0xFF)
                            {
                                break;
                            }
                            else if (data[msgIndex] == 0xF0)
                            {
                                byte highNibble = (byte)(data[msgIndex] & 0xF0);
                                msgIndex++;
                                byte lowNibble = (byte)(data[msgIndex] & 0x0F);
                                msgIndex++;
                                // Conjoin the nibbles to form byte of data
                                dataToAdd[i] = (byte)(highNibble | lowNibble);
                            }
                            else
                            {
                                dataToAdd[i] = data[msgIndex];
                                msgIndex++;
                            }
                        }
                        Tags.Tags.Find(X => X.TagID == tagIDindex).Data(dataToAdd);
                    }
                    catch (NullReferenceException ex)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(String.Format("DataPlayback::BLAHHHHH-threw exception {0}", ex.ToString()));

                        Console.WriteLine(sb.ToString());
                        // Skip over bad datHeader
                        while (data[msgIndex] != 0xFF)
                        {
                            msgIndex++;
                        }
                    }
                } // End While for each Tag message read until 0xFF!!

                dataLengthIndex = msgIndex + 1;
                try
                {
                    if (firstTag)
                    {
                        _thisTagList = fullTagList(Tags);
                        firstTagInList = fullTagList(Tags);
                        firstTag = false;
                        secondTag = true;
                    }
                    else if (secondTag)
                    {
                        firstTagInList.Tags.Find(X => X.TagID == 90).Data(Tags.Tags.Find(X => X.TagID == 90).Data());
                        firstTagInList.Tags.Find(X => X.TagID == 91).Data(Tags.Tags.Find(X => X.TagID == 91).Data());
                        secondTag = false;
                        TagListQueue.Enqueue(firstTagInList);

                        fullTags = fullTagList(firstTagInList);
                    }
                    else
                    {
                        for (int i = 0; i < 92; i++)
                        {
                            if (Tags.Tags.Find(X => X.TagID == i).HasData)
                            {
                                // UPDATE DATA IF DATA EXISTS
                                fullTags.Tags.Find(X => X.TagID == i).AbsoluteDataWrite(Tags.Tags.Find(X => X.TagID == i).Data());
                            }
                        }

                        _thisTagList = fullTagList(fullTags);
                        TagListQueue.Enqueue(_thisTagList);
                    }
                    Thread.Sleep(20);
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(String.Format("DataPlayback::33333333333-threw exception {0}", ex.ToString()));

                    Console.WriteLine(sb.ToString());
                }
            } //End while for all of the bytes in DAT file
        }

        /// <summary>
        /// Determines and returns the index at which the valid data bytes end, indicated by two consecutive 0xFF bytes.
        /// </summary>
        private static int getDataEnd(byte[] data)
        {
            int i = 0;
            int j = 1;

            // Array of data bytes is terminated by two consecutive 0xFF bytes
            while (data[i] != 0xFF || data[j] != 0xFF)
            {
                i++;
                j++;
            }

            return j;
        }

        /// <summary>
        /// Saves a TagList that has all of the tags filled with data.
        /// </summary>
        /// <param name="thisTagList">Received full TagList from OBC</param>
        /// <returns>Full Tag List</returns>
        private TagList fullTagList(TagList thisTagList)
        {
            try
            {
                TagList tagListFull;
                tagListFull = thisTagList;
                return tagListFull;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::fullTagList-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Retrieves all of the header info from the DAT file for SystemView's use.
        /// </summary>
        private void getDATfileHeader(object sender, RunWorkerCompletedEventArgs e)
        {
            datHeader = new List<StringBuilder>();

            header = new byte[736];
            Buffer.BlockCopy(datFile, 0, header, 0, 736);

            // Create Stringbuilder of blank lines
            StringBuilder blankBuilder = new StringBuilder();
            blankBuilder.Append(" ");
            datHeader.Add(blankBuilder);

            // Input file name
            StringBuilder pathBuilder = new StringBuilder();
            pathBuilder.Append("DATALOG FILE:   " + DATFileName);
            datHeader.Add(pathBuilder);
            datHeader.Add(blankBuilder);

            // Date downloaded and ACSESView version
            StringBuilder dldateSB = new StringBuilder();
            byte[] dldate = new byte[4];
            Buffer.BlockCopy(header, 4, dldate, 0, 4);
            Array.Reverse(dldate);
            uint sec1900 = (uint)((dldate[0] << 24) + (dldate[1] << 16) + (dldate[2] << 8) + dldate[3]); // the value of date downloaded is the number of seconds since 1/1/1900 represented as 4 bytes
            byte[] avVersion = new byte[10];
            Buffer.BlockCopy(header, 725, avVersion, 0, 10);
            dldateSB.Append("DOWNLOADED:  " + dateDownloaded(sec1900).ToString() + "   using ACSESView  " + Encoding.UTF8.GetString(avVersion));
            datHeader.Add(dldateSB);
            datHeader.Add(blankBuilder);

            // Date output file created (report date)
            StringBuilder rdateBuilder = new StringBuilder();
            DateTime now = DateTime.Now;
            rdateBuilder.Append("REPORT DATE:  " + now.ToString());
            datHeader.Add(rdateBuilder);
            datHeader.Add(blankBuilder);

            // Locomotive number/railroad name/ATCS 
            StringBuilder locoBuilder = new StringBuilder();
            int loconum = bytesToInt(firstTagInList.Tags.Find(X => X.TagID == 69).Data(), 2);
            byte[] rrBytes = new byte[20];
            Buffer.BlockCopy(header, 68, rrBytes, 0, 20);
            byte[] atcsBytes = new byte[2];
            Buffer.BlockCopy(header, 707, atcsBytes, 0, 2);
            int atcs = bytesToInt(atcsBytes, 2);
            locoBuilder.Append("Locomotive # " + loconum.ToString() + "   Railroad:  " + Encoding.UTF8.GetString(rrBytes) + "    ATCS#:  " + atcs.ToString());
            datHeader.Add(locoBuilder);

            // Vehicle type
            StringBuilder veTypeSB = new StringBuilder();
            veTypeSB.Append("Vehicle Type:  ");

            // If railroad = METRO NORTH
            if (rrBytes[0] == 0x4D & rrBytes[1] == 0x45)
            {
                switch ((int)header[16])
                {
                    case 0:
                        {
                            veTypeSB.Append("M7A");
                            break;
                        }
                    case 1:
                        {
                            veTypeSB.Append("M3A");
                            break;
                        }
                    case 2:
                        {
                            veTypeSB.Append("CONTRACT 12");
                            break;
                        }
                    case 3:
                        {
                            veTypeSB.Append("CONTRACT 19");
                            break;
                        }
                    case 4:
                        {
                            veTypeSB.Append("CONTRACT 21");
                            break;
                        }
                    case 5:
                        {
                            veTypeSB.Append("CONTRACT 34");
                            break;
                        }
                    case 6:
                        {
                            veTypeSB.Append("CONTRACT 38/38A/82");
                            break;
                        }
                    case 7:
                        {
                            veTypeSB.Append("P32AC-DM");
                            break;
                        }
                    case 8:
                        {
                            veTypeSB.Append("BL-14CG");
                            break;
                        }
                    case 9:
                        {
                            veTypeSB.Append("BL-20GH");
                            break;
                        }
                    case 10:
                        {
                            veTypeSB.Append("GP-35");
                            break;
                        }
                    case 11:
                        {
                            veTypeSB.Append("M9");
                            break;
                        }
                    default:
                        {
                            veTypeSB.Append("error");
                            break;
                        }
                }
            }
            else // if LIRR
            {
                switch ((int)header[16])
                {
                    case 0:
                        {
                            veTypeSB.Append("M7");
                            break;
                        }
                    case 1:
                        {
                            veTypeSB.Append("TC-82");
                            break;
                        }
                    case 2:
                        {
                            veTypeSB.Append("C3");
                            break;
                        }
                    case 3:
                        {
                            veTypeSB.Append("DE30AC");
                            break;
                        }
                    case 4:
                        {
                            veTypeSB.Append("DM30AC");
                            break;
                        }
                    case 5:
                        {
                            veTypeSB.Append("LIRR MP15");
                            break;
                        }
                    case 6:
                        {
                            veTypeSB.Append("NYAR MP15");
                            break;
                        }
                    case 7:
                        {
                            veTypeSB.Append("NYAR GP 38-2");
                            break;
                        }
                    case 8:
                        {
                            veTypeSB.Append("M9");
                            break;
                        }
                    case 9:
                        {
                            veTypeSB.Append("NYAR PR20B");
                            break;
                        }
                    case 10:
                        {
                            veTypeSB.Append("M3");
                            break;
                        }
                    case 11:
                        {
                            veTypeSB.Append("   ");
                            break;
                        }
                    default:
                        {
                            veTypeSB.Append("error");
                            break;
                        }
                }
            }

            datHeader.Add(veTypeSB);

            // Train type
            StringBuilder trType = new StringBuilder();
            byte[] trBytes = new byte[12];
            Buffer.BlockCopy(header, 48, trBytes, 0, 12);
            trType.Append("Train Type:  " + Encoding.UTF8.GetString(trBytes));
            datHeader.Add(trType);

            // Vehicle max speed
            StringBuilder vSpeedSB = new StringBuilder();
            int vSpeed = (int)header[17];
            vSpeedSB.Append("Maximum Speed: " + vSpeed.ToString() + " MPH");
            datHeader.Add(vSpeedSB);

            // Wheel diameter
            StringBuilder wheelDiamSB = new StringBuilder();
            decimal wheelDiam = (decimal)bytesToInt(firstTagInList.Tags.Find(X => X.TagID == 72).Data(), 2);
            wheelDiam = Math.Round((wheelDiam / 100), 2);
            wheelDiamSB.Append("Wheel Diameter: " + wheelDiam.ToString("#.##") + " in.");
            datHeader.Add(wheelDiamSB);

            // Front antenna offset
            StringBuilder fcaSB = new StringBuilder();
            int fca = bytesToInt(firstTagInList.Tags.Find(X => X.TagID == 59).Data(), 1);
            fcaSB.Append("Front Cab Antenna Offset: " + fca.ToString() + " ft.");
            datHeader.Add(fcaSB);

            // Rear antenna offset
            StringBuilder rcaSB = new StringBuilder();
            int rca = bytesToInt(firstTagInList.Tags.Find(X => X.TagID == 60).Data(), 1);
            rcaSB.Append("Rear Cab Antenna Offset: " + rca.ToString() + " ft.");
            datHeader.Add(rcaSB);

            // Yards per pulse cf
            StringBuilder yppcfSB = new StringBuilder();
            float yppcf = (float)bytesToInt(firstTagInList.Tags.Find(X => X.TagID == 66).Data(), 2);
            yppcf = (yppcf / 100000);
            yppcfSB.Append("YardsperPulseCF: " + yppcf.ToString());
            datHeader.Add(yppcfSB);

            // Decel direction
            StringBuilder decelDirectSB = new StringBuilder();
            decelDirectSB.Append("Decelerometer Direction:  ");
            byte decelDirect = firstTagInList.Tags.Find(X => X.TagID == 58).Data()[0];
            decelDirect = (byte)(decelDirect & 0x08);
            if (decelDirect != 0x08)
            {
                decelDirectSB.Append("Forward");
            }
            else
            {
                decelDirectSB.Append("Reverse");
            }
            datHeader.Add(decelDirectSB);

            // Tunnel enabled
            StringBuilder tunnelSB = new StringBuilder();
            tunnelSB.Append("Tunnel Enabled?  ");
            byte tunnel = header[709];
            if (tunnel == 0x00)
            {
                tunnelSB.Append("NO");
            }
            else
            {
                tunnelSB.Append("YES");
            }
            datHeader.Add(tunnelSB);

            datHeader.Add(blankBuilder);

            /* PART/REV NUMBERS*/

            // MAIN
            StringBuilder mainSB = new StringBuilder();
            mainSB.Append("MAIN:  ");
            byte[] main = new byte[37];
            Buffer.BlockCopy(header, 88, main, 0, 37);
            mainSB.Append(Encoding.UTF8.GetString(main));
            datHeader.Add(mainSB);

            // AUX IO
            StringBuilder auxIOSB = new StringBuilder();
            auxIOSB.Append("AUX I/O:  ");
            byte[] auxIO = new byte[37];
            Buffer.BlockCopy(header, 126, auxIO, 0, 37);
            auxIOSB.Append(Encoding.UTF8.GetString(auxIO));
            datHeader.Add(auxIOSB);

            // TACH IO
            StringBuilder tachIOSB = new StringBuilder();
            tachIOSB.Append("TACH I/O:  ");
            byte[] tachIO = new byte[37];
            Buffer.BlockCopy(header, 164, tachIO, 0, 37);
            tachIOSB.Append(Encoding.UTF8.GetString(tachIO));
            datHeader.Add(tachIOSB);

            // AIU
            StringBuilder aiuSB = new StringBuilder();
            aiuSB.Append("AIU:  ");
            byte[] aiu = new byte[37];
            Buffer.BlockCopy(header, 202, aiu, 0, 37);
            aiuSB.Append(Encoding.UTF8.GetString(aiu));
            datHeader.Add(aiuSB);

            // COMM
            StringBuilder commSB = new StringBuilder();
            commSB.Append("COMM:  ");
            byte[] comm = new byte[37];
            Buffer.BlockCopy(header, 240, comm, 0, 37);
            commSB.Append(Encoding.UTF8.GetString(comm));
            datHeader.Add(commSB);

            // CONFIG 
            StringBuilder configSB = new StringBuilder();
            configSB.Append("Config:  ");
            byte[] config = new byte[37];
            Buffer.BlockCopy(header, 278, config, 0, 37);
            configSB.Append(Encoding.UTF8.GetString(config));
            datHeader.Add(configSB);

            // DECEL
            StringBuilder decelSB = new StringBuilder();
            decelSB.Append("Decel:  ");
            byte[] decel = new byte[37];
            Buffer.BlockCopy(header, 316, decel, 0, 37);
            decelSB.Append(Encoding.UTF8.GetString(decel));
            datHeader.Add(decelSB);

            // TP READER
            StringBuilder tprdrSB = new StringBuilder();
            tprdrSB.Append("TP RDR:  ");
            byte[] tprdr = new byte[37];
            Buffer.BlockCopy(header, 354, tprdr, 0, 37);
            tprdrSB.Append(Encoding.UTF8.GetString(tprdr));
            datHeader.Add(tprdrSB);

            // ADU 1
            StringBuilder adu1SB = new StringBuilder();
            adu1SB.Append("ADU 1:  ");
            byte[] adu1 = new byte[37];
            Buffer.BlockCopy(header, 392, adu1, 0, 37);
            adu1SB.Append(Encoding.UTF8.GetString(adu1));
            datHeader.Add(adu1SB);

            // ADU 2
            StringBuilder adu2SB = new StringBuilder();
            adu2SB.Append("ADU 2:  ");
            byte[] adu2 = new byte[37];
            Buffer.BlockCopy(header, 430, adu2, 0, 37);
            adu2SB.Append(Encoding.UTF8.GetString(adu2));
            datHeader.Add(adu2SB);

            // ADU 3
            StringBuilder adu3SB = new StringBuilder();
            adu3SB.Append("ADU 3:  ");
            byte[] adu3 = new byte[37];
            Buffer.BlockCopy(header, 468, adu3, 0, 37);
            adu3SB.Append(Encoding.UTF8.GetString(adu3));
            datHeader.Add(adu3SB);

            // ADU 4
            StringBuilder adu4SB = new StringBuilder();
            adu4SB.Append("ADU 4:  ");
            byte[] adu4 = new byte[37];
            Buffer.BlockCopy(header, 506, adu4, 0, 37);
            adu4SB.Append(Encoding.UTF8.GetString(adu4));
            datHeader.Add(adu4SB);

            // ETHER
            StringBuilder etherSB = new StringBuilder();
            etherSB.Append("Ethernet:  ");
            byte[] ether = new byte[37];
            Buffer.BlockCopy(header, 544, ether, 0, 37);
            etherSB.Append(Encoding.UTF8.GetString(ether));
            datHeader.Add(etherSB);

            // NETBURNER
            StringBuilder netbSB = new StringBuilder();
            netbSB.Append("Netburner:  ");
            byte[] netb = new byte[37];
            Buffer.BlockCopy(header, 582, netb, 0, 37);
            netbSB.Append(Encoding.UTF8.GetString(netb));
            datHeader.Add(netbSB);

            // ARMM
            StringBuilder armmSB = new StringBuilder();
            armmSB.Append("ARMM:  ");
            byte[] armm = new byte[37];
            Buffer.BlockCopy(header, 620, armm, 0, 37);
            armmSB.Append(Encoding.UTF8.GetString(armm));
            datHeader.Add(armmSB);

            // CHMM
            StringBuilder chmmSB = new StringBuilder();
            chmmSB.Append("CHMM:  ");
            byte[] chmm = new byte[37];
            Buffer.BlockCopy(header, 658, chmm, 0, 37);
            chmmSB.Append(Encoding.UTF8.GetString(chmm));
            datHeader.Add(chmmSB);

            datHeader.Add(blankBuilder);
        }

        /// <summary>
        /// Converts a byte array to an Integer.
        /// </summary>
        /// <param name="byteInt">Byte Array</param>
        /// <param name="Length">Length of Byte Array</param>
        /// <returns>Integer</returns>
        private static int bytesToInt(byte[] byteInt, int Length)
        {
            try
            {
                ushort value;

                if (Length == 1)
                {
                    value = byteInt[0];
                }
                else if (Length == 2)
                {
                    value = (ushort)((byteInt[0] << 8) + byteInt[1]);
                }
                else
                {
                    value = 0;
                }
                return value;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::bytesToInt-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Convert Byte Array of seconds from 1,1,2000 to the actual time it represents in DateTime format.
        /// </summary>
        /// <param name="byteDateTime">Byte Array of seconds since 1,1,2000</param>
        /// <returns>DateTime byte array represents</returns>
        private static DateTime bytesToDateTime(byte[] byteDateTime)
        {
            try
            {
                DateTime returnedDT;
                long t, tt;
                Byte b;
                tt = 0;

                DateTime dtWork = new DateTime(2000, 1, 1);

                for (int i = 0; i < 4; i++)
                {
                    tt = (tt << 8);
                    b = byteDateTime[i];
                    t = Convert.ToInt32(b);
                    // tt is now a 32-bit integer representing the number of seconds since 1/1/2000
                    tt += t;
                }
                // Add the number of seconds to the DateTime object, resulting in a DateTime object that represents the current OBC time
                dtWork = dtWork.AddSeconds(tt);

                // The last byte of Date Time from OBC is milliseconds, Add them
                b = byteDateTime[4];
                t = Convert.ToInt32(b);

                returnedDT = dtWork.AddMilliseconds(t);

                // convert to local time if LIRR
                if (_railroadID == 1)
                {
                    returnedDT = returnedDT.ToLocalTime();
                }

                return returnedDT;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::bytesToDateTime-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Converts an integer representing the number of seconds since 1/1/1900 into a dateTime object
        /// </summary>
        /// <param name="secsSince1900">uint</param>
        /// <returns>DateTime that file was downloaded</returns>
        private static DateTime dateDownloaded(uint secsSince1900)
        {
            DateTime returnedDT = new DateTime(1900, 1, 1);
            returnedDT = returnedDT.AddSeconds(secsSince1900);

            return returnedDT.ToLocalTime();
        }

        #region Extra Code that may need utilized in future
        /*
        /// <summary>
        /// Converts byte array to String to represents the bytes.
        /// </summary>
        /// <param name="byteToString">Byte Array</param>
        /// <returns>String that Represents the Bytes</returns>
        private static string byteArryToString(byte[] byteToString, int Length)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Length; i++)
                {
                    sb.Append(String.Format("{0:X2}", byteToString[i]));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::getTagType-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Creates a String for the output for the Hex Data Type defined for the specific Tag.
        /// </summary>
        /// <param name="byteToHex"></param>
        /// <returns>String that represents the hex </returns>
        private static string byteArrayToStringHex(byte[] byteToHex, int Length)
        {
            try
            {
                if (Length == 1)
                {
                    return String.Format("0x{0:X2}", byteToHex[0]);
                }
                else if (Length == 2)
                {
                    return String.Format("0x{0:X2}{1:X2}", byteToHex[0], byteToHex[1]);
                }
                else
                {
                    return "Not Supported";
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("DataPlayback::byteArrayToString-threw exception {0}", ex.ToString()));

                Console.WriteLine(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// Given a Tag ID, determines whether that tag has extra data prameters associated with it.
        /// </summary>
        /// <param name="id">int</param>
        /// <returns>boolean</returns>
        private bool extraParams(int id)
        {
            if (id == 6 | id == 8 | id == 9 | id == 10 | id == 11 | id == 16 | id == 17 | id == 18 | id == 30 | id == 34 | id == 38 | id == 47 | id == 58 | id == 65 | id == 76 | id == 77 | id == 78 | id == 84 | id == 91)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Given a Tag ID, determines how many extra data parameters that tag has associated with it.
        /// </summary>
        /// <param name="id">int</param>
        /// <returns>int</returns>
        private int getNumExtras(int id)
        {
            switch (id)
            {
                case 6:
                    return 3;
                case 8:
                    return 2;
                case 9:
                    return 3;
                case 10:
                    return 1;
                case 11:
                    return 1;
                case 16:
                    return 3;
                case 17:
                    return 2;
                case 18:
                    return 4;
                case 30:
                    return 8;

                case 34:
                    return 3;

                case 38:
                    return 5;

                case 47:
                    return 3;

                case 58:
                    return 1;

                case 65:
                    return 2;

                case 76:
                    return 9;

                case 77:
                    return 1;

                case 78:
                    return 11;

                case 84:
                    return 5;

                case 91:
                    return 9;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Given a Tag ID and parameter number, determines the name of one particular extra data parameter.
        /// </summary>
        /// <param name="id">int</param>
        /// <param name="num">int</param>
        /// <returns>string</returns>
        private string getNameExtras(int id, int num)
        {
            switch (id)
            {
                case 6:
                    switch (num)
                    {
                        case 0:
                            return "ACKNOWLEDGE";

                        case 1:
                            return "ACTIVECAB";

                        case 2:
                            return "STOPBYPASS";

                        default:
                            return "error";
                    }

                case 8:
                    switch (num)
                    {
                        case 0:
                            return "ATC CIO";

                        case 1:
                            return "ATC CO";

                        default:
                            return "error";
                    }

                case 9:
                    switch (num)
                    {
                        case 0:
                            return "ACSES CIO";

                        case 1:
                            return "ACSES CO OUT";

                        case 2:
                            return "PERMSUPPR";

                        default:
                            return "error";
                    }

                case 10:
                    switch (num)
                    {
                        case 0:
                            return "SPEEDSENSOR";

                        default:
                            return "error";
                    }

                case 11:
                    switch (num)
                    {
                        case 0:
                            return "PTSALARMSUP";

                        default:
                            return "error";
                    }

                case 16:
                    switch (num)
                    {
                        case 0:
                            return "VFBREAK";

                        case 1:
                            return "PHABREAK";

                        case 2:
                            return "TILTAUTH";

                        default:
                            return "error";
                    }

                case 17:
                    switch (num)
                    {
                        case 0:
                            return "SERVICE PEN";

                        case 1:
                            return "EM PENALTY";

                        default:
                            return "error";
                    }


                case 18:
                    switch (num)
                    {
                        case 0:
                            return "DT STEP";

                        case 1:
                            return "DT SUSPENDED";

                        case 2:
                            return "DT SUCCESS";

                        case 3:
                            return "DT RUNNING";

                        default:
                            return "error";
                    }

                case 30:
                    switch (num)
                    {
                        case 0:
                            return "TPFOUND";

                        case 1:
                            return "TPINWIN";

                        case 2:
                            return "TRAILINGOPR";

                        case 3:
                            return "ALARM";

                        case 4:
                            return "OVERSPEED";

                        case 5:
                            return "ACSES PEN";

                        case 6:
                            return "ATENNAPOWER";

                        case 7:
                            return "DASHDASH";

                        default:
                            return "error";
                    }

                case 34:
                    switch (num)
                    {
                        case 0:
                            return "RADIOCHAN";

                        case 1:
                            return "BCPCOVERAGE";

                        case 2:
                            return "RADIO STATUS";

                        default:
                            return "error";
                    }

                case 38:
                    switch (num)
                    {
                        case 0:
                            return "ACSES MVCO";

                        case 1:
                            return "TPOUTOFWIN";

                        case 2:
                            return "DATABASE OK";

                        case 3:
                            return "TPMISSING";

                        case 4:
                            return "TSRLISTOK";

                        default:
                            return "error";
                    }

                case 47:
                    switch (num)
                    {
                        case 0:
                            return "PTSZONE";

                        case 1:
                            return "RADIORELEASE";

                        case 2:
                            return "C SIGNAL";

                        default:
                            return "error";
                    }

                case 58:
                    switch (num)
                    {
                        case 0:
                            return "DECEL DIRECT";

                        default: return "error";
                    }

                case 65:
                    switch (num)
                    {
                        case 0:
                            return "ROLLAWAY PEN";

                        case 1:
                            return "GEARTHEETH";

                        default:
                            return "error";
                    }

                case 76:
                    switch (num)
                    {
                        case 0:
                            return "FAULT AUXIO";

                        case 1:
                            return "FAULT TACHIO";

                        case 2:
                            return "FAULT CONFIG";

                        case 3:
                            return "FAULT ENET";

                        case 4:
                            return "FAULT ADU 1";

                        case 5:
                            return "FAULT ADU 2";

                        case 6:
                            return "FAULT DECEL";

                        case 7:
                            return "FAULT ATC";

                        case 8:
                            return "FAULT AIU";

                        default:
                            return "error";
                    }

                case 77:
                    switch (num)
                    {
                        case 0:
                            return "FAULT TPR";

                        default:
                            return "error";
                    }

                case 78:
                    switch (num)
                    {
                        case 0:
                            return "BRAKE ASR";

                        case 1:
                            return "ABS CMB TER";

                        case 2:
                            return "ZS TSR IP";

                        case 3:
                            return "TUNNEL AVOID";

                        case 4:
                            return "TB PTS EN";

                        case 5:
                            return "TB PTS ACT";

                        case 6:
                            return "SAR TSR STS";

                        case 7:
                            return "TSR ENFORCED";

                        case 8:
                            return "DB SLIP IP";

                        case 9:
                            return "DB SLIDE IP";

                        case 10:
                            return "TCP/IP";

                        default:
                            return "error";
                    }

                case 84:
                    switch (num)
                    {
                        case 0:
                            return "ACSESPTSREQ";

                        case 1:
                            return "ACKREQ";

                        case 2:
                            return "ACSES CO FLS";

                        case 3:
                            return "ADUVZ";

                        case 4:
                            return "ACSES DASHDASH";

                        default:
                            return "error";
                    }

                case 91:
                    switch (num)
                    {
                        case 0:
                            return "ATCSEQNUM";

                        case 1:
                            return "BCPNUM";

                        case 2:
                            return "RADIORXERROR";

                        case 3:
                            return "RADIORXINF";

                        case 4:
                            return "RADIOTRAFFIC";

                        case 5:
                            return "X|TSRTYPE";

                        case 6:
                            return "RADIOTXRXENC";

                        case 7:
                            return "RADIOTXRXTSR";

                        case 8:
                            return "RADIOTXRXMTA";

                        default:
                            return "error";
                    }

                default:
                    return "error";
            }
        }

        /// <summary>
        /// Given a Tag ID and parameter number, returns a string representing the value of that specific data parameter.
        /// </summary>
        /// <param name="id">int</param>
        /// <param name="num">int</param>
        /// <returns>string</returns>
        private string fillExtraData(int id, int paramNum, TagList record)
        {
            switch (id)
            {
                case 6:
                    switch (paramNum)
                    {
                        // ACKNOWLEDGE
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x20) == 0x20))
                                {
                                    return "A";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        // ACTIVECAB
                        case 1:
                            {
                                byte[] aux1 = record.Tags.Find(X => X.TagID == id).Data();
                                byte rearCab = (byte)(aux1[0] & 0x08);

                                byte[] tach1 = record.Tags.Find(X => X.TagID == 8).Data();
                                byte frontCab = (byte)(tach1[0] & 0x02);

                                if ((rearCab == 0x08 & frontCab == 0x02) | (rearCab != 0x08 & frontCab != 0x02))
                                {
                                    return "N";
                                }

                                else if (frontCab == 0x02)
                                {
                                    return "F";
                                }

                                else if (rearCab == 0x08)
                                {
                                    return "R";
                                }

                                else
                                {
                                    // shouldn't exist
                                    return "error activecab";
                                }

                            }

                        // STOPBYPASS
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0x80);

                                if (paramByte == 0x80)
                                {
                                    return "B";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 8:
                    switch (paramNum)
                    {
                        // ATC CIO
                        case 0:
                            {
                                byte[] data0 = record.Tags.Find(X => X.TagID == id).Data();
                                byte hw = (byte)(data0[0] & 0x80);

                                byte[] data1 = record.Tags.Find(X => X.TagID == 11).Data();
                                byte phw1 = (byte)(data1[0] & 0x02);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte phw2 = (byte)(data2[0] & 0x02);

                                if (hw == 0x80 | phw1 == 0x02 | phw2 == 0x02)
                                {
                                    return "CI";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // ATC CO
                        case 1:
                            {
                                byte[] data0 = record.Tags.Find(X => X.TagID == id).Data();
                                byte hw = (byte)(data0[0] & 0x80);

                                byte[] data1 = record.Tags.Find(X => X.TagID == 11).Data();
                                byte phw1 = (byte)(data1[0] & 0x02);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte phw2 = (byte)(data2[0] & 0x02);

                                if (hw == 0x80 | phw1 == 0x02 | phw2 == 0x02)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "CO";
                                }

                            }

                        default:
                            return "error";
                    }

                case 9:
                    switch (paramNum)
                    {
                        // ACSES CIO
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {
                                    return "CI";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        // ACSES CO OUT
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x02) == 0x02)
                                {
                                    return " ";
                                }

                                else
                                {
                                    return "CI";
                                }
                            }

                        // PERMSUPPR
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if ((paramByte & 0x04) == 0x04)
                                {
                                    return "PS";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 10:
                    switch (paramNum)
                    {
                        // SPEEDSENSOR
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] & 0xC0);

                                if (paramByte > 0)
                                {
                                    return "S";
                                }
                                else
                                {
                                    paramByte = (byte)(data[0] & 0x3E);

                                    if (paramByte > 0)
                                    {
                                        return "E";
                                    }
                                    else
                                    {
                                        return " ";
                                    }
                                }
                            }

                        default:
                            return "error";
                    }

                case 11:
                    switch (paramNum)
                    {
                        // PTSALARMSUP
                        case 0:
                            {
                                byte[] data1 = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte1 = (byte)(data1[0] & 0x10);

                                byte[] data2 = record.Tags.Find(X => X.TagID == 12).Data();
                                byte paramByte2 = (byte)(data2[0] & 0x10);

                                if (paramByte1 == 0x10 | paramByte2 == 0x10)
                                {
                                    return "S";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                case 16:
                    switch (paramNum)
                    {
                        // VFBREAK
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x00)
                                {
                                    return "V";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // PHABREAK
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x00)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TILTAUTH
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 1)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 17:
                    switch (paramNum)
                    {
                        // SERVICE PEN
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // EM PENALTY (LIRR only)
                        case 1:
                            {
                                // if LIRR
                                if (_railroadID == 1)
                                {
                                    byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                    byte paramByte = data[0];

                                    paramByte = (byte)((paramByte >> 1) & 0x01);

                                    if (paramByte == 0)
                                    {
                                        return "P";
                                    }
                                    else
                                    {
                                        return " ";
                                    }
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }
                case 18:
                    switch (paramNum)
                    {
                        // DT STEP
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte dtStep = data[0];

                                dtStep = (byte)((dtStep) & 0x0F);

                                int stepInt = Convert.ToInt16(dtStep);

                                return stepInt.ToString();
                            }

                        // DT SUSPENDED
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // DT SUCCESS
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // DT RUNNING
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "R";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }
                case 30:
                    switch (paramNum)
                    {
                        // TPFOUND
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // TPINWIN
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "W";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // TRAILINGOPR
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "T";
                                }
                                else
                                {
                                    return "L";
                                }

                            }

                        // ALARM
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // OVERSPEED
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "O";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // ACSES PEN
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // ANTENNAPOWER
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // DASHDASH
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)(paramByte & 0x80);

                                if (paramByte == 0x80)
                                {
                                    return "-";
                                }
                                else
                                {
                                    return " ";
                                }

                            }
                        default:
                            return "error";
                    }

                case 34:
                    switch (paramNum)
                    {
                        // RADIOCHAN
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte radioChan = (byte)((data[0] >> 5) & 0xE0);

                                int radiochanInt = Convert.ToInt16(radioChan);

                                return radiochanInt.ToString();
                            }

                        // BCPCOVERAGE
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "C";
                                }
                                else
                                {
                                    return " ";
                                }

                            }

                        // RADIO STATUS
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte radioStat = (byte)((data[0]) & 0x05);

                                if (radioStat == 0x05)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "E";
                                }
                            }
                        default:
                            return "error";

                    }

                case 38:
                    switch (paramNum)
                    {
                        // ACSES MVCO
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "CO";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TPOUTOFWIN
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // DATABASE OK
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x40) == 0x40))
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "D";
                                }
                            }

                        // TPMISSING
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                if (((paramByte & 0x10) == 0x10) | ((paramByte & 0x40) == 0x40))
                                {
                                    return "M";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TSRLISTOK
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "T";
                                }
                            }
                        default:
                            return "error";
                    }

                case 47:
                    switch (paramNum)
                    {
                        // PTSZONE
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte ptsZone = (byte)(data[0] & 0x0F);

                                if (ptsZone == 0x00)
                                {
                                    return " ";
                                }
                                else if ((ptsZone & 0x08) == 0x08)
                                {
                                    return "IXL";
                                }
                                else if ((ptsZone & 0x02) == 0x02)
                                {
                                    return "PDS";
                                }
                                else if ((ptsZone & 0x04) == 0x04)
                                {
                                    return "DS";
                                }
                                else if ((ptsZone & 0x01) == 0x01)
                                {
                                    return "PTS";
                                }
                                else
                                {
                                    return "?";
                                }
                            }

                        // RADIORELEASE
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "R";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // C SIGNAL
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x03);

                                if (paramByte == 0x03)
                                {
                                    return "C";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 58:
                    switch (paramNum)
                    {
                        // DECEL DIRECT
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)(paramByte & 0x08);

                                if (paramByte != 0x08)
                                {
                                    return "F";
                                }

                                else
                                {
                                    return "R";
                                }

                            }

                        default:
                            return "error";
                    }

                case 65:
                    switch (paramNum)
                    {
                        // ROLLAWAY PEN
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // GEARTEETH
                        case 1:
                            {
                                return ((int)(header[24])).ToString();
                            }

                        default:
                            return "error";
                    }

                case 76:
                    switch (paramNum)
                    {
                        // FAULT AUXIO
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT TACHIO
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT CONFIG
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT ENET
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT ADU 1
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT ADU 2
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT DECEL
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT ATC
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 2) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // FAULT AIU
                        case 8:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 77:
                    switch (paramNum)
                    {
                        // FAULT TPR
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 78:
                    switch (paramNum)
                    {
                        // BRAKE ASR
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // ABS CMB TER
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 1) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "O";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // ZS TSR IP
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "IP";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TUNNEL AVOID
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte tunnelAvoid = (data[1]);

                                if ((tunnelAvoid & 0x40) == 0x40)
                                {
                                    return "IG";
                                }
                                else if ((tunnelAvoid & 0x20) == 0x20)
                                {
                                    return "LZ";
                                }
                                else if ((tunnelAvoid & 0x10) == 0x10)
                                {
                                    return "IP";
                                }
                                else if ((tunnelAvoid & 0x04) == 0x04)
                                {
                                    return "DS";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TB PTS EN
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 7) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TB PTS ACT
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "A";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // SAR TSR STS
                        case 6:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 1) & 0x03);

                                int paramInt = Convert.ToInt16(paramByte);

                                return paramInt.ToString();
                            }

                        // TRS ENFORCED
                        case 7:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 3) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "E";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // DB SLIP IP
                        case 8:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "S";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // DB SLIDE IP
                        case 9:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 5) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "S";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // TCP/IP
                        case 10:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "D";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 84:
                    switch (paramNum)
                    {
                        // ACSESPTSREQ
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "P";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // ACKREQ
                        case 1:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[1];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "Q";
                                }
                            }

                        // ACSES CO FLS
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "F";
                                }
                                else
                                {
                                    return " ";
                                }
                            }

                        // ADUVZ
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 4) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return " ";
                                }
                                else
                                {
                                    return "VZ";
                                }
                            }

                        // ACSES DASHDASH
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = data[0];

                                paramByte = (byte)((paramByte >> 6) & 0x01);

                                if (paramByte == 0x01)
                                {
                                    return "-";
                                }
                                else
                                {
                                    return " ";
                                }
                            }
                        default:
                            return "error";
                    }

                case 91:
                    switch (paramNum)
                    {
                        // ATCSEQNUM
                        case 0:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();
                                byte paramByte = (byte)(data[0] >> 1);

                                return paramByte.ToString();
                            }

                        // BCPNUM
                        case 1:
                            {


                                return "TODO";
                                //return BCPNUM(record);
                            }

                        // RADIORXERROR
                        case 2:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                int RRXerr = 0;

                                if (((data[RADIO_LENGTH - 1] & 0x01) == 0x01))
                                {
                                    switch (data[RADIO_LENGTH - 2])
                                    {
                                        case 0:
                                            RRXerr = 0;
                                            break;

                                        case 11:
                                            {
                                                if (data[7] == 0x90)
                                                {
                                                    RRXerr = 0;
                                                }
                                                else
                                                {
                                                    RRXerr = 1;
                                                }
                                                break;
                                            }
                                        case 3:
                                            RRXerr = 0;
                                            break;

                                        default:
                                            RRXerr = 0;
                                            break;
                                    }
                                }

                                if (RRXerr == 1)
                                    return "E";

                                else if (RRXerr == 0)
                                    return " ";

                                else
                                    return "PTE bug";
                            }

                        // RADIORXINF
                        case 3:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                int RRXinf = 0;

                                if (((data[RADIO_LENGTH - 1] & 0x01) == 0x01))
                                {
                                    switch (data[RADIO_LENGTH - 2])
                                    {
                                        case 9:
                                            RRXinf = 0;
                                            break;

                                        case 11:
                                            {
                                                if (data[7] == 0x90)
                                                {
                                                    RRXinf = 2;
                                                }
                                                else
                                                {
                                                    RRXinf = 1;
                                                }
                                                break;
                                            }
                                        case 3:
                                            RRXinf = 2;
                                            break;

                                        default:
                                            RRXinf = 2;
                                            break;
                                    }
                                }

                                if (RRXinf == 0)
                                    return " ";

                                else if (RRXinf == 1)
                                    return "R";

                                else if (RRXinf == 2)
                                    return "I";

                                else
                                    return "PTE bug";
                            }

                        // RADIOTRAFFIC
                        case 4:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte radioTraffic = data[RADIO_LENGTH - 1];

                                if (radioTraffic == 0x02)
                                {
                                    return "T";
                                }

                                else if ((radioTraffic & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }

                            }

                        // X|TSRTYPE
                        case 5:
                            {
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte radioTraffic = data[RADIO_LENGTH - 1];

                                if (radioTraffic == 0x02)
                                {
                                    return "??";
                                }

                                else if ((radioTraffic & 0x01) == 0x01)
                                {
                                    return "%02X";
                                }

                                else
                                {
                                    return "??";
                                }

                            }

                        // RADIOTXRXENC
                        case 6:
                            {
                                int rtXrXenc = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 5 || msgtype == 11 || msgtype == 8 || msgtype == 2 || msgtype == 13)
                                {
                                    rtXrXenc = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXenc == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXenc & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }

                            }

                        // RADIOTXRXTSR
                        case 7:
                            {
                                int rtXrXtsr = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 3 || msgtype == 4 || msgtype == 12)
                                {
                                    rtXrXtsr = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXtsr == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXtsr & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        // RADIOTXRXMTA
                        case 8:
                            {
                                int rtXrXmta = 0;
                                byte[] data = record.Tags.Find(X => X.TagID == id).Data();

                                byte msgtype = (byte)(data[7] >> 4);

                                if (msgtype == 9)
                                {
                                    rtXrXmta = data[RADIO_LENGTH - 1];
                                }

                                if (rtXrXmta == 0x02)
                                {
                                    return "T";
                                }

                                else if ((rtXrXmta & 0x01) == 0x01)
                                {
                                    return "R";
                                }

                                else
                                {
                                    return " ";
                                }
                            }

                        default:
                            return "error";
                    }

                default:
                    return "error";
            }
        }
        */
        #endregion
    }
}
