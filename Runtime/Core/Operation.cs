using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AbyssWalkerDev.NativeNFC.NFCTag;

[Serializable]
public class Operation {


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

        public override string ToString() {
            return ToStringIndented(0, "");
        }

        public string ToStringIndented(int level, string character) {
            string indentation = "";
            for (int i = 0; i < level; i++) indentation += character;
            string output = indentation + "---Command---\n";
            output += indentation + "Cmd: " + NFCUtils.bytesToHexString(command) + "\n";
            output += indentation + "Ex. reply: " + NFCUtils.bytesToHexString(expectedReply) + "\n";
            output += indentation + "Reply: " + NFCUtils.bytesToHexString(reply) + "\n";
            return output;
        }

    }

    public override string ToString() {
        return ToStringIndented(0, "");
    }

    public string ToStringIndented(int level, string character) {
        string indentation = "";
        for (int i = 0; i < level; i++) indentation += character;
        string output = indentation + "---Operation---\n";
        output += indentation + "Technology used: " + technologyUsed + "\n";
        output += indentation + "Commands:\n";
        if (commands != null && commands.Count > 0) foreach (ChainedCommand cc in commands) output += cc.ToStringIndented(++level, character);
        else output += indentation + "None.\n";

        return output;
    }


}
