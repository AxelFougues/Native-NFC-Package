package com.AzApps.NativeNFC.player;

import static com.AzApps.NativeNFC.player.Utils.*;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.media.AudioDeviceInfo;
import android.media.AudioManager;
import android.nfc.NfcAdapter;
import android.nfc.NfcManager;
import android.nfc.Tag;
import android.nfc.tech.IsoDep;
import android.nfc.tech.MifareClassic;
import android.nfc.tech.MifareUltralight;
import android.nfc.tech.Ndef;
import android.nfc.tech.NdefFormatable;
import android.nfc.tech.NfcA;
import android.nfc.tech.NfcB;
import android.nfc.tech.NfcBarcode;
import android.nfc.tech.NfcF;
import android.nfc.tech.NfcV;
import android.os.Build;
import android.os.Bundle;
import android.view.ContextThemeWrapper;

import com.google.gson.Gson;
import com.unity3d.player.UnityPlayerActivity;

import java.util.ArrayList;
import java.util.List;

public class NativeNFC extends UnityPlayerActivity {
    //Seems unused, need to check before removing it
    static final String TAG = "Unity";
    //True upon initialization, not strictly necessary. Should consider removing if initialization can't fail
    boolean available = false;
    //Set by Unity through toggleNFCIntentCapture, defines what action to do when detecting a tag
    AndroidActionType actionType = AndroidActionType.NONE;
    //Holds payload received by unity for current action, if any
    NFCOperationData actionPayload = null;

    //Stuff needed for NFC intents
    PendingIntent pendingIntent ;
    IntentFilter[] deepIntentFiltersArray;
    IntentFilter[] ndefIntentFiltersArray;
    String[][] techListsArray;
    NfcAdapter nfcAdapter;





    //ACTIVITY LIFECYCLE
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //Toast.makeText(this, "Create", Toast.LENGTH_SHORT).show();
        initializeNFCIntentCapture();
    }

    protected void initializeNFCIntentCapture(){
        NfcManager nfcManager = (NfcManager) getSystemService(Context.NFC_SERVICE);
        nfcAdapter = NfcAdapter.getDefaultAdapter(this);
        if (nfcManager == null) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "NFC Manager not initialized ...");
            return;
        }
        if (nfcAdapter == null) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "NFC Adapter not initialized ...");
            return;
        }
        try{
            pendingIntent = PendingIntent.getActivity(
                    this,
                    0,
                    new Intent(this, NativeNFC.class).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP),
                    PendingIntent.FLAG_MUTABLE
            );
        }catch (Exception e){
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, e.getMessage());
        }

        deepIntentFiltersArray = new IntentFilter[]{new IntentFilter(NfcAdapter.ACTION_TAG_DISCOVERED)};
        ndefIntentFiltersArray = new IntentFilter[]{new IntentFilter(NfcAdapter.ACTION_NDEF_DISCOVERED)};
        techListsArray = new String[][] {
                new String[] { NfcA.class.getName() },
                new String[] { NfcB.class.getName() },
                new String[] { NfcF.class.getName() },
                new String[] { NfcV.class.getName() },
                new String[] { IsoDep.class.getName() },
                new String[] { Ndef.class.getName() },
                new String[] { MifareClassic.class.getName() },
                new String[] { MifareUltralight.class.getName() },
                new String[] { NfcBarcode.class.getName() },
                new String[] { NdefFormatable.class.getName() }
        };
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Android lib initialized.");
        available = true;
    }

    protected  void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Tag detected.");

        Tag tagFromIntent = intent.getParcelableExtra(NfcAdapter.EXTRA_TAG);

        if (tagFromIntent != null) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Tag reading using action: " + actionType.toString());
            TagPair tp = new TagPair(tagFromIntent, nfcTagFromIntentTag(tagFromIntent));

            switch (actionType){
                case DEEP_SCAN:
                    new DeepScan(tp, intent);
                    break;
                case NDEF_SCAN:
                    new NdefScan(tp, intent);
                    break;
                case POWER:
                    new PowerScan(tp);
                    break;
                case NDEF_WRITE:
                    new NdefWrite(tp, intent, actionPayload);
            }

        } else {
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Android failed to scan tag.");
        }
        actionType = AndroidActionType.NONE;

    }
/*
//These don't seem necessary

    public void onPause() {
        super.onPause();
        Toast.makeText(this, "Pause", Toast.LENGTH_SHORT).show();
    }

    public void onResume() {
        super.onResume();
        Toast.makeText(this, "Resume", Toast.LENGTH_SHORT).show();
    }
*/




    //UNITY CALLED METHODS

    public boolean androidAvailable(){
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Native lib available: "+ available);
        return available;
    }

    //NFC

    //Called by Unity to start or end an NFC action
    public void toggleNFCIntentCapture(boolean state, String type){
        actionType = AndroidActionType.valueOf(type);

        if(state && actionType != AndroidActionType.NONE){ //Start an action
            if (nfcAdapter != null) {
                switch (actionType) {
                    case DEEP_SCAN:
                        nfcAdapter.enableForegroundDispatch(this, pendingIntent, deepIntentFiltersArray, techListsArray);
                        break;
                    case NDEF_SCAN:
                        nfcAdapter.enableForegroundDispatch(this, pendingIntent, ndefIntentFiltersArray, techListsArray);
                        break;
                    case POWER:
                        nfcAdapter.enableForegroundDispatch(this, pendingIntent, deepIntentFiltersArray, techListsArray);
                        break;
                }
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan started. "+ AndroidActionType.valueOf(type).toString());
            }else{
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan failed to start. "+ AndroidActionType.valueOf(type).toString());
            }

        }else{ //Stop current action
            PowerScan.powerON = false;
            if (nfcAdapter != null) {
                try {
                    nfcAdapter.disableForegroundDispatch(this);
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan stopped. " + AndroidActionType.valueOf(type).toString());
                } catch (IllegalStateException ex) {
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan failed to stop. " + AndroidActionType.valueOf(type).toString());
                }
            }

        }

    }

    //Overload for actions with an attached payload
    public void toggleNFCIntentCapture(boolean state, String type, String actionPayload){
        actionType = AndroidActionType.valueOf(type);
        Gson gson = new Gson();
        this.actionPayload = gson.fromJson(actionPayload, NFCOperationData.class);
        if(state && actionType != AndroidActionType.NONE){
            if (nfcAdapter != null) {
                switch (actionType) {
                    case NDEF_WRITE:
                        nfcAdapter.enableForegroundDispatch(this, pendingIntent, ndefIntentFiltersArray, techListsArray);
                        break;
                }
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan started. "+ AndroidActionType.valueOf(type).toString());
            }else{
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan failed to start. "+ AndroidActionType.valueOf(type).toString());
            }

        }else{
            PowerScan.powerON = false;
            if (nfcAdapter != null) {
                try {
                    nfcAdapter.disableForegroundDispatch(this);
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan stopped. " + AndroidActionType.valueOf(type).toString());
                } catch (IllegalStateException ex) {
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Scan failed to stop. " + AndroidActionType.valueOf(type).toString());
                }
            }

        }

    }







    //Extra little native functions for ZINC

    //THEME

    public int androidTheme(ContextThemeWrapper ctw) {
        if (Build.VERSION.SDK_INT < 29) {  // Need minimum android Q
            return 0;  // Unspecified
        }

        switch (ctw.getResources().getConfiguration().uiMode & Configuration.UI_MODE_NIGHT_MASK) {
            case Configuration.UI_MODE_NIGHT_YES:
                return 2;
            case Configuration.UI_MODE_NIGHT_NO:
                return 1;
        }

        return 0;  // Unspecified
    }

    //AUDIO

    public String androidAudioOutput(){
        AudioManager audioManager = (AudioManager)getSystemService(Context.AUDIO_SERVICE);
        ArrayList<AudioOutput> outputs = new ArrayList<>();
        if (audioManager != null)
            for (AudioDeviceInfo info : audioManager.getDevices(AudioManager.GET_DEVICES_OUTPUTS)) {
                outputs.add(new AudioOutput(info));
            }
        return new Gson().toJson(new AudioOutputs(outputs));
    }

    public void androidMaxVolume(){
        AudioManager audioManager = (AudioManager)getSystemService(Context.AUDIO_SERVICE);
        final int originalVolume = audioManager.getStreamVolume(AudioManager.STREAM_MUSIC);
        audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, audioManager.getStreamMaxVolume(AudioManager.STREAM_MUSIC), 0);
    }

    //APPS

    public String androidInstalledApps(){
        final PackageManager pm = getPackageManager();
        List<ApplicationInfo> packages = pm.getInstalledApplications(PackageManager.GET_META_DATA);
        InstalledAppsInfo result = new InstalledAppsInfo();
        for (ApplicationInfo packageInfo : packages) {
            result.apps.add(new InstalledAppInfo(String.valueOf(packageInfo.loadLabel(pm)), packageInfo.packageName, packageInfo.loadIcon(pm)));
        }
        return new Gson().toJson(result);
    }

}



