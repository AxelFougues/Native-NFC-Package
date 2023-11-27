package com.AzApps.NativeNFC.player;

import com.google.gson.Gson;

import android.content.Intent;
import android.nfc.tech.IsoDep;
import android.nfc.tech.MifareClassic;
import android.nfc.tech.MifareUltralight;
import android.nfc.tech.NfcA;
import android.nfc.tech.NfcB;
import android.nfc.tech.NfcBarcode;
import android.nfc.tech.NfcF;
import android.nfc.tech.NfcV;
import android.os.AsyncTask;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import static com.AzApps.NativeNFC.player.Utils.*;

//Proceeds to retrieve as much information as possible from any tag
public class DeepScan {

    static List<String> NFCTechnologiesOrdered = Arrays.asList(
        "UNKNOWN",
        "ISO_DEP",
        "MIFARE_CLASSIC",
        "MIFARE_ULTRALIGHT",
        "NDEF",
        "NDEF_FORMATABLE",
        "NFC_A",
        "NFC_B",
        "NFC_BARCODE",
        "NFC_F",
        "NFC_V"
    );

    static boolean byteArrayIsZeros(final byte[] array) {
        for (byte b : array) {
            if (b != 0) {
                return false;
            }
        }
        return true;
    }

    static boolean intArrayIsZeros(final int[] array) {
        for (int i : array) {
            if (i != 0) {
                return false;
            }
        }
        return true;
    }




    public DeepScan(TagPair tp, Intent intent){
        sendToUnity(AndroidMessagePrefix.SCAN_START, "");
        sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Starting deep scan");

        if (tp.nfcTag.technologies.contains(1)) new ReadIsoDep().execute(tp);
        else if (tp.nfcTag.technologies.contains(2)) new ReadMifareClassic().execute(tp);
        else if (tp.nfcTag.technologies.contains(3)) new ReadMifareUltralight().execute(tp);
        else if (tp.nfcTag.technologies.contains(6)) new ReadNfcA().execute(tp);
        else if (tp.nfcTag.technologies.contains(7)) new ReadNfcB().execute(tp);
        else if (tp.nfcTag.technologies.contains(8)) new ReadNfcBarcode().execute(tp);
        else if (tp.nfcTag.technologies.contains(9)) new ReadNfcF().execute(tp);
        else if (tp.nfcTag.technologies.contains(10)) new ReadNfcV().execute(tp);
        else{
            String s = "";
            for ( Integer i: tp.nfcTag.technologies) {
                s += " " + i.toString();
            }
            if(s.equals("")) s = "No tech detected.";
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Unknown tag tech." + s);
        }
    }


    private static class ReadNfcA extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static  NfcA nfcA;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcA, not yet implemented");
            tp = params[0];
            nfcA = NfcA.get(tp.tag);
            tp.nfcTag.sak = nfcA.getSak();
            tp.nfcTag.atqa = nfcA.getAtqa();
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }

    //NfcA
    private static class ReadMifareUltralight extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static  MifareUltralight nfcMU;
        static  NfcA nfcA;
        int currentSector = 0;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcMU");
            tp = params[0];
            tp.nfcTag.rawContents = new ArrayList<>();
            nfcMU = MifareUltralight.get(tp.tag);
            nfcA = NfcA.get(tp.tag);

            tp.nfcTag.sak = nfcA.getSak();
            tp.nfcTag.atqa = nfcA.getAtqa();

            long startTime = System.nanoTime();

            try{
                nfcMU.connect();
                TagPattern pattern = getPattern();
                if(pattern != null && pattern.memoryMap != null){
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Pattern read");
                    readContentFromPattern(pattern);
                }else {
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Recursive read");
                    readContentRecursivelyOptimized(0, null);
                }
                nfcMU.close();

            }catch (Exception e){
                onFail(e);
            }

            tp.nfcTag.scanDuration = (System.nanoTime() - startTime) / 1000000L;
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcMU done");
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
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Deep scan (MU) failed: "+ e.getMessage());
            this.cancel(true);
        }


        boolean readContentRecursivelyOptimized(int i, RawContent sectorContents){

            if(sectorContents == null) sectorContents = new RawContent(currentSector);

            byte[] originBlock = readBlock(i);
            if(isCancelled()) return true;
            if(originBlock != null){ //if address is valid
                int[] blockContent = {Byte.toUnsignedInt(originBlock[0]), Byte.toUnsignedInt(originBlock[1]), Byte.toUnsignedInt(originBlock[2]), Byte.toUnsignedInt(originBlock[3])};
                sectorContents.content.add(new RawBlockContent(i, blockContent)); // save block at address
                if(i == 255){
                    tp.nfcTag.rawContents.add(sectorContents);
                    return true; // finished
                }

                for (int b = 4; b < originBlock.length; b += 4) { //For each extra blocks
                    int subIndex = i + (b / 4);
                    int[] subBlockContent = {Byte.toUnsignedInt(originBlock[b]), Byte.toUnsignedInt(originBlock[b+1]), Byte.toUnsignedInt(originBlock[b+2]), Byte.toUnsignedInt(originBlock[b+3])};
                    if(!intArrayIsZeros(subBlockContent)) { //If not zeros then save it too
                        sectorContents.content.add(new RawBlockContent(subIndex, subBlockContent));
                        if(subIndex == 255){
                            tp.nfcTag.rawContents.add(sectorContents);
                            return true; // finished
                        }

                    }else { // if zeros call recursively on it
                        return readContentRecursivelyOptimized(subIndex, sectorContents);// subsequent blocks will inevitably be handled by this so no need to keep checking
                    }
                }
            }else { // if address was invalid save as invalid and follow up with next block
                sectorContents.content.add(new RawBlockContent(i, null, false, false, true, true));
                if(i == 255){
                    tp.nfcTag.rawContents.add(sectorContents);
                    return true; // finished
                }
                return readContentRecursivelyOptimized(++i, sectorContents);
            }
            i+=4;
            return readContentRecursivelyOptimized(i, sectorContents); //jump to next address
        }

        //Slow
        void readContentIterativelyOptimized(){
            RawContent sectorContents = new RawContent(currentSector);
            for(int i = 0; i < 255; i++) {
                //send command
                byte[] response = readBlock(i);
                //Parse response
                if (response != null && response.length > 0 && response.length % 4 == 0) { // store requested address block
                    int[] blockContent = {Byte.toUnsignedInt(response[0]), Byte.toUnsignedInt(response[1]), Byte.toUnsignedInt(response[2]), Byte.toUnsignedInt(response[3])};
                    sectorContents.content.add(new RawBlockContent(i, blockContent));

                    for (int b = 4; b < response.length; b += 4) { //For the following returned blocks

                        int[] subBlockContent = {Byte.toUnsignedInt(response[b]), Byte.toUnsignedInt(response[b+1]), Byte.toUnsignedInt(response[b+2]), Byte.toUnsignedInt(response[b+3])};
                        if(!intArrayIsZeros(subBlockContent)) { //Store them while they're not zeros
                            sectorContents.content.add(new RawBlockContent(i + (b / 4), subBlockContent));
                            i++;
                        }else break;

                    }

                }else{
                    sectorContents.content.add(new RawBlockContent(i, null, false, false, true, true));
                }
            }
            tp.nfcTag.rawContents.add(sectorContents);
        }

        void readContentFromPattern(TagPattern pattern) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcMU with pattern");
            for (int sectorPattern = 0; sectorPattern < pattern.memoryMap.length; sectorPattern++) {

                RawContent sectorContents = new RawContent(currentSector);
                if (sectorPattern == 0 || selectSector(sectorPattern)) {
                    int realBlockIndex = 0;
                    for (int blockPattern = 0; blockPattern < pattern.memoryMap[sectorPattern].length; blockPattern++) {

                        //Get range defined by current blockPattern
                        short lastByte = pattern.memoryMap[sectorPattern][blockPattern][pattern.memoryMap[sectorPattern][blockPattern].length - 1];
                        int rangeStart = realBlockIndex;
                        int rangeEnd = lastByte < 0 ? lastByte * -1 : rangeStart;
                        short[] cleanBlockPattern = getCleanBlockPattern(pattern.memoryMap[sectorPattern][blockPattern]);

                        //For invalid pattern quick fill with appropriate data
                        if (patternBlockIsInvalid(pattern.memoryMap[sectorPattern][blockPattern])) {
                            for (int block = rangeStart; block <= rangeEnd; block++) {
                                sectorContents.content.add(new RawBlockContent(block, new int[]{0,0,0,0}, cleanBlockPattern, false, false, true, true));
                            }
                            //For valid pattern read and fill
                        } else {
                            for (int block = rangeStart; block <= rangeEnd; block++) {
                                //read block
                                byte[] response = readBlock(block);
                                if(isCancelled()) return;
                                if (response != null) {
                                    //add block
                                    int[] blockContent = {Byte.toUnsignedInt(response[0]), Byte.toUnsignedInt(response[1]), Byte.toUnsignedInt(response[2]), Byte.toUnsignedInt(response[3])};
                                    sectorContents.content.add(new RawBlockContent(block, blockContent, cleanBlockPattern));
                                    //add extra blocks
                                    for (int offset = 4; offset < response.length; offset += 4) { //For each extra blocks
                                        int extraBlock = block + (offset / 4);
                                        if(extraBlock <= rangeEnd) {
                                            int[] subBlockContent = {Byte.toUnsignedInt(response[offset]), Byte.toUnsignedInt(response[offset+1]), Byte.toUnsignedInt(response[offset+2]), Byte.toUnsignedInt(response[offset+3])};
                                            sectorContents.content.add(new RawBlockContent(extraBlock, subBlockContent, cleanBlockPattern));
                                        }
                                    }
                                    block += 3;

                                //JIC
                                } else {
                                    sectorContents.content.add(new RawBlockContent(block, new int[]{0,0,0,0}, cleanBlockPattern, false, false, true, true));
                                }
                            }
                        }
                        realBlockIndex = ++ rangeEnd;

                    }

                    tp.nfcTag.rawContents.add(sectorContents);
                }
            }
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcMU with pattern done");
        }


        short[] getCleanBlockPattern(short[] blockPattern){
            if(blockPattern == null) return null;
            if(blockPattern.length <= 2){
                return new short[]{blockPattern[0],blockPattern[0],blockPattern[0],blockPattern[0]};
            }
            if(blockPattern.length == 4) return blockPattern;
            if(blockPattern.length == 5) return Arrays.copyOfRange(blockPattern, 0, 4);
            return null;
        }

        boolean patternBlockIsInvalid(short[] blockPattern){
            return blockPattern.length == 0 || blockPattern[0] == 0;
        }

        boolean selectSector(int i){
            if(i < 0 || isCancelled()) return false;
            boolean requestFinished = false;
            try {
                byte sector = (byte) i;
                byte[] response = nfcMU.transceive(new byte[]{
                        (byte) 0xC2,  // SECTOR_SELECT
                        (byte) 0xFF,  //
                });

                if(response.length > 0 && response[0] == (byte) 0x0A) {

                    requestFinished = true;
                    response = nfcMU.transceive(new byte[]{
                            sector,  // SECTOR_SELECT
                            (byte) 0x00,
                            (byte) 0x00,
                            (byte) 0x00,
                    });

                }

            }catch(Exception e){ //passive ACK throws tag lost, NAK throws IOException (transceive failed)
                if (requestFinished && e.getClass() == android.nfc.TagLostException.class){
                    currentSector = i;
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Found sector " + i);
                    return true;
                }
                return false;

            }
            return false;
        }

        byte[] readBlock(int i){
            try {
                /*
                byte block = (byte) i;
                byte[] response = nfcMU.transceive(new byte[]{
                        (byte) 0x30,  // READ
                        block,  // block
                });
                */
                return nfcMU.readPages(i);
            }catch(Exception e){
                if (e.getClass() == android.nfc.TagLostException.class){
                    onFail(e);
                }
                return  null;
            }
        }

        TagPattern getPattern(){
            if(tp.nfcTag.manufacturerID.equals("04")) return getNXPPattern();
            return null;
        }

        TagPattern getNXPPattern(){
            try { // check ev1's with GET_VERSION
                byte[] response = nfcMU.transceive(new byte[]{
                        (byte) 0x60
                });
                if (response.length > 0) {
                    tp.nfcTag.versionData = bytesToHex(response);
                    if (ULTRALIGHT_PATTERNS.containsKey(tp.nfcTag.versionData)) {
                        TagPattern pattern = ULTRALIGHT_PATTERNS.get(tp.nfcTag.versionData);
                        if (pattern != null) {
                            tp.nfcTag.storageSize = pattern.userMemory;
                            return pattern;
                        }
                    }
                }
            } catch (Exception e) { // Pre-ev1
                return  getOldNXPPattern();
            }
            return null;
        }

        TagPattern getOldNXPPattern(){
            try {
                byte[] response = nfcMU.transceive(new byte[]{
                        (byte) 0x1A,
                        (byte) 0x00
                });
                if (response.length > 0 &&  ((response[0] & 0x00A) == 0x000)) { // MIFARE Ultralight C or Hospitality
                    if(readBlock(2) != null) tp.nfcTag.versionData = "MUC";
                    else tp.nfcTag.versionData = "MH";
                    if (ULTRALIGHT_OLDER_PATTERNS.containsKey(tp.nfcTag.versionData)) {
                        TagPattern pattern = ULTRALIGHT_OLDER_PATTERNS.get(tp.nfcTag.versionData);
                        if (pattern != null) {
                            tp.nfcTag.storageSize = pattern.userMemory;
                            return pattern;
                        }
                    }
                }
            } catch (Exception e) { // Ultralight or NTAG203
                if(readBlock(41) != null) tp.nfcTag.versionData = "N203";
                else tp.nfcTag.versionData = "MU";
                if (ULTRALIGHT_OLDER_PATTERNS.containsKey(tp.nfcTag.versionData)) {
                    TagPattern pattern = ULTRALIGHT_OLDER_PATTERNS.get(tp.nfcTag.versionData);
                    if (pattern != null) {
                        tp.nfcTag.storageSize = pattern.userMemory;
                        return pattern;
                    }
                }
            }
            return null;

        }

    }

    //NfcA
    private static class ReadMifareClassic extends AsyncTask<TagPair, String, TagPair> {
        static TagPair tp;
        static  MifareClassic nfcMC;
        static  NfcA nfcA;
        int currentSector = 0;
        int sectorCount = 0;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcMC");
            tp = params[0];
            tp.nfcTag.rawContents = new ArrayList<>();
            nfcMC = MifareClassic.get(tp.tag);
            nfcA = NfcA.get(tp.tag);

            tp.nfcTag.sak = nfcA.getSak();
            tp.nfcTag.atqa = nfcA.getAtqa();
            tp.nfcTag.storageSize = nfcMC.getSize();
            sectorCount = nfcMC.getSectorCount();

            long startTime = System.nanoTime();

            try{
                nfcMC.connect();
                TagPattern pattern = getPattern();
                if(pattern != null && pattern.memoryMap != null){
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Pattern read");
                    readContentFromPattern(pattern);
                }else {
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Iterative read");
                    readContentsIteratively();
                }
                nfcMC.close();

            }catch (Exception e){
                onFail(e);
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
            sendToUnity(AndroidMessagePrefix.SCAN_FAIL, "Deep scan (MU) failed: "+ e.getMessage());
            this.cancel(true);
        }


        void readContentsIteratively(){
            for (int sector = 0; sector < sectorCount; sector++) {
                RawContent sectorContents = new RawContent(currentSector);
                int sectorBlocks = nfcMC.getBlockCountInSector(sector);
                for(int block = 0; block < sectorBlocks; block++){
                    //read block
                    byte[] response = readBlock(block);
                    if(isCancelled()) return;
                    if (response != null) {
                        //add block
                        int[] blockContent = {
                                Byte.toUnsignedInt(response[0]), Byte.toUnsignedInt(response[1]), Byte.toUnsignedInt(response[2]), Byte.toUnsignedInt(response[3]),
                                Byte.toUnsignedInt(response[4]), Byte.toUnsignedInt(response[5]), Byte.toUnsignedInt(response[6]), Byte.toUnsignedInt(response[7]),
                                Byte.toUnsignedInt(response[8]), Byte.toUnsignedInt(response[9]), Byte.toUnsignedInt(response[10]), Byte.toUnsignedInt(response[11]),
                                Byte.toUnsignedInt(response[12]), Byte.toUnsignedInt(response[13]), Byte.toUnsignedInt(response[14]), Byte.toUnsignedInt(response[15])
                        };
                        sectorContents.content.add(new RawBlockContent(block, blockContent));
                        //JIC
                    } else {
                        sectorContents.content.add(new RawBlockContent(block, new int[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, false, false, true, true));
                    }
                }
                tp.nfcTag.rawContents.add(sectorContents);
            }
        }

        void readContentFromPattern(TagPattern pattern) {
            for (int sectorPattern = 0; sectorPattern < pattern.memoryMap.length; sectorPattern++) {

                RawContent sectorContents = new RawContent(currentSector);
                if (selectSector(sectorPattern)) {
                    int realBlockIndex = 0;
                    for (int blockPattern = 0; blockPattern < pattern.memoryMap[sectorPattern].length; blockPattern++) {

                        //Get range defined by current blockPattern
                        short lastByte = pattern.memoryMap[sectorPattern][blockPattern][pattern.memoryMap[sectorPattern][blockPattern].length - 1];
                        int rangeStart = realBlockIndex;
                        int rangeEnd = lastByte < 0 ? lastByte * -1 : rangeStart;
                        short[] cleanBlockPattern = getCleanBlockPattern(pattern.memoryMap[sectorPattern][blockPattern]);

                        //For invalid pattern quick fill with appropriate data
                        if (patternBlockIsInvalid(pattern.memoryMap[sectorPattern][blockPattern])) {
                            for (int block = rangeStart; block <= rangeEnd; block++) {
                                sectorContents.content.add(new RawBlockContent(block, new int[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, cleanBlockPattern, false, false, true, true));
                            }

                            //For valid pattern read and fill
                        } else {
                            for (int block = rangeStart; block <= rangeEnd; block++) {
                                //read block
                                byte[] response = readBlock(block);
                                if(isCancelled()) return;
                                int number = response[0] & 0xff;
                                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Block " + blockPattern + " " + number);
                                if (response != null && response.length == 16) {
                                    //add block
                                    int[] blockContent = {
                                            Byte.toUnsignedInt(response[0]), Byte.toUnsignedInt(response[1]), Byte.toUnsignedInt(response[2]), Byte.toUnsignedInt(response[3]),
                                            Byte.toUnsignedInt(response[4]), Byte.toUnsignedInt(response[5]), Byte.toUnsignedInt(response[6]), Byte.toUnsignedInt(response[7]),
                                            Byte.toUnsignedInt(response[8]), Byte.toUnsignedInt(response[9]), Byte.toUnsignedInt(response[10]), Byte.toUnsignedInt(response[11]),
                                            Byte.toUnsignedInt(response[12]), Byte.toUnsignedInt(response[13]), Byte.toUnsignedInt(response[14]), Byte.toUnsignedInt(response[15])
                                    };
                                    sectorContents.content.add(new RawBlockContent(block, blockContent, cleanBlockPattern));
                                    //JIC
                                } else {
                                    sectorContents.content.add(new RawBlockContent(block, new int[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, cleanBlockPattern, false, false, true, true));
                                }
                            }
                        }
                        realBlockIndex = ++ rangeEnd;
                    }
                    tp.nfcTag.rawContents.add(sectorContents);
                }
            }
        }

        short[] getCleanBlockPattern(short[] blockPattern){
            if(blockPattern == null) return null;
            if(blockPattern.length <= 2){
                return new short[]{
                        blockPattern[0],blockPattern[0],blockPattern[0],blockPattern[0],
                        blockPattern[0],blockPattern[0],blockPattern[0],blockPattern[0],
                        blockPattern[0],blockPattern[0],blockPattern[0],blockPattern[0],
                        blockPattern[0],blockPattern[0],blockPattern[0],blockPattern[0]
                };
            }
            if(blockPattern.length == 16) return blockPattern;
            if(blockPattern.length == 17) return Arrays.copyOfRange(blockPattern, 0, 16);
            return null;
        }

        boolean patternBlockIsInvalid(short[] blockPattern){
            return blockPattern.length == 0 || blockPattern[0] == 0;
        }



        TagPattern getPattern(){
            if(tp.nfcTag.sak == 9) tp.nfcTag.versionData = "MM3"; //0x09
            else if(tp.nfcTag.sak == 8) tp.nfcTag.versionData = "MC1"; //0x08
            else if(tp.nfcTag.sak == 25) tp.nfcTag.versionData = "MC2"; //0x19
            else if(tp.nfcTag.sak == 24) tp.nfcTag.versionData = "MC4"; //0x18
            else if(tp.nfcTag.sak == 56){ // 0x38
                tp.nfcTag.versionData = "MC4";
                tp.nfcTag.emulated = "SmartMX";
            }
            else if(tp.nfcTag.sak == 40){ //0x28
                tp.nfcTag.versionData = "MC1";
                tp.nfcTag.emulated = "SmartMX";
            }

            if (CLASSIC_PATTERNS.containsKey(tp.nfcTag.versionData)) {
                TagPattern pattern = CLASSIC_PATTERNS.get(tp.nfcTag.versionData);
                if (pattern != null) {
                    tp.nfcTag.storageSize = pattern.userMemory;
                    return pattern;
                }
            }
            return  null;
        }

        byte[] readBlock(int i){
            i += nfcMC.sectorToBlock(currentSector);
            try {
                return nfcMC.readBlock(i);
            }catch(Exception e){
                if (e.getClass() == android.nfc.TagLostException.class){
                    onFail(e);
                }
                return  null;
            }
        }

        boolean selectSector(int i){
            if(i >= sectorCount || i < 0 || isCancelled()) return false;

            try {
                if(nfcMC.authenticateSectorWithKeyA(i, MifareClassic.KEY_DEFAULT)){
                    currentSector = i;
                    return true;
                }else{
                    sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Authentification failed for sector " + i);
                    return false;
                }
            }catch(Exception e){
                sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Authentification failed for sector " + i);
                return false;
            }
        }

    }


    private static class ReadNfcB extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static  NfcB nfcB;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcA, not yet implemented");
            tp = params[0];
            nfcB = NfcB.get(tp.tag);
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }

    //NfcA or NfcB
    private static class ReadIsoDep extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static IsoDep isoDep;
        static  NfcA nfcA;
        static NfcB nfcB;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning IsoDep, not yet implemented");
            tp = params[0];
            isoDep = IsoDep.get(tp.tag);
            nfcA = NfcA.get(tp.tag);
            nfcB = NfcB.get(tp.tag);
            if(nfcA != null) {
                tp.nfcTag.sak = nfcA.getSak();
                tp.nfcTag.atqa = nfcA.getAtqa();
            }
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }


    private static class ReadNfcF extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static NfcF nfcF;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcA, not yet implemented");
            tp = params[0];
            nfcF = NfcF.get(tp.tag);
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }


    private static class ReadNfcV extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static NfcV nfcV;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcA, not yet implemented");
            tp = params[0];
            nfcV = NfcV.get(tp.tag);
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }


    private static class ReadNfcBarcode extends AsyncTask<TagPair, String, TagPair> {

        static TagPair tp;
        static NfcBarcode nfcBarcode;

        @Override
        protected TagPair doInBackground(TagPair... params) {
            sendToUnity(AndroidMessagePrefix.DEBUG_LOG, "Deep Scanning NfcA, not yet implemented");
            tp = params[0];
            nfcBarcode = NfcBarcode.get(tp.tag);
            long startTime = System.nanoTime();
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
            sendToUnity(AndroidMessagePrefix.SCAN_END, new Gson().toJson(tp.nfcTag));
        }

    }




    public static class TagPattern{
        // 0 - inaccessible
        // 1 - user memory
        // 2 - CNT_MAX counter max value
        // 3 - UID
        // 4 - Internal
        // 5 - Dynamic lock bytes
        // 6 - RFU reserved for future use
        // 7 - AUTH0
        // 8 - ACCESS
        // 9 - PWD
        //10 - PACK
        //11 - PT_I2C
        //12 - TT message
        //13 - Config Registers
        //14 - Session Registers
        //15 - Static Lock Bytes
        //16 - Capability Container CC
        //17 - OTP
        public TagPattern(int userMemory, short[][][] memoryMap) {
            this.userMemory = userMemory;
            this.memoryMap = memoryMap;
        }

        public int userMemory;
        //[sector][block][content]
        //skipped blocks are same as last, use negative extra content to indicate end of skip
        //single content means same type for rest of block
        public short[][][] memoryMap;

    }

    static Map<String, TagPattern> ULTRALIGHT_PATTERNS = new HashMap<String, TagPattern>() {
        {
        //NTAG 210μ NT2L1001
        put("0004040102000B03", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -15}
                        }
                }
        ));
        //NTAG 210μ NT2H1001
        put("0004040202000B03", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -15}
                        }
                }
        ));
        //NTAG 210
        put("0004040101000B03", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -15},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG 212
        put("0004040101000E03", new TagPattern(
                128,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -35},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG213
        put("0004040201000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -39},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG213F
        put("0004040401000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -39},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG213TT
        put("0004040203000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -39},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6},
                                {12}
                        }
                }
        ));
        //NTAG215
        put("0004040201001103", new TagPattern(
                504,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -129},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG216
        put("0004040201001303", new TagPattern(
                888,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -225},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG216F
        put("0004040401001303", new TagPattern(
                888,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -225},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //NTAG223 DNA
        put("0004040204000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -39},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6},
                                {13, 6, 6, 6},
                                {6},
                                {2,2,2,6},
                                {6, -51},
                                {12, -55},
                                {6, -59}
                        }
                }
        ));
        //NTAG223 DNA SD
        put("0004040804000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -39},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {9},
                                {10, 10, 6, 6},
                                {13, 6, 6, 6},
                                {13, 6, 6, 6},
                                {2,2,2,6},
                                {6, -51},
                                {12, -55},
                                {13, -57},
                                {6, -59}
                        }
                }
        ));
        //NTAG224 DNA
        put("0004040205001003", new TagPattern(
                208,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -55},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {6},
                                {6},
                                {13, 6, 6, 6},
                                {13, 6, 6, 6},
                                {2,2,2,6},
                                {12, -71},
                                {6, -75}
                        }
                }
        ));
        //NTAG224 DNA SD
        put("0004040805001003", new TagPattern(
                208,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {16},
                                {1, -55},
                                {5, 5, 5, 6},
                                {13},
                                {13},
                                {6},
                                {6},
                                {13, 6, 6, 6},
                                {13, 6, 6, 6},
                                {2,2,2,6},
                                {12, -71},
                                {13, -73},
                                {6, -75}
                        }
                }
        ));
        //NTAG I2C 1k
        put("0004040502011303", new TagPattern(
                888,
                new short[][][]{
                        {//Sector 0
                                {3},//block 0
                                {3, 3, 3, 4},
                                {4, 4, 15, 15},
                                {16},
                                {1, -225},
                                {5, 5, 5, 1},
                                {0, -231},
                                {13, -233},
                                {0, -255},
                        },
                        {//Sector 1
                                {0,-255}//block 0
                        },
                        {//Sector 2
                                {0,-255}//block 0
                        },
                        {//Sector 3
                                {0,-247},//block 0
                                {14,-249},
                                {0,-255}
                        }
                }
        ));
        //NTAG I2C plus 1k
        put("0004040502021303", new TagPattern(
                888,
                new short[][][]{
                        {//Sector 0
                                {3},//block 0
                                {3, 3, 3, 4},
                                {4, 4, 15, 15},
                                {16},
                                {1, -225},
                                {5, 5, 5, 1},
                                {6, 6, 6, 7},
                                {8, 6, 6, 6},
                                {9},
                                {10, 10, 6, 6},
                                {11, 6, 6, 6},
                                {13, -233},
                                {0, -235},
                                {14, -237},
                                {0, -255}
                        },
                        {//Sector 1
                                {0,-255}//block 0
                        },
                        {//Sector 2
                                {0,-255}//block 0
                        },
                        {//Sector 3
                                {0,-247},//block 0
                                {14,-249},
                                {0,-255}
                        }
                }
        ));
        //NTAG I2C 2k
        put("0004040502011503", new TagPattern(
                1904,
                new short[][][]{
                        {//Sector 0
                                {3},//block 0
                                {3,3,3,4},
                                {4,4,15,15},
                                {16},
                                {1,-255},
                        },
                        {//Sector 1
                                {1, -223},
                                {5,5,5,1},
                                {0,-231},
                                {13,-233},
                                {0,-255}
                        },
                        {//Sector 2
                                {0,-255}//block 0
                        },
                        {//Sector 3
                                {0,-247},//block 0
                                {14,-249},
                                {0,-255}
                        }

                }
        ));
        //NTAG I2C plus 2k
        put("0004040502021503", new TagPattern(
                1912,
                new short[][][]{
                        {//Sector 0
                                {3},//block 0
                                {3,3,3,4},
                                {4,4,15,15},
                                {16},
                                {1,-225},
                                {5,5,5,1},
                                {6,6,6,7},
                                {8,6,6,6},
                                {9},
                                {10,10,6,6},
                                {11,6,6,6},
                                {13,-233},
                                {0,-235},
                                {14,-237},
                                {0,-255}
                        },
                        {//Sector 1
                                {1,-255}//block 0
                        },
                        {//Sector 2
                                {0,-255}//block 0
                        },
                        {//Sector 3
                                {0,-247},//block 0
                                {14,-249},
                                {0,-255}
                        }

                }
        ));
        //Mifare Ultralight EV1 11
        put("0004030101000B03", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {17},
                                {1, -15},
                                {13, -17},
                                {9},
                                {10, 10, 6, 6},
                                {2}
                        }
                }
        ));
        //Mifare Ultralight EV1 H11
        put("0004030201000B03", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {17},
                                {1, -15},
                                {13, -17},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //Mifare Ultralight EV1 21
        put("0004030101000E03", new TagPattern(
                128,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {17},
                                {1, -35},
                                {15, 15, 15, 6},
                                {13, -38},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //Mifare Ultralight EV1 H21
        put("0004030201000E03", new TagPattern(
                128,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3,4,15,15},
                                {17},
                                {1, -35},
                                {15, 15, 15, 6},
                                {13, -38},
                                {9},
                                {10, 10, 6, 6}
                        }
                }
        ));
        //MIFARE Ultralight AES
        put("0004030104000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -39},
                                {15, 15, 15, 6},
                                {13, -42},
                                {6, -47},
                                {12, -55},
                                {6, -59}
                        }
                }
        ));
        //MIFARE Ultralight AES H
        put("0004030204000F03", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -39},
                                {15, 15, 15, 6},
                                {13, -42},
                                {6, -47},
                                {12, -55},
                                {6, -59}
                        }
                }
        ));
        //MIFARE Ultralight Nano
        put("0004030102000B03", new TagPattern(
                40,
                new short[][][]{
                        {
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -13}
                        }
                }
        ));
        //MIFARE Ultralight Nano H
        put("0004030202000B03", new TagPattern(
                40,
                new short[][][]{
                        {
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -13}
                        }
                }
        ));
    }};

    static Map<String, TagPattern> ULTRALIGHT_OLDER_PATTERNS = new HashMap<String, TagPattern>() {{
        //MIFARE Ultralight C
        put("MUC", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -39},
                                {15, 15, 0, 0},
                                {2, 2, 0, 0},
                                {7, -43},
                                {12, -47}
                        }
                }
        ));
        //NTAG203
        put("N203", new TagPattern(
                144,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -39},
                                {15, 15, 0, 0},
                                {2, 2, 0, 0}
                        }
                }
        ));
        //MIFARE Ultralight
        put("MU", new TagPattern(
                48,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -15}
                        }
                }
        ));
        //MIFARE Hospitality
        put("MH", new TagPattern(
                96,
                new short[][][]{
                        {//Sector 0
                                {3, -1},//block 0
                                {3, 4, 15, 15},
                                {17},
                                {1, -15}
                        }
                }
        ));
    }};

    static  Map<String, TagPattern> CLASSIC_PATTERNS = new HashMap<String, TagPattern>(){
        {
            //MIFARE Mini 0.3K
            put("MM3", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            }
                    }
            ));
            //MIFARE Classic 1k
            put("MC1", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 5
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 6
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 7
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 8
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 9
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 10
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 11
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 12
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 13
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 14
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 15
                                    {1, -2},
                                    {12}
                            }
                    }
            ));
            //MIFARE Classic 2k
            put("MC2", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 5
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 6
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 7
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 8
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 9
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 10
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 11
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 12
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 13
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 14
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 15
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 16
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 17
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 18
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 20
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 21
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 22
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 23
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 24
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 25
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 26
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 27
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 28
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 29
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 30
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 31
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 32
                                    {1, -14},
                                    {12}
                            }
                    }
            ));
            //MIFARE Classic 4k
            put("MC4", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 5
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 6
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 7
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 8
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 9
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 10
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 11
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 12
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 13
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 14
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 15
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 16
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 17
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 18
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 20
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 21
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 22
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 23
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 24
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 25
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 26
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 27
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 28
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 29
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 30
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 31
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 32
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 33
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 34
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 35
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 36
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 37
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 38
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 39
                                    {1, -14},
                                    {12}
                            }
                    }
            ));
            //MIFARE Classic EV1 1k
            put("MCEV11", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 5
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 6
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 7
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 8
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 9
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 10
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 11
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 12
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 13
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 14
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 15
                                    {1, -2},
                                    {12}
                            }
                    }
            ));
            //MIFARE Classic EV1 4k
            put("MCEV14", new TagPattern(
                    0,
                    new short[][][]{
                            {//Sector 0
                                    {3},//block 0
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 1
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 3
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 4
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 5
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 6
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 7
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 8
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 9
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 10
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 11
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 12
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 13
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 14
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 15
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 16
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 17
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 18
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 20
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 21
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 22
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 23
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 24
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 25
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 26
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 27
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 28
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 29
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 30
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 31
                                    {1, -2},
                                    {12}
                            },
                            {//Sector 32
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 33
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 34
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 35
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 36
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 37
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 38
                                    {1, -14},
                                    {12}
                            },
                            {//Sector 39
                                    {1, -14},
                                    {12}
                            }
                    }
            ));
        }
    };

    static  Map<String, TagPattern> DESFIRE_PATTERNS = new HashMap<String, TagPattern>(){
        {
            //NTAG 413 DNA need access to doc 400310 from nxp docs

            //NTAG 424 DNA
            put("0404..3000110591AF", new TagPattern(
                    416, null
            ));

        }
    };

}
