package com.AzApps.NativeNFC.player;

import static com.AzApps.NativeNFC.player.Utils.*;
import android.content.Intent;
import android.nfc.NdefMessage;
import android.nfc.tech.Ndef;
import android.os.AsyncTask;

//Will write an NDEF message to tag
public class NdefWrite {

    protected NFCOperationData operationData;

    public  NdefWrite(TagPair tp, Intent intent, NFCOperationData operationData){
        sendToUnity(AndroidMessagePrefix.SCAN_START, "");
        this.operationData = operationData;
        new NdefWrite.WriteNDEF().execute(tp);
    }

    private class WriteNDEF extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {

            NdefMessage message = operationData.ndefMessage.toAndroidNDEFMessage();

            if(message == null){
                sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "No records to be written.");
                return null;
            }

            long startTime = System.nanoTime();

            TagPair tp = params[0];
            Ndef ndef = Ndef.get(tp.tag);

            if(ndef == null) {
                sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Tag is not NDEF formatted.");
                return tp;
            }

            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Writing message: " + message.getRecords().length + " records");

            try{
                ndef.connect();
                ndef.writeNdefMessage(message);
                ndef.close();
            }catch (Exception e){
                sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Could not write ndef message: " + e.getMessage());
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, "");
        }
    }

}
