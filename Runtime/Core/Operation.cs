using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AbyssWalkerDev.NativeNFC.NFCTag;

[Serializable]
public class Operation{

    
    public NFC_Technology technologyUsed = NFC_Technology.UNKNOWN;
    public List<ChainedCommand> commands = new List<ChainedCommand>();

    [Serializable]
    public class ChainedCommand {
        public byte[] command = null;
        public byte[] expectedReply = null;
        public byte[] reply = null;

        public ChainedCommand() {
        }

        public ChainedCommand(byte[] command, byte[] expectedReply) {
            this.command = command;
            this.expectedReply = expectedReply;
        }
    }


}
