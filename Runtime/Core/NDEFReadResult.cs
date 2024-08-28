using System;
using System.Collections.Generic;
using System.Linq;
using static AbyssWalkerDev.NativeNFC.NFCUtils;

namespace AbyssWalkerDev.NativeNFC {

    public enum NDEFReadError { NONE, HAS_UNREADABLE_RECORDS, UNKNOWN, USER_CANCEL, TIMEOUT, NO_RECORDS }

    [Serializable]
    public class NDEFContent {
        public bool success;
        public NDEFReadError error;
        public List<NDEFRecord> records = new List<NDEFRecord>();

        public void trySimplify() {
            foreach (NDEFRecord record in records) record.trySimplify();
        }

        public override string ToString() {
            return ToStringIndented(0, "");
        }

        public string ToStringIndented(int level, string character) {
            string indentation = "";
            for (int i = 0; i < level; i++) indentation += character;
            string output = indentation + "---NDEF Record---\n";
            output += indentation + "Success: " + success + "\n";
            output += indentation + "Earror: " + error + "\n";
            output += indentation + "Records:\n";
            if (records.Count > 0) foreach (NDEFRecord record in records) output += record.ToStringIndented(++level, character) + "\n";
            else output += indentation + "- None\n";
            return output;
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

            public override string ToString() {
                return ToStringIndented(0, "");
            }

            public string ToStringIndented(int level, string character) {
                string indentation = "";
                for (int i = 0; i < level; i++) indentation += character;
                string output = indentation + "---NDEF Record---\n";
                output += indentation + "Simple record type: " + simpleRecordType + "\n";
                output += indentation + "TNF: " + tnf + "\n";
                output += indentation + "Record type: " + (recordType == null? "null" : string.Join(",", recordType)) + "\n";
                output += indentation + "Payload: " + (payload == null ? "null" : string.Join(",", payload)) + "\n";
                output += indentation + "Record ID: " + (recordID == null ? "null" : string.Join(",", recordID)) + "\n";
                output += indentation + "Text: " + text + "\n";
                output += indentation + "Language code: " + languageCode + "\n";
                output += indentation + "Text encoding: " + textEncoding + "\n";
                output += indentation + "URI: " + uri + "\n";
                output += indentation + "Mime type: " + mimeType + "\n";
                output += indentation + "Package name: " + packageName + "\n";
                output += indentation + "Domain: " + domain + "\n";
                output += indentation + "Domain data type: " + domainDataType + "\n";
                return output;
            }
        }
    }





    
}