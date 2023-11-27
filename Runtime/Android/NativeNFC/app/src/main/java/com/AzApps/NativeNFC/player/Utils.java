package com.AzApps.NativeNFC.player;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.drawable.Drawable;
import android.media.AudioDeviceInfo;
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

import com.unity3d.player.UnityPlayer;

import java.io.ByteArrayOutputStream;
import java.util.ArrayList;



public class Utils {

    public enum AndroidMessagePrefix {
        DEBUG_LOG, DEBUG_ERROR, SCAN_START, SCAN_PROGRESS, SCAN_END, SCAN_FAIL
    }

    public enum AndroidActionType {
        DEEP_SCAN, NDEF_SCAN, NDEF_WRITE, POWER, NONE
    }

    public static class TagPair{
        Tag tag;
        NFCTag nfcTag;
        public TagPair(Tag tag, NFCTag nfcTag) {
            this.tag = tag;
            this.nfcTag = nfcTag;
        }
    }

    public static class AudioOutput{
        public int deviceType;
        public String address;
        public int id;
        public String deviceName;
        public AudioOutput(AudioDeviceInfo info){
            deviceName = info.getProductName().toString();
            deviceType = info.getType();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                address = info.getAddress();
            }
            id = info.getId();
        }
    }
    public static class AudioOutputs{
        public ArrayList<AudioOutput> outputs = new ArrayList<>();
        public AudioOutputs(ArrayList<AudioOutput> outputs){
            this.outputs = new ArrayList<>(outputs);
        }
    }

    public static class InstalledAppsInfo{
        public ArrayList<InstalledAppInfo> apps = new ArrayList<>();
    }

    public static class InstalledAppInfo{
        public static class InstalledAppIcon{
            public int[] iconData;
            public InstalledAppIcon(int[] iconData){ this.iconData = iconData;}
        }
        public String packageName;
        public String readableName;
        public InstalledAppIcon icon;

        public InstalledAppInfo(String readableName, String packageName, Drawable icon){
            this.readableName = readableName;
            this.packageName = packageName;
            Bitmap bitmap = getBitmapFromDrawable(icon);
            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            bitmap.compress(Bitmap.CompressFormat.JPEG, 100, stream);
            byte[] iconData = stream.toByteArray();
            this.icon = new InstalledAppIcon(bytesToIntArray(iconData));
        }
    }




    //Utilities

    public static NFCTag nfcTagFromIntentTag(Tag tag){
        NFCTag nfcTag = new NFCTag();

        byte[] id = tag.getId();
        nfcTag.ID = bytesToHex(id);


        if(id.length == 7){
            nfcTag.manufacturerID = byteToHex(id[0]);
        }

        for (String t : tag.getTechList()) {
            if(t.equals(NfcA.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NFC_A.ordinal());
            if(t.equals(NfcB.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NFC_B.ordinal());
            if(t.equals(NfcF.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NFC_F.ordinal());
            if(t.equals(NfcV.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NFC_V.ordinal());
            if(t.equals(MifareClassic.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.MIFARE_CLASSIC.ordinal());
            if(t.equals(MifareUltralight.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.MIFARE_ULTRALIGHT.ordinal());
            if(t.equals(NfcBarcode.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NFC_BARCODE.ordinal());
            if(t.equals(Ndef.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NDEF.ordinal());
            if(t.equals(NdefFormatable.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.NDEF_FORMATABLE.ordinal());
            if(t.equals(IsoDep.class.getName())) nfcTag.technologies.add(NFCTag.NFC_Technology.ISO_DEP.ordinal());
        }
        return nfcTag;
    }

    public static final char[] HEX_ARRAY = "0123456789ABCDEF".toCharArray();

    public static String bytesToHex(byte[] bytes) {
        char[] hexChars = new char[bytes.length * 2];
        for (int j = 0; j < bytes.length; j++) {
            int v = bytes[j] & 0xFF;
            hexChars[j * 2] = HEX_ARRAY[v >>> 4];
            hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
        }
        return new String(hexChars).toUpperCase();
    }

    public static String byteToHex(byte data) {
        StringBuilder sb = new StringBuilder();
        sb.append(String.format("%02X", data));
        return sb.toString().toUpperCase();
    }

    public static int[] bytesToIntArray(byte[] bytes){
        int[] integers = new int[bytes.length];
        for (int i = 0; i < bytes.length; i++) {
            integers[i] = Byte.toUnsignedInt(bytes[i]);
        }
        return  integers;
    }

    public static byte[] intsToByteArray(int[] ints){
        byte[] bytes = new byte[ints.length];
        for (int i = 0; i < ints.length; i++) {
            bytes[i] = (byte)ints[i];
        }
        return bytes;
    }

    public static boolean isByteBitSet(byte value, int n) {
        return (value & (1<<n)) != 0;
    }

    public static byte setByteBit(byte b, int n, boolean value) {
        if(value) b |= 1 << n;
        else b &= ~(1 << n);
        return b;
    }

    public static Bitmap getBitmapFromDrawable(Drawable drawable) {
        final Bitmap bmp = Bitmap.createBitmap(drawable.getIntrinsicWidth(), drawable.getIntrinsicHeight(), Bitmap.Config.ARGB_8888);
        final Canvas canvas = new Canvas(bmp);
        drawable.setBounds(0, 0, canvas.getWidth(), canvas.getHeight());
        drawable.draw(canvas);
        return bmp;
    }

    public static void sendToUnity(AndroidMessagePrefix prefix, String msg){
        UnityPlayer.UnitySendMessage("NativeNFC", "messageFromAndroid", prefix.toString() + "#" + msg);
    }

}
