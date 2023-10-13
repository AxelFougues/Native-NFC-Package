using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AzApps.NativeNFC {

    public class NFCUtils : MonoBehaviour {

        #region LOOKUP_TABLES
        public static Dictionary<string, string> NTAG_VERSION_DATA_IC = new Dictionary<string, string>() {

    { "0004040102000B03", "NTAG 210u NT2L1001" },
    { "0004040202000B03", "NTAG 210u NT2H1001" },

    { "0004040101000B03", "NTAG 210" },
    { "0004040101000E03", "NTAG 212" },

    { "0004040201000F03", "NTAG 213" },
    { "0004040401000F03", "NTAG 213F" },
    { "0004040203000F03", "NTAG 213TT" },
    { "0004040201001103", "NTAG 215" },
    { "0004040201001303", "NTAG 216" },
    { "0004040401001303", "NTAG 216F" },

    { "0004040204000F03", "NTAG 223 DNA" },
    { "0004040804000F03", "NTAG 223 DNA SD" },
    { "0004040205001003", "NTAG 224 DNA" },
    { "0004040805001003", "NTAG 224 DNA SD" },

    { "0004040502011303", "NTAG I2C 1k" },
    { "0004040502011503", "NTAG I2C 2k" },

    { "0004040502021303", "NTAG I2C plus 1k" },
    { "0004040502021503", "NTAG I2C plus 2k" },

    { "0004030101000B03", "MIFARE Ultralight EV1 11" },
    { "0004030201000B03", "MIFARE Ultralight EV1 H11" },

    { "0004030101000E03", "MIFARE Ultralight EV1 21" },
    { "0004030201000E03", "MIFARE Ultralight EV1 H21" },

    { "0004030104000F03" , "MIFARE Ultralight AES"},
    { "0004030204000F03" , "MIFARE Ultralight AES H"},

    { "0004030102000B03" , "MIFARE Ultralight Nano"},
    { "0004030202000B03" , "MIFARE Ultralight Nano H"},

    //older
    { "N203", "NTAG 203" },
    { "MUC", "MIFARE Ultralight C" },
    { "MU", "MIFARE Ultralight" },
    { "MH", "MIFARE Hospitality" },

    //Classic
    { "MM3", "Mifare Mini 0.3K" },
    { "MC1", "MIFARE Classic 1k" },
    { "MC2", "MIFARE Classic 2k" },
    { "MC4", "MIFARE Classic 4k" },
    { "MCEV11", "MIFARE Classic EV1 1k" },
    { "MCEV14", "MIFARE Classic EV1 4k" },

};

        public static Dictionary<string, string> manufacturers = new Dictionary<string, string>() {
    {"01", "Motorola (UK)"},
    {"02", "STMicroelectronics SA (FR)"},
    {"03", "Hitachi Ltd (JP)"},
    {"04", "NXP Semiconductors (DE)"},
    {"05", "Infineon Technologies AG (DE)"},
    {"06", "Cylink (US)"},
    {"07", "Texas Instruments (FR)"},
    {"08", "Fujitsu Limited (JP)"},
    {"09", "Matsushita Electronics Corporation, Semiconductor Company (JP)"},
    {"0A", "NEC (JP)"},
    {"0B", "Oki Electric Industry Co Ltd (JP)"},
    {"0C", "Toshiba Corp (JP)"},
    {"0D", "Mitsubishi Electric Corp (JP)"},
    {"0E", "Samsung Electronics Co Ltd (KR)"},
    {"0F", "Hynix (KR)"},
    {"10", "LG-Semiconductors Co Ltd (KR)"},
    {"11", "Emosyn-EM Microelectronics (US)"},
    {"12", "INSIDE Technology (FR)"},
    {"13", "ORGA Kartensysteme GmbH (DE)"},
    {"14", "Sharp Corporation (JP)"},
    {"15", "ATMEL (FR)"},
    {"16", "EM Microelectronic-Marin (CH)"},
    {"17", "SMARTRAC TECHNOLOGY GmbH (DE)"},
    {"18", "ZMD AG (DE)"},
    {"19", "XICOR Inc (US)"},
    {"1A", "Sony Corporation (JP)"},
    {"1B", "Malaysia Microelectronic Solutions Sdn Bhd (MY)"},
    {"1C", "Emosyn (US)"},
    {"1D", "Shanghai Fudan Microelectronics Co Ltd (CN)"},
    {"1E", "Magellan Technology Pty Limited (AU)"},
    {"1F", "Melexis NV BO (CH)"},
    {"20", "Renesas Technology Corp (JP)"},
    {"21", "TAGSYS (FR)"},
    {"22", "Transcore (US)"},
    {"23", "Shanghai Belling Corp Ltd (CN)"},
    {"24", "Masktech Germany GmbH (DE)"},
    {"25", "Innovision Research and Technology Plc (UK)"},
    {"26", "Hitachi ULSI Systems Co Ltd (JP)"},
    {"27", "Yubico AB (SE)"},
    {"28", "Ricoh (JP)"},
    {"29", "ASK (FR)"},
    {"2A", "Unicore Microsystems LLC (RU)"},
    {"2B", "Dallas semiconductor/Maxim (US)"},
    {"2C", "Impinj Inc (US)"},
    {"2D", "RightPlug Alliance (US)"},
    {"2E", "Broadcom Corporation (US)"},
    {"2F", "MStar Semiconductor Inc (TW)"},
    {"30", "BeeDar Technology Inc (US)"},
    {"31", "RFIDsec (DK)"},
    {"32", "Schweizer Electronic AG (DE)"},
    {"33", "AMIC Technology Corp (TW)"},
    {"34", "Mikron JSC (RU)"},
    {"35", "Fraunhofer Institute for Photonic Microsystems (DE)"},
    {"36", "IDS Microship AG (CH)"},
    {"37", "Kovio (US)"},
    {"38", "HMT Microelectronic Ltd (CH)"},
    {"39", "Silicon Craft Technology (TH)"},
    {"3A", "Advanced Film Device Inc. (JP)"},
    {"3B", "Nitecrest Ltd (UK)"},
    {"3C", "Verayo Inc. (US)"},
    {"3D", "HID Global (US)"},
    {"3E", "Productivity Engineering Gmbh (DE)"},
    {"3F", "Austriamicrosystems AG (reserved) (AT)"},
    {"40", "Gemalto SA (FR)"},
    {"41", "Renesas Electronics Corporation (JP)"},
    {"42", "3Alogics Inc (KR)"},
    {"43", "Top TroniQ Asia Limited (Hong Kong)"},
    {"44", "Gentag Inc (USA)"},
    {"45", "Invengo Information Technology Co.Ltd (CN)"},
    {"46", "Guangzhou Sysur Microelectronics, Inc (CN)"},
    {"47", "CEITEC S.A. (BR)"},
    {"48", "Shanghai Quanray Electronics Co. Ltd. (CN)"},
    {"49", "MediaTek Inc (TW)"},
    {"4A", "Angstrem PJSC (RU)"},
    {"4B", "Celisic Semiconductor (Hong Kong) Limited (CN)"},
    {"4C", "LEGIC Identsystems AG (CH)"},
    {"4D", "Balluff GmbH (DE)"},
    {"4E", "Oberthur Technologies (FR)"},
    {"4F", "Silterra Malaysia Sdn. Bhd. (MY)"},
    {"50", "DELTA Danish Electronics, Light & Acoustics (DK)"},
    {"51", "Giesecke & Devrient GmbH (DE)"},
    {"52", "Shenzhen China Vision Microelectronics Co., Ltd. (CN)"},
    {"53", "Shanghai Feiju Microelectronics Co. Ltd. (CN)"},
    {"54", "Intel Corporation (US)"},
    {"55", "Microsensys GmbH (DE)"},
    {"56", "Sonix Technology Co., Ltd. (TW)"},
    {"57", "Qualcomm Technologies Inc (US)"},
    {"58", "Realtek Semiconductor Corp (TW)"},
    {"59", "Freevision Technologies Co. Ltd (CN)"},
    {"5A", "Giantec Semiconductor Inc. (CN)"},
    {"5B", "JSC Angstrem-T (RU)"},
    {"5C", "STARCHIP France"},
    {"5D", "SPIRTECH (FR)"},
    {"5E", "GANTNER Electronic GmbH (AT)"},
    {"5F", "Nordic Semiconductor (NO)"},
    {"60", "Verisiti Inc (US)"},
    {"61", "Wearlinks Technology Inc. (CN)"},
    {"62", "Userstar Information Systems Co., Ltd (TW)"},
    {"63", "Pragmatic Printing Ltd. (UK)"},
    {"64", "Associação do Laboratório de Sistemas Integráveis Tecnológico – LSI-TEC (BR)"},
    {"65", "Tendyron Corporation (CN)"},
    {"66", "MUTO Smart Co., Ltd.(KR)"},
    {"67", "ON Semiconductor (US)"},
    {"68", "TUBITAK BILGEM (TR)"},
    {"69", "Huada Semiconductor Co., Ltd (CN)"},
    {"6A", "SEVENEY (FR)"},
    {"6B", "ISSM (FR)"},
    {"6C", "Wisesec Ltd (IL)"},
    {"7E", "Holtek (TW)"}
};

        public enum WellKnownRecordType { None, Text, URI, Smart_Poster, Alternative_Carrier, Handover_Carrier, Handover_Request, Handover_Select }
        public static Dictionary<WellKnownRecordType, byte[]> WELL_KNOWN_RECORD_TYPE_VALUES = new Dictionary<WellKnownRecordType, byte[]> {
    { WellKnownRecordType.None, null },
    { WellKnownRecordType.Text, new byte[]{ 0x54 } },
    { WellKnownRecordType.URI, new byte[]{ 0x55 } },
    { WellKnownRecordType.Smart_Poster, new byte[]{ 0x53, 0x70 } },
    { WellKnownRecordType.Alternative_Carrier, new byte[]{ 0x61, 0x63 } },
    { WellKnownRecordType.Handover_Carrier, new byte[]{ 0x48, 0x63 } },
    { WellKnownRecordType.Handover_Request, new byte[]{ 0x48, 0x72 } },
    { WellKnownRecordType.Handover_Select, new byte[]{ 0x48, 0x73 } }

};

        public static Dictionary<byte, string> URI_IDENTIFIER_CODES = new Dictionary<byte, string> {
    { 0x00, "" },
    { 0x01, "http://www." },
    { 0x02, "https://www." },
    { 0x03, "http://" },
    { 0x04, "https://" },
    { 0x05, "tel:" },
    { 0x06, "mailto:" },
    { 0x07, "ftp://anonymous:anonymous@" },
    { 0x08, "ftp://ftp." },
    { 0x09, "ftps:/" },
    { 0x0A, "sftp://" },
    { 0x0B, "smb://" },
    { 0x0C, "nfs://" },
    { 0x0D, "ftp://" },
    { 0x0E, "dav://" },
    { 0x0F, "news:" },
    { 0x10, "telnet://" },
    { 0x11, "imap:" },
    { 0x12, "rtsp://" },
    { 0x13, "urn:" },
    { 0x14, "pop:" },
    { 0x15, "sip:" },
    { 0x16, "sips:" },
    { 0x17, "tftp:" },
    { 0x18, "btspp://" },
    { 0x19, "btl2cap://" },
    { 0x1A, "btgoep://" },
    { 0x1B, "tcpobex://" },
    { 0x1C, "irdaobex://" },
    { 0x1D, "file://" },
    { 0x1E, "urn:epc:id:" },
    { 0x1F, "urn:epc:tag:" },
    { 0x20, "urn:epc:pat:" },
    { 0x21, "urn:epc:raw:" },
    { 0x22, "urn:epc:" },
    { 0x23, "urn:nfc:" }
};


        public enum ExternalRecordType { None, Android_Package }
        public static Dictionary<ExternalRecordType, byte[]> EXTERNAL_RECORD_TYPE_VALUES = new Dictionary<ExternalRecordType, byte[]> {
    { ExternalRecordType.None, null },
    { ExternalRecordType.Android_Package, new byte[]{ 0x61, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x2e, 0x63, 0x6f, 0x6d, 0x3a, 0x70, 0x6b, 0x67 } }

};

        #endregion

        public static string idToReadableManufacturer(string input) {
            if (string.IsNullOrEmpty(input)) return "N/A";
            if (input.Length == 2 && manufacturers.ContainsKey(input.ToUpper())) return input.ToUpper() + "-" + manufacturers[input.ToUpper()];
            return input;
        }

        public static string bytesToText(byte[] bytes) {
            string s = Regex.Replace(Encoding.UTF8.GetString(bytes), @"[^\u0020-\u007E]", ".");
            //s = Regex.Replace(s, @"[\u0020-\u007E]", ".");
            return s;
        }

        public static string bytesToHexString(byte[] bytes) {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat(" {0:x2}", b);
            return hex.ToString();
        }

        public static string byteToHexString(byte b) {
            StringBuilder hex = new StringBuilder(2);
            hex.AppendFormat(" {0:x2}", b);
            return hex.ToString();
        }

        public static string hexStringToText(string hexString) {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++) {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Regex.Replace(Encoding.UTF8.GetString(bytes), @"[^\u0020-\u007E]+", "_");
        }

        public static byte[] hexStringToBytes(string hexString) {
            return BigInteger.Parse(hexString, System.Globalization.NumberStyles.HexNumber).ToByteArray().Reverse().ToArray();
        }

        public static int[] hexStringToInts(string hexString) {
            return bytesToInts(hexStringToBytes(hexString));
        }

        public static int[] bytesToInts(byte[] bytes) {
            int[] ints = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++) ints[i] = bytes[i];
            return ints;
        }

        public static byte[] intsToBytes(int[] ints) {
            byte[] bytes = new byte[ints.Length];
            for (int i = 0; i < ints.Length; i++) bytes[i] = (byte)ints[i];
            return bytes;
        }

        public static DateTime stringToDateTime(string time) {
            if (string.IsNullOrWhiteSpace(time)) return DateTime.Now;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try {
                return DateTime.ParseExact(time, "yyyy_MM_dd_HH_mm", provider);
            } catch (Exception e) {
                return DateTime.Now;
            }
        }

        public static string dateTimeToString(DateTime time) {
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.Now.ToString("yyyy_MM_dd_HH_mm", provider);
        }
    }
}