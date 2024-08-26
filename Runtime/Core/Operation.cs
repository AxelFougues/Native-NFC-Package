using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AbyssWalkerDev.NativeNFC.NFCTag;

[Serializable]
public class Operation : MonoBehaviour{

    
    public NFC_Technology technologyUsed = NFC_Technology.UNKNOWN;
    public List<ChainedCommand> commands = new List<ChainedCommand>();


    public class ChainedCommand {
        public byte[] command = null;
        public byte[] expectedReply = null;
        public byte[] reply = null;
    }


}
