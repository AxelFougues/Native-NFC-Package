using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AbyssWalkerDev.NativeNFC {

    public class NFCUtils : MonoBehaviour {

        public enum NFC_IC {
            unknown,
            NTAG_203,
            NTAG_210u_NT2L1001,
            NTAG_210u_NT2H1001,
            NTAG_210,
            NTAG_212,
            NTAG_213,
            NTAG_213_F,
            NTAG_213_TT,
            NTAG_215,
            NTAG_216,
            NTAG_216_F,
            NTAG_223_DNA,
            NTAG_223_DNA_SD,
            NTAG_224_DNA,
            NTAG_224_DNA_SD,
            NTAG_I2C_1k,
            NTAG_I2C_2k,
            NTAG_I2C_plus_1k,
            NTAG_I2C_plus_2k,

            MIFARE_Ultralight_EV1_11,
            MIFARE_Ultralight_EV1_H11,
            MIFARE_Ultralight_EV1_21,
            MIFARE_Ultralight_EV1_H21,
            MIFARE_Ultralight_AES,
            MIFARE_Ultralight_AES_H,
            MIFARE_Ultralight_Nano,
            MIFARE_Ultralight_Nano_H,
            MIFARE_Ultralight_C,
            MIFARE_Ultralight,

            MIFARE_Plus_2k,
            MIFARE_Plus_4k,
            MIFARE_Plus_EV1_2k,
            MIFARE_Plus_EV1_4k,
            MIFARE_Plus_EV2_2k,
            MIFARE_Plus_EV2_4k,
            MIFARE_Plus_S_2k,
            MIFARE_Plus_S_4k,
            MIFARE_Plus_X_2k,
            MIFARE_Plus_X_4k,
            MIFARE_Plus_SE_1k,

            MIFARE_Hospitality,
            MIFARE_Mini_03K,

            MIFARE_Classic_1k,
            MIFARE_Classic_2k,
            MIFARE_Classic_4k,
            MIFARE_Classic_EV1_1k,
            MIFARE_Classic_EV1_4k,

            MIFARE_DESFire_Light,
            MIFARE_DESFire_EV1_256,
            MIFARE_DESFire_EV1_2k,
            MIFARE_DESFire_EV1_4k,
            MIFARE_DESFire_EV1_6k,
            MIFARE_DESFire_EV1_8k,
            MIFARE_DESFire_EV2_2k,
            MIFARE_DESFire_EV2_4k,
            MIFARE_DESFire_EV2_8k,
            MIFARE_DESFire_EV2_16k,
            MIFARE_DESFire_EV2_32k,
            MIFARE_DESFire_EV3_2k,
            MIFARE_DESFire_EV3_4k,
            MIFARE_DESFire_EV3_6k,

            SmartMX,

            ICODE_SLIX2,
            HID_iClass,
        }

        public enum NFC_Manufacturers {
            unknown,
            Motorola_UK,
            STMicroelectronics_SA,
            Hitachi_Ltd,
            NXP_Semiconductors,
            Infineon_Technologies_AG,
            Cylink,
            Texas_Instruments,
            Fujitsu_Limited,
            Matsushita_Electronics_Corporation_Semiconductor_Company,
            NEC,
            Oki_Electric_Industry_Co_Ltd,
            Toshiba_Corp,
            Mitsubishi_Electric_Corp,
            Samsung_Electronics_Co_Ltd,
            Hynix,
            Emosyn_EM_Microelectronics,
            LG_Semiconductors_Co_Ltd,
            INSIDE_Technology,
            ORGA_Kartensysteme_GmbH,
            Sharp_Corporation,
            ATMEL,
            EM_Microelectronic_Marin,
            SMARTRAC_TECHNOLOGY_GmbH,
            ZMD_AG,
            XICOR_Inc,
            Sony_Corporation,
            Malaysia_Microelectronic_Solutions_Sdn_Bhd,
            Emosyn,
            Shanghai_Fudan_Microelectronics_Co_Ltd,
            Magellan_Technology_Pty_Limited,
            Melexis_NV_BO,
            Renesas_Technology_Corp,
            TAGSYS,
            Transcore,
            Shanghai_Belling_Corp_Ltd,
            Masktech_Germany_GmbH,
            Innovision_Research_and_Technology_Plc,
            Hitachi_ULSI_Systems_Co_Ltd,
            Yubico_AB,
            Ricoh,
            ASK,
            Unicore_Microsystems_LLC,
            Dallas_semiconductor_Maxim,
            Impinj_Inc,
            RightPlug_Alliance,
            Broadcom_Corporation,
            MStar_Semiconductor_Inc,
            BeeDar_Technology_Inc,
            RFIDsec,
            Schweizer_Electronic_AG,
            AMIC_Technology_Corp,
            Mikron_JSC,
            Fraunhofer_Institute_for_Photonic_Microsystems,
            IDS_Microship_AG,
            Kovio,
            HMT_Microelectronic_Ltd,
            Silicon_Craft_Technology,
            Advanced_Film_Device_Inc,
            Nitecrest_Ltd,
            Verayo_Inc,
            HID_Global,
            Productivity_Engineering_Gmbh,
            Austriamicrosystems_AG,
            Gemalto_SA,
            Renesas_Electronics_Corporation,
            _3Alogics_Inc,
            Top_TroniQ_Asia_Limited,
            Gentag_Inc,
            Invengo_Information_Technology_Co_Ltd,
            Guangzhou_Sysur_Microelectronics_Inc,
            CEITEC_S_A,
            Shanghai_Quanray_Electronics_Co_Ltd,
            MediaTek_Inc,
            Angstrem_PJSC,
            Celisic_Semiconductor_Hong_Kong_Limited,
            LEGIC_Identsystems_AG,
            Balluff_GmbH,
            Oberthur_Technologies,
            Silterra_Malaysia_Sdn_Bhd,
            DELTA_Danish_Electronics_Light_Acoustics,
            Giesecke_Devrient_GmbH,
            Shenzhen_China_Vision_Microelectronics_Co_Ltd,
            Shanghai_Feiju_Microelectronics_Co_Ltd,
            Intel_Corporation,
            Microsensys_GmbH,
            Sonix_Technology_Co_Ltd,
            Qualcomm_Technologies_Inc,
            Realtek_Semiconductor_Corp,
            Freevision_Technologies_Co_Ltd,
            Giantec_Semiconductor_Inc,
            JSC_Angstrem_T,
            STARCHIP_France,
            SPIRTECH,
            GANTNER_Electronic_GmbH,
            Nordic_Semiconductor,
            Verisiti_Inc,
            Wearlinks_Technology_Inc,
            Userstar_Information_Systems_Co_Ltd,
            Pragmatic_Printing_Ltd,
            Associacao_do_Laboratorio_de_Sistemas_Integraveis_Tecnologico_LSI_TEC,
            Tendyron_Corporation,
            MUTO_Smart_Co_Ltd,
            ON_Semiconductor,
            TUBITAK_BILGEM,
            Huada_Semiconductor_Co_Ltd,
            SEVENEY,
            ISSM,
            Wisesec_Ltd,
            Holtek
        }

        #region LOOKUP_TABLES
        public static Dictionary<string, NFC_IC> NTAG_VERSION_DATA_IC = new Dictionary<string, NFC_IC>() {

    { "0004040102000B03", NFC_IC.NTAG_210u_NT2L1001 },
    { "0004040202000B03", NFC_IC.NTAG_210u_NT2H1001 },

    { "0004040101000B03", NFC_IC.NTAG_210 },
    { "0004040101000E03", NFC_IC.NTAG_212 },

    { "0004040201000F03", NFC_IC.NTAG_213 },
    { "0004040401000F03", NFC_IC.NTAG_213_F },
    { "0004040203000F03", NFC_IC.NTAG_213_TT },
    { "0004040201001103", NFC_IC.NTAG_215 },
    { "0004040201001303", NFC_IC.NTAG_216 },
    { "0004040401001303", NFC_IC.NTAG_216_F },

    { "0004040204000F03", NFC_IC.NTAG_223_DNA },
    { "0004040804000F03", NFC_IC.NTAG_223_DNA_SD },
    { "0004040205001003", NFC_IC.NTAG_224_DNA },
    { "0004040805001003", NFC_IC.NTAG_224_DNA_SD },

    { "0004040502011303", NFC_IC.NTAG_I2C_1k },
    { "0004040502011503", NFC_IC.NTAG_I2C_2k },

    { "0004040502021303", NFC_IC.NTAG_I2C_plus_1k },
    { "0004040502021503", NFC_IC.NTAG_I2C_plus_2k },

    { "0004030101000B03", NFC_IC.MIFARE_Ultralight_EV1_11 },
    { "0004030201000B03", NFC_IC.MIFARE_Ultralight_EV1_H11 },

    { "0004030101000E03", NFC_IC.MIFARE_Ultralight_EV1_21 },
    { "0004030201000E03", NFC_IC.MIFARE_Ultralight_EV1_H21 },

    { "0004030104000F03" , NFC_IC.MIFARE_Ultralight_AES},
    { "0004030204000F03" , NFC_IC.MIFARE_Ultralight_AES_H},

    { "0004030102000B03" , NFC_IC.MIFARE_Ultralight_Nano},
    { "0004030202000B03" , NFC_IC.MIFARE_Ultralight_Nano_H},

    //older
    { "N203", NFC_IC.NTAG_203 },
    { "MUC", NFC_IC.MIFARE_Ultralight_C },
    { "MU", NFC_IC.MIFARE_Ultralight },
    { "MH", NFC_IC.MIFARE_Hospitality },

    //Classic
    { "MM3", NFC_IC.MIFARE_Mini_03K },
    { "MC1", NFC_IC.MIFARE_Classic_1k },
    { "MC2", NFC_IC.MIFARE_Classic_2k },
    { "MC4", NFC_IC.MIFARE_Classic_4k },
    { "MCEV11", NFC_IC.MIFARE_Classic_EV1_1k },
    { "MCEV14", NFC_IC.MIFARE_Classic_EV1_4k },

};

        public static Dictionary<string, NFC_Manufacturers> manufacturers = new Dictionary<string, NFC_Manufacturers>() {
    {"01", NFC_Manufacturers.Motorola_UK},
    {"02", NFC_Manufacturers.STMicroelectronics_SA},
    {"03", NFC_Manufacturers.Hitachi_Ltd},
    {"04", NFC_Manufacturers.NXP_Semiconductors},
    {"05", NFC_Manufacturers.Infineon_Technologies_AG},
    {"06", NFC_Manufacturers.Cylink},
    {"07", NFC_Manufacturers.Texas_Instruments},
    {"08", NFC_Manufacturers.Fujitsu_Limited},
    {"09", NFC_Manufacturers.Matsushita_Electronics_Corporation_Semiconductor_Company},
    {"0A", NFC_Manufacturers.NEC},
    {"0B", NFC_Manufacturers.Oki_Electric_Industry_Co_Ltd},
    {"0C", NFC_Manufacturers.Toshiba_Corp},
    {"0D", NFC_Manufacturers.Mitsubishi_Electric_Corp},
    {"0E", NFC_Manufacturers.Samsung_Electronics_Co_Ltd},
    {"0F", NFC_Manufacturers.Hynix},
    {"10", NFC_Manufacturers.LG_Semiconductors_Co_Ltd},
    {"11", NFC_Manufacturers.Emosyn_EM_Microelectronics},
    {"12", NFC_Manufacturers.INSIDE_Technology},
    {"13", NFC_Manufacturers.ORGA_Kartensysteme_GmbH},
    {"14", NFC_Manufacturers.Sharp_Corporation},
    {"15", NFC_Manufacturers.ATMEL},
    {"16", NFC_Manufacturers.EM_Microelectronic_Marin},
    {"17", NFC_Manufacturers.SMARTRAC_TECHNOLOGY_GmbH},
    {"18", NFC_Manufacturers.ZMD_AG},
    {"19", NFC_Manufacturers.XICOR_Inc},
    {"1A", NFC_Manufacturers.Sony_Corporation},
    {"1B", NFC_Manufacturers.Malaysia_Microelectronic_Solutions_Sdn_Bhd},
    {"1C", NFC_Manufacturers.Emosyn},
    {"1D", NFC_Manufacturers.Shanghai_Fudan_Microelectronics_Co_Ltd},
    {"1E", NFC_Manufacturers.Magellan_Technology_Pty_Limited},
    {"1F", NFC_Manufacturers.Melexis_NV_BO},
    {"20", NFC_Manufacturers.Renesas_Technology_Corp},
    {"21", NFC_Manufacturers.TAGSYS},
    {"22", NFC_Manufacturers.Transcore},
    {"23", NFC_Manufacturers.Shanghai_Belling_Corp_Ltd},
    {"24", NFC_Manufacturers.Masktech_Germany_GmbH},
    {"25", NFC_Manufacturers.Innovision_Research_and_Technology_Plc},
    {"26", NFC_Manufacturers.Hitachi_ULSI_Systems_Co_Ltd},
    {"27", NFC_Manufacturers.Yubico_AB},
    {"28", NFC_Manufacturers.Ricoh},
    {"29", NFC_Manufacturers.ASK},
    {"2A", NFC_Manufacturers.Unicore_Microsystems_LLC},
    {"2B", NFC_Manufacturers.Dallas_semiconductor_Maxim},
    {"2C", NFC_Manufacturers.Impinj_Inc},
    {"2D", NFC_Manufacturers.RightPlug_Alliance},
    {"2E", NFC_Manufacturers.Broadcom_Corporation},
    {"2F", NFC_Manufacturers.MStar_Semiconductor_Inc},
    {"30", NFC_Manufacturers.BeeDar_Technology_Inc},
    {"31", NFC_Manufacturers.RFIDsec},
    {"32", NFC_Manufacturers.Schweizer_Electronic_AG},
    {"33", NFC_Manufacturers.AMIC_Technology_Corp},
    {"34", NFC_Manufacturers.Mikron_JSC},
    {"35", NFC_Manufacturers.Fraunhofer_Institute_for_Photonic_Microsystems},
    {"36", NFC_Manufacturers.IDS_Microship_AG},
    {"37", NFC_Manufacturers.Kovio},
    {"38", NFC_Manufacturers.HMT_Microelectronic_Ltd},
    {"39", NFC_Manufacturers.Silicon_Craft_Technology},
    {"3A", NFC_Manufacturers.Advanced_Film_Device_Inc},
    {"3B", NFC_Manufacturers.Nitecrest_Ltd},
    {"3C", NFC_Manufacturers.Verayo_Inc},
    {"3D", NFC_Manufacturers.HID_Global},
    {"3E", NFC_Manufacturers.Productivity_Engineering_Gmbh},
    {"3F", NFC_Manufacturers.Austriamicrosystems_AG},
    {"40", NFC_Manufacturers.Gemalto_SA},
    {"41", NFC_Manufacturers.Renesas_Electronics_Corporation},
    {"42", NFC_Manufacturers._3Alogics_Inc},
    {"43", NFC_Manufacturers.Top_TroniQ_Asia_Limited},
    {"44", NFC_Manufacturers.Gentag_Inc},
    {"45", NFC_Manufacturers.Invengo_Information_Technology_Co_Ltd},
    {"46", NFC_Manufacturers.Guangzhou_Sysur_Microelectronics_Inc},
    {"47", NFC_Manufacturers.CEITEC_S_A},
    {"48", NFC_Manufacturers.Shanghai_Quanray_Electronics_Co_Ltd},
    {"49", NFC_Manufacturers.MediaTek_Inc},
    {"4A", NFC_Manufacturers.Angstrem_PJSC},
    {"4B", NFC_Manufacturers.Celisic_Semiconductor_Hong_Kong_Limited},
    {"4C", NFC_Manufacturers.LEGIC_Identsystems_AG},
    {"4D", NFC_Manufacturers.Balluff_GmbH},
    {"4E", NFC_Manufacturers.Oberthur_Technologies},
    {"4F", NFC_Manufacturers.Silterra_Malaysia_Sdn_Bhd},
    {"50", NFC_Manufacturers.DELTA_Danish_Electronics_Light_Acoustics},
    {"51", NFC_Manufacturers.Giesecke_Devrient_GmbH},
    {"52", NFC_Manufacturers.Shenzhen_China_Vision_Microelectronics_Co_Ltd},
    {"53", NFC_Manufacturers.Shanghai_Feiju_Microelectronics_Co_Ltd},
    {"54", NFC_Manufacturers.Intel_Corporation},
    {"55", NFC_Manufacturers.Microsensys_GmbH},
    {"56", NFC_Manufacturers.Sonix_Technology_Co_Ltd},
    {"57", NFC_Manufacturers.Qualcomm_Technologies_Inc},
    {"58", NFC_Manufacturers.Realtek_Semiconductor_Corp},
    {"59", NFC_Manufacturers.Freevision_Technologies_Co_Ltd},
    {"5A", NFC_Manufacturers.Giantec_Semiconductor_Inc},
    {"5B", NFC_Manufacturers.JSC_Angstrem_T},
    {"5C", NFC_Manufacturers.STARCHIP_France},
    {"5D", NFC_Manufacturers.SPIRTECH},
    {"5E", NFC_Manufacturers.GANTNER_Electronic_GmbH},
    {"5F", NFC_Manufacturers.Nordic_Semiconductor},
    {"60", NFC_Manufacturers.Verisiti_Inc},
    {"61", NFC_Manufacturers.Wearlinks_Technology_Inc},
    {"62", NFC_Manufacturers.Userstar_Information_Systems_Co_Ltd},
    {"63", NFC_Manufacturers.Pragmatic_Printing_Ltd},
    {"64", NFC_Manufacturers.Associacao_do_Laboratorio_de_Sistemas_Integraveis_Tecnologico_LSI_TEC},
    {"65", NFC_Manufacturers.Tendyron_Corporation},
    {"66", NFC_Manufacturers.MUTO_Smart_Co_Ltd},
    {"67", NFC_Manufacturers.ON_Semiconductor},
    {"68", NFC_Manufacturers.TUBITAK_BILGEM},
    {"69", NFC_Manufacturers.Huada_Semiconductor_Co_Ltd},
    {"6A", NFC_Manufacturers.SEVENEY},
    {"6B", NFC_Manufacturers.ISSM},
    {"6C", NFC_Manufacturers.Wisesec_Ltd},
    {"7E", NFC_Manufacturers.Holtek}
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