using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NFCUtils;


public enum NDEFReadError { NONE, HAS_UNREADABLE_RECORDS, UNKNOWN, USER_CANCEL, TIMEOUT, NO_RECORDS }

[Serializable]
public class NDEFContents {
    public bool Success;
    public NDEFReadError Error;
    public List<NDEFRecord> records = new List<NDEFRecord>();

    public void trySimplify() {
        foreach (NDEFRecord record in records) record.trySimplify();
    }
}





[Serializable]
public class NDEFRecord {

    public enum SimpleRecordType { None, Text, Application, External, Mime, URI }
    public enum TypeNameFormat { EMPTY, WELL_KNOWN, MIME, ABSOLUTE_URI, EXTERNAL, UNKOWN }
    public enum LanguageCode { en }
    public enum TextEncoding { UTF8, UTF16 }


    public void trySimplify() {
        //Well known
        if (tnf == TypeNameFormat.WELL_KNOWN) {
            //Text
            if (intsToBytes(recordType).SequenceEqual(WELL_KNOWN_RECORD_TYPE_VALUES[WellKnownRecordType.Text])) {
                byte[] payloadBytes = intsToBytes(payload);
                textEncoding = ((payloadBytes[0] & 0x080) == 0) ? TextEncoding.UTF8 : TextEncoding.UTF16;
                int languageLength = payloadBytes[0] & 0x03F;
                int textLength = payloadBytes.Length - 1 - languageLength;
                byte[] languageBytes = new byte[languageLength];
                Array.Copy(payloadBytes, 1, languageBytes, 0, languageLength);
                languageCode = bytesToText(languageBytes);
                byte[] textBytes = new byte[textLength];
                Array.Copy(payloadBytes, languageLength + 1, textBytes, 0, textLength);
                text = bytesToText(textBytes);
                simpleRecordType = SimpleRecordType.Text;
                return;
            }
            //URI
            if (intsToBytes(recordType).SequenceEqual(WELL_KNOWN_RECORD_TYPE_VALUES[WellKnownRecordType.URI])) {
                byte[] payloadBytes = intsToBytes(payload);

                byte identifierByte = payloadBytes[0];

                int uriLength = payloadBytes.Length - 1;
                byte[] uriBytes = new byte[uriLength];
                Array.Copy(payloadBytes, 1, uriBytes, 0, uriLength);

                string identifierText = "";
                if (URI_IDENTIFIER_CODES.ContainsKey(identifierByte)) identifierText = URI_IDENTIFIER_CODES[identifierByte];

                uri = identifierText + bytesToText(uriBytes);
                simpleRecordType = SimpleRecordType.URI;
                return;
            }
        }
        //External
        if (tnf == TypeNameFormat.EXTERNAL) {
            //Shortcut
            if (intsToBytes(recordType).SequenceEqual(EXTERNAL_RECORD_TYPE_VALUES[ExternalRecordType.Android_Package])) {
                byte[] payloadBytes = intsToBytes(payload);
                packageName = bytesToText(payloadBytes);
                simpleRecordType = SimpleRecordType.Application;
                return;
            }

        }
    }

    public TypeNameFormat tnf = TypeNameFormat.EMPTY;
    public int[] recordType = null;
    public int[] payload = null;
    public int[] recordID = null;

    //Simplified values
    public SimpleRecordType simpleRecordType = SimpleRecordType.None;
    public string text;
    public string languageCode;
    public TextEncoding textEncoding;
    public string uri;
    public string mimeType;
    public string packageName;
    public string domain;
    public string domainDataType;
}