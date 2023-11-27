package com.AzApps.NativeNFC.player;

import static com.AzApps.NativeNFC.player.Utils.*;

import android.nfc.NdefRecord;

//NDEF record data structure. Mirrored in C#
public class NDEFRecord {
    public enum  SimpleRecordType { None, Text, Application, External, Mime, URI }
    public enum TypeNameFormat { EMPTY, WELL_KNOWN, MIME, ABSOLUTE_URI, EXTERNAL, UNKOWN }
    public enum WellKnownRecordType { None, Text, URI, Smart_Poster, Alternative_Carrier, Handover_Carrier, Handover_Request, Handover_Select }
    public enum LanguageCode { en }
    public enum TextEncoding { UTF8, UTF16 }

    public NDEFRecord(){ }

    public NDEFRecord(NdefRecord androidRecord){
        this.tnf = androidRecord.getTnf();
        this.recordType = bytesToIntArray(androidRecord.getType());
        this.recordID = bytesToIntArray(androidRecord.getId());
        this.payload = bytesToIntArray(androidRecord.getPayload());
    }

    public int tnf = 0;
    public int[] recordType = null;
    public int[] payload = null;
    public int[] recordID = null;
    //Parsed values
    public int simpleRecordType = 0;
    public String text;
    public String languageCode;
    public TextEncoding textEncoding;
    public String uri;
    public String mimeType;
    public String packageName;
    public String domain;
    public String domainDataType;


    public NdefRecord toAndroidRecord(){
        if(SimpleRecordType.values()[simpleRecordType] != SimpleRecordType.None) return createAndroidRecordSimple();
        try{
        switch (TypeNameFormat.values()[tnf]){
            case EMPTY:
                return new NdefRecord(NdefRecord.TNF_EMPTY, null, null, null);
            case WELL_KNOWN:
                return new NdefRecord(NdefRecord.TNF_WELL_KNOWN, intsToByteArray(recordType), intsToByteArray(recordID), intsToByteArray(payload));
            case MIME:
                return new NdefRecord(NdefRecord.TNF_MIME_MEDIA, intsToByteArray(recordType), intsToByteArray(recordID), intsToByteArray(payload));
            case ABSOLUTE_URI:
                return new NdefRecord(NdefRecord.TNF_ABSOLUTE_URI, intsToByteArray(recordType), intsToByteArray(recordID), intsToByteArray(payload));
            case EXTERNAL:
                return new NdefRecord(NdefRecord.TNF_EXTERNAL_TYPE, intsToByteArray(recordType), intsToByteArray(recordID), intsToByteArray(payload));
            case UNKOWN:
                return new NdefRecord(NdefRecord.TNF_UNKNOWN, null, intsToByteArray(recordID), intsToByteArray(payload));
        }
        }catch (IllegalArgumentException e){
            sendToUnity(Utils.AndroidMessagePrefix.DEBUG_LOG, "Ndef creation failed");
        }
        return null;
    }

    public NdefRecord createAndroidRecordSimple(){
        switch (SimpleRecordType.values()[simpleRecordType]){
            case Text:
                return NdefRecord.createTextRecord(null, text);
            case Application:
                return NdefRecord.createApplicationRecord(packageName);
            case External:
                return NdefRecord.createExternal(domain, domainDataType, intsToByteArray(payload));
            case Mime:
                return NdefRecord.createMime(mimeType, intsToByteArray(payload));
            case URI:
                return NdefRecord.createUri(uri);
        }
        return null;
    }

}
