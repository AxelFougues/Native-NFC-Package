using System;
using System.Collections.Generic;
using UnityEngine;

namespace AzApps.NativeNFC {

    [Serializable]
    public class NFCTag {



        public enum NFCTechnology {
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



        //Handled by the android lib
        //from intent
        public string ID = null;
        public byte[] atqa;
        public short sak;
        public string emulated;
        public List<NFCTechnology> technologies = new List<NFCTechnology>();
        public string manufacturer = null;

        //from tranceives
        public long scanDuration;
        public string versionData = null;
        public bool writable;
        public int storageSize;
        public NDEFContents ndefMessage;
        public List<RawContent> rawContents;

        //Extrapolated by NFCManager
        public string icName = null; //Match readable name




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