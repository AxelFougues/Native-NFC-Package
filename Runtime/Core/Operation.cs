using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AbyssWalkerDev.NativeNFC.NFCTag;

[Serializable]
public class Operation : MonoBehaviour{

    public class ChainedCommand {
        public List<byte> command = new List<byte>();
        public List<byte> expectedReply = new List<byte>();
    }


    public NFC_Technology technologyUsed;
    public List<ChainedCommand> commands;

}
