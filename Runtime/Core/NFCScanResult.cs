using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AzApps.NativeNFC {

    [Serializable]
    public class NFCScanResult {
        [SerializeField] string time;
        DateTime parsedTime;
        public NFCTag tag;
        public string userUpgradeID;

        public NFCScanResult(NFCTag tag) {
            time = NFCUtils.dateTimeToString(DateTime.Now);
            this.tag = extrapolateAndroidTagData(tag);
        }

        NFCTag extrapolateAndroidTagData(NFCTag tag) {
            if (tag == null) return null;
            //Extrapolate IC for NTAG w/ versionData
            if (!string.IsNullOrEmpty(tag.versionData) && NFCUtils.NTAG_VERSION_DATA_IC.ContainsKey(tag.versionData)) tag.icName = NFCUtils.NTAG_VERSION_DATA_IC[tag.versionData];
            //Clean up ndef message
            if (tag.ndefMessage != null) tag.ndefMessage.trySimplify();

            return tag;
        }

        public string getTimeString() { return time; }
        public DateTime getTime() {
            if (parsedTime == DateTime.MinValue) {
                parsedTime = NFCUtils.stringToDateTime(time);
            }
            return parsedTime;
        }


    }
}
