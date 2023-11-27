package com.AzApps.NativeNFC.player;

import static com.AzApps.NativeNFC.player.NativeNFC.*;

import com.google.gson.Gson;

import android.content.Intent;
import android.nfc.NdefMessage;
import android.nfc.NdefRecord;
import android.nfc.tech.Ndef;
import android.nfc.tech.NfcA;
import android.os.AsyncTask;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;

import static com.AzApps.NativeNFC.player.Utils.*;

//Will retrieve tag's NDEF message
public class NdefScan {

    public NdefScan(TagPair tp, Intent intent){
        sendToUnity(AndroidMessagePrefix.SCAN_START, "");
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Starting NDEF scan");
        new NdefScan.ReadNDEF().execute(tp);
    }

    private class ReadNDEF extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {

            long startTime = System.nanoTime();

            TagPair tp = params[0];
            Ndef ndef = Ndef.get(tp.tag);
            if(ndef == null) {
                onFail("Tag is not NDEF formatted.");
                return tp;
            }
            try{
                ndef.connect();
                NdefMessage message = ndef.getNdefMessage();
                tp.nfcTag.ndefMessage = parseNdefMessage(message);
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "NDEF parsed. Records: " + message.getRecords().length + " -> " + tp.nfcTag.ndefMessage.records.size());

                ndef.close();
            }catch (Exception e){
                onFail(e);
                return tp;
            }

            tp.nfcTag.scanDuration = (System.nanoTime() - startTime) / 1000000L;

            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, progress[0]);
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            super.onPostExecute(tp);
            if(!isCancelled()) sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

        void onFail(Exception e){
            this.cancel(true);
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, e.getMessage());
        }

        void onFail(String message){
            this.cancel(true);
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, message);
        }
    }

    NDEFContents parseNdefMessage(NdefMessage message){
        NDEFContents parsedContents = new NDEFContents();
        parsedContents.Success = true;
        if(message == null) {
            parsedContents.Error = NDEFContents.NDEFReadError.NO_RECORDS;
            parsedContents.Success = false;
            return  parsedContents;
        }

        NdefRecord[] records = message.getRecords();
        for (NdefRecord record : records) {
            parsedContents.records.add(new NDEFRecord(record));
        }

        return parsedContents;
    }

}
