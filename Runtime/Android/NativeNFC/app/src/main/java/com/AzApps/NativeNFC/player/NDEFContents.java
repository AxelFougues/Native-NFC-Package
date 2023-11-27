package com.AzApps.NativeNFC.player;

import android.nfc.NdefMessage;
import android.nfc.NdefRecord;

import java.util.ArrayList;

//NDEF message data structure. Mirrored in C#
public class NDEFContents {
    public enum NDEFReadError {NONE, HAS_UNREADABLE_RECORDS, UNKNOWN, USER_CANCEL, TIMEOUT, NO_RECORDS}

    public boolean Success = false;
    public NDEFReadError Error = NDEFReadError.NONE;
    public ArrayList<NDEFRecord> records = new ArrayList<>();

    public NdefMessage toAndroidNDEFMessage() {
        ArrayList<NdefRecord> androidRecords = new ArrayList<>();
        for (NDEFRecord record : records) {
            NdefRecord androidRecord = record.toAndroidRecord();
            if(androidRecord != null) androidRecords.add(androidRecord);
        }
        try {
            return new NdefMessage(androidRecords.toArray(new NdefRecord[0]));
        }catch (Exception e){
            return null;
        }
    }
}
