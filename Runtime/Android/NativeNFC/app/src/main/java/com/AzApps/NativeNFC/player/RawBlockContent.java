package com.AzApps.NativeNFC.player;

//Tag content block data structure. Mirrored in C#
public class RawBlockContent {
    public int blockIndex = 0;
    public int[] blockContent;
    public short[] byteTypes;
    public boolean writable = true;
    public boolean readable = true;
    public boolean locked = false;
    public boolean blocked = false;

    public RawBlockContent(int blockIndex, int[] blockContent) {
        this.blockIndex = blockIndex;
        this.blockContent = blockContent;
    }

    public RawBlockContent(int blockIndex, int[] blockContent, short[] byteTypes) {
        this.blockIndex = blockIndex;
        this.blockContent = blockContent;
        this.byteTypes = byteTypes;
    }

    public RawBlockContent(int blockIndex, int[] blockContent, boolean writable, boolean readable, boolean locked, boolean blocked) {
        this.blockIndex = blockIndex;
        this.blockContent = blockContent;
        this.writable = writable;
        this.readable = readable;
        this.locked = locked;
        this.blocked = blocked;
    }

    public RawBlockContent(int blockIndex, int[] blockContent, short[] byteTypes, boolean writable, boolean readable, boolean locked, boolean blocked) {
        this.blockIndex = blockIndex;
        this.blockContent = blockContent;
        this.byteTypes = byteTypes;
        this.writable = writable;
        this.readable = readable;
        this.locked = locked;
        this.blocked = blocked;
    }
}
