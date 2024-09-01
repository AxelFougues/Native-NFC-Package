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

        public override string ToString() {
            return ToStringIndented(0, "");
        }

        public string ToStringIndented(int level, string character) {
            string indentation = "";
            for (int i = 0; i < level; i++) indentation += character;
            string output = indentation + "---Tag---\n";
            output += indentation + "ID: " + ID + "\n";
            output += indentation + "IC name: " + icName + "\n";
            output += indentation + "Manufacturer name: " + manufacturerName + "\n";
            output += indentation + "Technologies:";
            if (technologies!= null && technologies.Count > 0) foreach (NFC_Technology technology in technologies) output += indentation + "- " + technology + "\n";
            else output += indentation + "- None\n";
            output += indentation + "Version data: " + versionData + "\n";
            output += indentation + "Manufacturer ID: " + manufacturerID + "\n";
            output += indentation + "Emulated: " + emulated + "\n";
            output += indentation + "Storage size: " + storageSize + "\n";
            output += indentation + "Raw contents:\n";
            if (rawContents != null && rawContents.Count > 0) foreach (RawContent rc in rawContents) output += rc.ToStringIndented(++level, character);
            else output += indentation + "- None\n";

            if (technologies == null || technologies.Contains(NFC_Technology.NDEF)) {
                output += indentation + "NDEF type: " + ndefType + "\n";
                output += indentation + "NDEF writable: " + ndefWritable + "\n";
                output += indentation + "NDEF max size: " + ndefMaxSize + "\n";
                if (ndefMessage != null) output += ndefMessage.ToStringIndented(++level, character);
            }

            if (technologies == null || technologies.Contains(NFC_Technology.NFC_A)) {
                output += indentation + "ATQA: " + (atqa != null? NFCUtils.bytesToHexString(atqa) : "null") + "\n";
                output += indentation + "SAK: " + sak + "\n";
                output += indentation + "Transceive timeout: " + transceiveTimeout + "\n";
                output += indentation + "Transceive max length: " + transceiveMaxLength + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.NFC_BARCODE)) {
                output += indentation + "Barcode: " + (barcode != null ? NFCUtils.bytesToHexString(barcode) : null) + "\n";
                output += indentation + "Barcode type: " + barcodeType + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.MIFARE_ULTRALIGHT)) {
                output += indentation + "Mifare Ultralight type: " + muType + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.MIFARE_CLASSIC)) {
                output += indentation + "Mifare Classic size: " + mcSize + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.NFC_B)) {
                output += indentation + "ProtocolInfo: " + (protocolInfo != null ? NFCUtils.bytesToHexString(protocolInfo) : null) + "\n";
                output += indentation + "App data: " + (appData != null ? NFCUtils.bytesToHexString(appData) : null) + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.ISO_DEP)) {
                output += indentation + "HI Layer Response: " + (hiLayerResponse != null ? NFCUtils.bytesToHexString(hiLayerResponse) : null) + "\n";
                output += indentation + "Historical bytes: " + (historicalBytes != null ? NFCUtils.bytesToHexString(historicalBytes) : null) + "\n";
                output += indentation + "Extended length Apdu support: " + isExtendedLengthApduSupported + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.NFC_F)) {
                output += indentation + "Manufacturer: " + (manufacturer != null ? NFCUtils.bytesToHexString(manufacturer) : null) + "\n";
                output += indentation + "System code: " + (systemCode != null ? NFCUtils.bytesToHexString(systemCode) : null) + "\n";
            }
            if (technologies == null || technologies.Contains(NFC_Technology.NFC_V)) {
                output += indentation + "DSF ID: " + NFCUtils.byteToBinaryString(dsfId) + "\n";
                output += indentation + "Response flags: " + NFCUtils.byteToBinaryString(responseFlags) + "\n";
            }
            return output;
        }

    }

    [Serializable]
    public class RawContent {
        public int sector;
        public List<RawBlockContent> content = new List<RawBlockContent>();

        public override string ToString() {
            return ToStringIndented(0, "");
        }

        public string ToStringIndented(int level, string character) {
            string indentation = "";
            for (int i = 0; i < level; i++) indentation += character;
            string output = indentation + "---Raw content---\n";
            output += indentation += "Sector " + sector + ":" + (content != null? content.Count.ToString() : "0") + " Blocks\n";
            output += indentation + "Raw block contents:\n";
            if (content != null && content.Count > 0) foreach (RawBlockContent rbc in content) output += rbc.ToStringIndented(++level, " ") + "\n";
            else output += indentation + "- None\n";
            return output;
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
                return ToStringIndented(0, "");
            }

            public string ToStringIndented(int level, string character) {
                string indentation = "";
                for (int i = 0; i < level; i++) indentation += character;
                string hexIndex = $"0x{blockIndex:X4}";
                if (blocked || locked) return indentation + "Block [" + hexIndex + "] " + "locked or blocked\n";
                else if (blockContent == null || blockContent.Length == 0) return indentation + "Block [" + hexIndex + "] " + "N/A\n";
                else return indentation + "Block [" + hexIndex + "] " + BitConverter.ToString(blockContent) + " |" + NFCUtils.bytesToText(blockContent) + "|\n";
            }

        }
    }

    
}