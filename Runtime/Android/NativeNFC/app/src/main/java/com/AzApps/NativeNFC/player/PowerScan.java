package com.AzApps.NativeNFC.player;

import com.google.gson.Gson;

import android.nfc.tech.NfcA;
import android.nfc.tech.NfcB;
import android.nfc.tech.NfcBarcode;
import android.nfc.tech.NfcF;
import android.nfc.tech.NfcV;
import android.os.AsyncTask;

import static com.AzApps.NativeNFC.player.Utils.*;

//Upon detecting a tag will lock a thread in a loop of constant NFC command emission, Until action is ended by Unity
public class PowerScan {

    public static boolean powerON = false;
    public static int maxDurationSeconds = 60;
    public static boolean doLimit = true;

    public PowerScan(TagPair tp){
        sendToUnity(AndroidMessagePrefix.SCAN_START, "");
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Starting power scan");

        powerON = false;

        String[] techs = tp.tag.getTechList();

        for(int i = 0; i < techs.length; i++){
            if(techs[i] == NfcA.class.getName()){
                new ReadNfcA().execute(tp);
                return;
            }
            if(techs[i] == NfcB.class.getName()){
                new ReadNfcB().execute(tp);
                return;
            }
            if(techs[i] == NfcF.class.getName()){
                new ReadNfcF().execute(tp);
                return;
            }
            if(techs[i] == NfcV.class.getName()){
                new ReadNfcV().execute(tp);
                return;
            }

            if(techs[i] == NfcBarcode.class.getName()){
                new ReadNfcBarcode().execute(tp);
                return;
            }
        }
        //Unknown or no tech
        String s = "";
        for ( Integer i: tp.nfcTag.technologies) {
            s += " " + i.toString();
        }
        if(s.equals("")) s = "No tech detected.";
        sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Unknown tag tech." + s);

    }


    private class ReadNfcA extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Power Scanning NfcA");
            TagPair tp = params[0];
            NfcA nfcA = NfcA.get(tp.tag);
            powerON = true;
            try{
                nfcA.connect();
                long startTime = System.currentTimeMillis();
                while (powerON){ //&& (!doLimit || (System.currentTimeMillis() - startTime) < maxDurationSeconds * 1000L)) {
                    sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, "");
                    byte block = Byte.parseByte(String.valueOf(1));
                    try {
                        byte[] response = nfcA.transceive(new byte[]{
                                (byte) 0x30,  // READ
                                block,  // block
                        });
                    }catch (Exception e){
                        //powerON = false;
                    }
                }
                nfcA.close();
            }catch (Exception e){

            }
            powerON = false;
            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            //never called
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            sendToUnity(AndroidMessagePrefix.SCAN_END, "");
            super.onPostExecute(tp);
        }
    }

    private class ReadNfcB extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Power Scanning NfcB");
            TagPair tp = params[0];
            NfcB nfcB = NfcB.get(tp.tag);
            powerON = true;
            try{
                nfcB.connect();
                long startTime = System.currentTimeMillis();
                while (powerON){ // && (!doLimit || (System.currentTimeMillis() - startTime) < maxDurationSeconds * 1000L)) {
                    sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, "");
                    byte block = Byte.parseByte(String.valueOf(1));
                    try {
                        byte[] response = nfcB.transceive(new byte[]{
                                (byte) 0x30,  // READ
                                block,  // block
                        });
                    }catch (Exception e){
                        //powerON = false;
                    }
                }
                nfcB.close();
            }catch (Exception e){

            }
            powerON = false;
            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            //never called
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            sendToUnity(AndroidMessagePrefix.SCAN_END, "");
            super.onPostExecute(tp);
        }
    }

    private class ReadNfcF extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Power Scanning NfcF");
            TagPair tp = params[0];
            NfcF nfcF = NfcF.get(tp.tag);
            powerON = true;
            try{
                nfcF.connect();
                long startTime = System.currentTimeMillis();
                while (powerON){ // && (!doLimit || (System.currentTimeMillis() - startTime) < maxDurationSeconds * 1000L)) {
                    sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, "");
                    byte block = Byte.parseByte(String.valueOf(1));
                    try {
                        byte[] response = nfcF.transceive(new byte[]{
                                (byte) 0x30,  // READ
                                block,  // block
                        });
                    }catch (Exception e){
                        //powerON = false;
                    }
                }
                nfcF.close();
            }catch (Exception e){

            }
            powerON = false;
            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            //never called
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            sendToUnity(AndroidMessagePrefix.SCAN_END, "");
            super.onPostExecute(tp);
        }
    }

    private class ReadNfcV extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Power Scanning NfcV");
            TagPair tp = params[0];
            NfcV nfcV = NfcV.get(tp.tag);
            powerON = true;
            try{
                nfcV.connect();
                long startTime = System.currentTimeMillis();
                while (powerON){ // && (!doLimit || (System.currentTimeMillis() - startTime) < maxDurationSeconds * 1000L)) {
                    sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, "");
                    byte block = Byte.parseByte(String.valueOf(1));
                    try {
                        byte[] response = nfcV.transceive(new byte[]{
                                (byte) 0x30,  // READ
                                block,  // block
                        });
                    }catch (Exception e){
                        //powerON = false;
                    }
                }
                nfcV.close();
            }catch (Exception e){

            }
            powerON = false;
            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            //never called
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            sendToUnity(AndroidMessagePrefix.SCAN_END, "");
            super.onPostExecute(tp);
        }
    }

    private class ReadNfcBarcode extends AsyncTask<TagPair, String, TagPair> {

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Power Scanning NfcBarcode");
            TagPair tp = params[0];
            NfcBarcode nfcBarcode = NfcBarcode.get(tp.tag);
            powerON = true;
            try{
                nfcBarcode.connect();
                long startTime = System.currentTimeMillis();
                while (powerON){ // && (!doLimit || (System.currentTimeMillis() - startTime) < maxDurationSeconds * 1000L)) {
                    sendToUnity(AndroidMessagePrefix.SCAN_PROGRESS, "");
                    byte block = Byte.parseByte(String.valueOf(1));
                    try {
                        //byte[] response = nfcBarcode.transceive(new byte[]{
                                //(byte) 0x30,  // READ
                                //block,  // block
                        //});
                    }catch (Exception e){
                        //powerON = false;
                    }
                }
                nfcBarcode.close();
            }catch (Exception e){

            }
            powerON = false;
            return tp;
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            //never called
        }

        @Override
        protected void onPostExecute(TagPair tp) {
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
            super.onPostExecute(tp);
        }
    }

}
