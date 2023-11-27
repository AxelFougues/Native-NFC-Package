package com.AzApps.NativeNFC.player;

import java.util.ArrayList;

//Tag raw content data structure. Mirrored in C#
public class RawContent {
    public int sector = 0;
    public ArrayList<RawBlockContent> content = new ArrayList<RawBlockContent>();

    public RawContent(int sector){
        this.sector = sector;
        content = new ArrayList<>();
    }

}
