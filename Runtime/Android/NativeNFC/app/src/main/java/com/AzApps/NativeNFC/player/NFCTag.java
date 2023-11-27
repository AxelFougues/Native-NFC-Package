package com.AzApps.NativeNFC.player;

import java.util.ArrayList;

//Tag info data structure. Mirrored in C#
public class NFCTag {
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

    //From Intent
    public String ID;
    public byte[] atqa;
    public short sak;
    public String emulated = null;
    public ArrayList<Integer> technologies = new ArrayList<Integer>();
    public String manufacturerID;
    public NDEFContents ndefMessage;
    //From tranceives
    public String versionData; //For NTAG chips stores result of GET_VERSION
    public Boolean writable; //tbd
    public int storageSize; //Calculated from versionData or by iterative reading
    public ArrayList<RawContent> rawContents;// Filled in by FAST_READ or iterative reading
    public long scanDuration; //From nfcX.connect to nfcX.close or failure

    //Extrapolated by Unity-NFCManager from collected data
    public String icName;
    public String manufacturerName;
}
