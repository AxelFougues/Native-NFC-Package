using System;
using System.Collections.Generic;
using static AbyssWalkerDev.NativeNFC.NFCUtils;

namespace AbyssWalkerDev.NativeNFC {

    [Serializable]
    public class NFCTag {

        public enum OperationType { DEEP_SCAN, NDEF_SCAN, NDEF_WRITE, POWER, NONE }
        public enum Status { SUCCESS, FAILED, PARTIAL, NONE }

        public enum NFC_Technology {
            UNKNOWN,
            ISO_DEP,
            MIFARE_CLASSIC,
            MIFARE_ULTRALIGHT,
            NDEF,
            NDEF_FORMATABLE,
            NFC_A,
            NFC_B,
            NFC_BARCODE,
            NFC_F,
            NFC_V
        }
        public enum MifareClassicType {UNKNOWN = -1,CLASSIC = 0, PLUS = 1, PRO = 2 }
        public enum MifareUltralightType { UNKNOWN = -1, ULTRALIGHT = 0, ULTRALIGHT_C = 1 }
        public enum BarcodeType { UNKNOWN = -1, KOVIO = 1 }

        //Handled by the android lib

        public Operation lastOperation = null;
        //from intent
        public string ID = null;
        public List<NFC_Technology> technologies = new List<NFC_Technology>();
        //from dump
        public string versionData = null;
        public string manufacturerID = null;
        public string emulated = null;
        public int storageSize = 0;
        public List<RawContent> rawContents = new List<RawContent>();
        //NfcA
        public byte[] atqa = null;
        public short sak = 0;
        public int transceiveTimeout = 0;
        public int transceiveMaxLength = 0;
        //NDEF
        public string ndefType = null;
        public bool ndefWritable = false;
        public int ndefMaxSize = 0;
        public NDEFContent ndefMessage = null;
        //Barcode
        public byte[] barcode = null;
        public BarcodeType barcodeType = BarcodeType.UNKNOWN;
        //MifareUltralight
        public MifareUltralightType muType = MifareUltralightType.UNKNOWN;
        //MifareClassic
        public MifareClassicType mcSize = MifareClassicType.UNKNOWN;
        //NfcB
        public byte[] protocolInfo = null;
        public byte[] appData = null;
        //IsoDep
        public byte[] hiLayerResponse = null;
        public byte[] historicalBytes = null;
        public bool isExtendedLengthApduSupported = false;
        //NfcF
        public byte[] manufacturer = null;
        public byte[] systemCode = null;
        //NfcV
        public byte dsfId = 0;
        public byte responseFlags = 0;


        //Extrapolated by NFCManager
        public NFC_IC icName = NFC_IC.unknown;
        public NFC_Manufacturers manufacturerName = NFC_Manufacturers.unknown;



        public string getICToString() {
            return icName.ToString().Replace("_", " ").Trim();
        }
        public string getManufacturerToString() {
            return manufacturerID.ToUpper() + " - " + manufacturerName.ToString().Replace("_", " ").Trim();
        }


    }

    [Serializable]
    public class RawContent {
        public int sector;
        public List<RawBlockContent> content = new List<RawBlockContent>();

        public override string ToString() {
            string result = "Sector " + sector + ":" + content.Count + " Blocks\n";
            foreach (RawBlockContent rbc in content) {
                result += rbc.ToString() + "\n";
            }
            return result;
        }

        [Serializable]
        public class RawBlockContent {

            public int blockIndex = 0;
            public byte[] blockContent;
            public short[] byteTypes;
            public bool writable = true;
            public bool readable = true;
            public bool locked = false;
            public bool blocked = false;

            public override string ToString() {
                string hexIndex = $"0x{blockIndex:X4}";
                if (blocked || locked) return "Block [" + hexIndex + "] " + "locked or blocked";
                else if (blockContent == null || blockContent.Length == 0) return "Block [" + hexIndex + "] " + "N/A";
                else return "Block [" + hexIndex + "] " + BitConverter.ToString(blockContent) + " |" + NFCUtils.bytesToText(blockContent) + "|";
            }

        }
    }

    
}