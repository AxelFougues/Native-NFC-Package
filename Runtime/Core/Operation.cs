using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Operation : MonoBehaviour{

    public enum StandardComplexOperation {
        None,
        ChainedCommand,
        DeepScan,
        PowerScan,
        WriteNDEF,
        ReadNDEF
    }
    public class ChainedCommand {
        List<byte> command = new List<byte>();
        List<byte> expectedReply = new List<byte>();
    }

    public StandardComplexOperation standardComplexOperation;
    public NDEFContent ndefMessage;
    public List<ChainedCommand> commands;

}
