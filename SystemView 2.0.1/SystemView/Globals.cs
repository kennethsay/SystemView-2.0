
#define MTA
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SystemView
{

    static class Globals
    {

#if MTA
        public const UInt16 IXLSTATUSREQMSG = 0xB841;
        public const UInt16 IXLSTATUSRESWLOMAMSG = 0xB881;
        public const UInt16 IXLSTATUSRESWOUTLOMAMSG = 0xB882;
        public const UInt16 TSRLISTREQMSG = 0xB842;
        public const UInt16 TSRNOCHANGERESMSG = 0xB884;
        public const UInt16 TSRLISTRESMSG = 0xB883;
        public const UInt16 MAINTENANCEMSGREQ = 0xB902;
        public const UInt16 MAINTENANCEMSGACK = 0xB903;


        public const byte RadioMsgTagIndex = 91;
        public const byte TPMsgTagIndex = 90;

        public static readonly List<string> IORIBBONITEMS = new List<string>()
        {
            "ACSES Status", 
            "ATC Status", 
            "AIU Aspects",
            "Cab Status",
            "Comm Status",
            "Departure Test",
            "Speed Sensor",
            "Other"
        };

        public static readonly List<string> IORIBBONCABITEMS = new List<string>()
        {
            "Front Cab Active",            
            "F CAB Reverser Forward",
            "F CAB Reverser Neutral",
            "F CAB Reverser Reverse",
            "Rear Cab Active",
            "R CAB Reverser Forward",
            "R CAB Reverser Neutral",
            "R CAB Reverser Reverse",
            "ACK",
            "ACK Pedal",
            "ASB",
            "Overspeed",
            "Alarm"
        };

        public static readonly List<string> IORIBBONACSESITEMS = new List<string>()
        {
            "ACSES CUTOUT",
            "ACSES CUTIN/OPR",
            "TRAILING",
            "PTS (ABS. STOP REQ)"
        };

        public static readonly List<string> IORIBBONATCITEMS = new List<string>()
        {
            "ATC CUTOUT VIA ADU",
            "ATC CUTIN VIA ADU",
            "ATC HARDWIRE CUTOUT"
        };

        public static readonly List<string> IORIBBONAIUITEMS = new List<string>()
        {
            "STOP",
            "30/38",
            "55/80",
            "70",
            "RES/15",
            "60/40",
            "60",
            "80",
            "20/30",
            "80/45/70",
            "65",
            "FAULT"
        };

        public static readonly List<string> IORIBBONCOMMITEMS = new List<string>()
        {
            "AUX",
            "DECEL",
            "E-NET",
            "AIU",
            "ATC",
            "TTSS",
            "CHMM",
            "TACH",
            "CFG",
            "ADU1",
            "ADU2",
            "TPR",
            "RADIO"
        };

        public static readonly List<string> IORIBBONDTITEMS = new List<string>()
        {
            "RUNNING",
            "RESULT"
        };

        public static readonly List<string> IORIBBONSSITEMS = new List<string>()
        {
            "SLD",
            "SLP",
            "PE",
            "TE",
            "WE",
            "CE",
            "MOV"
        };

        public static readonly List<string> IORIBBONOTHERITEMS = new List<string>()
        {
            "VZ",
            "SERVICE PENALTY",
            "EM PENALTY",
            "PERM SUPPR",
            "PTS ALARM SUPPR",
            "RCV"
        };

        public static readonly List<string> RAILROADS = new List<string>()
        {
            "LIRR",
            "MNR"
        };

        public static readonly List<string> TRAINTYPESLIRR = new List<string>()
        {
            "B - Passenger",
            "E - Freight"
        };

        public static readonly List<string> TRAINTYPESMNR = new List<string>()
        {
            "B - Passenger",
            "C - Passenger",
            "D - Freight",
        };

        public static readonly List<string> VEHICLETYPESMNR = new List<string>()
        {
            "M7A",
            "M3A",
            "CONTRACT 12",
            "CONTRACT 19",
            "CONTRACT 21",
            "CONTRACT 34",
            "CONTRACT 38/38A/82",
            "P32AC-DM",
            "BL-14CG",
            "BL-20GH",
            "GP-35",
            "M9",
        };

        public static readonly List<string> VEHICLETYPESLIRR = new List<string>()
        {
            "M3",
            "M7"
        };

        public static readonly List<string> DATALOGDEVICESMNR = new List<string>()
        {
            "NONE",
            "ARMM",
            "CHMM",
            "BOTH FOR M9"
        };

        public static readonly List<string> DATALOGDEVICESLIRR = new List<string>()
        {
            "CHMM"
        };

        public static readonly List<string> DECELDIRECTION = new List<string>()
        {
            "Forward (Normal)",
            "Reverse (VEH. Specific)"
        };
        public static readonly List<string> CONFIGCHECKBOXPARAMSMTA = new List<string>()
        {
            "HST Tilt Bypass",
            "DT Antenna Bypass",
            "Radio DT Bypass",
            "Single Cab Vehicle",
            "AIU Decel Communications",
            "Reverse Operation Allowed",
            "Short Hood Forward",
            "Long Hood Forward",
            "Married Pair",
            "BA Equipped",
            "Pulse Emergency Brake",
            "2 Second Emergency Brake Pulse",
            "Tunnel Enable",
            "3 Second Alarm Delay",
            "Slip/Slide Bypass Before Crosscheck",
            "Total Slip/Slide Bypass"
        };

        public static readonly List<string> REVISIONAPPS = new List<string>()
        {
            "Main",
            "Aux I/O",
            "Tach I/O",
            "AIU",
            "COMM",
            "Config",
            "Decel",
            "TP RDR",
            "ADU 1",
            "ADU 2",
            "ADU 3",
            "ADU 4",
            "Ethernet",
            "Netburner",
            "ARMM",
            "CHMM"
        };

#endif


        public const int LOWERNIBBLE = 15;
        public const int UPPERNIBBLE = 240;
        public const int BIT0 = 1;
        public const int BIT1 = 2;
        public const int BIT2 = 4;
        public const int BIT3 = 8;
        public const int BIT4 = 16;
        public const int BIT5 = 32;
        public const int BIT6 = 64;
        public const int BIT7 = 128;





    }


}
