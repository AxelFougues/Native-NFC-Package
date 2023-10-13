using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace AzApps.NativeNFC {

    public class NativeNFC : MonoBehaviour {

        public enum DeviceTheme {
            UNSPECIFIED, LIGHT, DARK
        }

        public enum AndroidMessagePrefix {
            DEBUG_LOG, DEBUG_ERROR, SCAN_START, SCAN_PROGRESS, SCAN_END, SCAN_FAIL
        }

        public enum AndroidActionType {
            DEEP_SCAN, NDEF_SCAN, NDEF_WRITE, POWER, NONE
        }

        public enum AndroidAudioOutputType {
            TYPE_UNKNOWN, TYPE_BUILTIN_EARPIECE, TYPE_BUILTIN_SPEAKER, TYPE_WIRED_HEADSET, TYPE_WIRED_HEADPHONES, TYPE_LINE_ANALOG, TYPE_LINE_DIGITAL, TYPE_BLUETOOTH_SCO, TYPE_BLUETOOTH_A2DP, TYPE_HDMI, TYPE_HDMI_ARC, TYPE_USB_DEVICE, TYPE_USB_ACCESSORY, TYPE_DOCK, TYPE_FM, TYPE_BUILTIN_MIC, TYPE_FM_TUNER, TYPE_TV_TUNER, TYPE_TELEPHONY, TYPE_AUX_LINE, TYPE_IP, TYPE_BUS, TYPE_USB_HEADSET, TYPE_HEARING_AID, TYPE_BUILTIN_SPEAKER_SAFE, TYPE_REMOTE_SUBMIX, TYPE_BLE_HEADSET, TYPE_BLE_SPEAKER, TYPE_ECHO_REFERENCE, TYPE_HDMI_EARC, TYPE_BLE_BROADCAST
        }

        public static Action<string> onNFCScanFail;
        public static Action<string> onNFCDebugLog;
        public static Action<string> onNFCDebugError;
        public static Action<string> onNFCScanStart;
        public static Action<string> onNFCScanProgress;
        public static Action<NFCScanResult> onNFCScanEnd;

        static bool available = false;
        static AndroidJavaClass unityClass;
        static AndroidJavaObject unityActivity;

        public static InstalledAppsInfo installedApps = null;

        private void Awake() {
            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null && unityActivity.Call<bool>("androidAvailable")) {
                    available = true;
                }
            }

            getDeviceInstalledApps();
        }

        public static bool isAvailable() { return available; }

        public static void startScan(AndroidActionType type) {
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", true, type.ToString());
        }

        public static void startOperation(AndroidActionType type, string data) {
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", true, type.ToString(), data);
        }

        public static void stopScan() {
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", false, AndroidActionType.NONE.ToString());
        }

        public static DeviceTheme getDeviceTheme() {
            AndroidJavaClass unityClass;
            AndroidJavaObject unityActivity;
            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null) {
                    return (DeviceTheme)unityActivity.Call<int>("androidTheme", unityActivity);
                }
            }
            return DeviceTheme.UNSPECIFIED;
        }

        public static AudioOutputs getDeviceAudioOutputs() {
            AndroidJavaClass unityClass;
            AndroidJavaObject unityActivity;
            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null) {
                    return JsonUtility.FromJson<AudioOutputs>(unityActivity.Call<string>("androidAudioOutput"));
                }
            }
            return null;
        }

        public static bool setDeviceMaxVolume() {
            AndroidJavaClass unityClass;
            AndroidJavaObject unityActivity;
            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null) {
                    unityActivity.Call("androidMaxVolume");
                    return true;
                }
            }
            return false;
        }

        public static bool getDeviceInstalledApps() {
            AndroidJavaClass unityClass;
            AndroidJavaObject unityActivity;
            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null) {
                    InstalledAppsInfo iai = JsonUtility.FromJson<InstalledAppsInfo>(unityActivity.Call<string>("androidInstalledApps"));
                    if (iai != null) {
                        iai.createIcons();
                        installedApps = iai;
                        return true;
                    }
                }
            }
            return false;
        }

        #region FROM_UNITY

        void messageFromAndroid(string message) {
            //Parse prefix
            foreach (AndroidMessagePrefix prefix in (AndroidMessagePrefix[])Enum.GetValues(typeof(AndroidMessagePrefix))) {
                if (message.StartsWith(prefix.ToString())) {
                    message = message.Remove(0, prefix.ToString().Length + 1);
                    switch (prefix) {

                        case AndroidMessagePrefix.DEBUG_LOG:
                            message = "NativeNFC: " + message;
                            Debug.Log(message);
                            onNFCDebugLog?.Invoke(message);
                            return;
                        case AndroidMessagePrefix.DEBUG_ERROR:
                            message = "NativeNFC: " + message;
                            Debug.LogError(message);
                            onNFCDebugError?.Invoke(message);
                            return;

                        case AndroidMessagePrefix.SCAN_FAIL:
                            //NFCManager.instance.androidScanFail(message);
                            onNFCScanFail?.Invoke(message);
                            return;

                        case AndroidMessagePrefix.SCAN_START:
                            //NFCManager.instance.androidStartScan(message);
                            onNFCScanStart?.Invoke(message);
                            return;

                        case AndroidMessagePrefix.SCAN_PROGRESS:
                            //NFCManager.instance.androidScanProgress(message);
                            onNFCScanProgress?.Invoke(message);
                            return;

                        case AndroidMessagePrefix.SCAN_END:
                            //NFCManager.instance.androidScanEnd(message);
                            NFCTag nfcTag = JsonUtility.FromJson<NFCTag>(message);
                            if (nfcTag != null) {
                                onNFCScanEnd?.Invoke(new NFCScanResult(nfcTag));
                            } else {
                                onNFCScanFail?.Invoke("Tag de-serialization failed, NativeNFC lib returned invalid data: \n" + message);
                            }
                            return;
                    }
                }
            }
        }

        #endregion


        #region UTILITY_CLASSES

        [Serializable]
        public class NFCScanResult {
            [SerializeField] string time;
            DateTime parsedTime;
            public NFCTag tag;
            public string userUpgradeID;

            public NFCScanResult(NFCTag tag) {
                time = UserManager.dateTimeToString(DateTime.Now);
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
                    parsedTime = stringToDateTime(time);
                }
                return parsedTime;
            }

            static DateTime stringToDateTime(string time) {
                if (string.IsNullOrWhiteSpace(time)) return DateTime.Now;
                CultureInfo provider = CultureInfo.InvariantCulture;
                try {
                    return DateTime.ParseExact(time, "yyyy_MM_dd_HH_mm", provider);
                } catch (Exception e) {
                    return DateTime.Now;
                }
            }

        }

        //Audio outputs

        [Serializable]
        public class AudioOutput {
            public AndroidAudioOutputType deviceType = 0;
            public string address = "";
            public int id = -1;
            public string deviceName = "";
            public override string ToString() {
                return "Name: " + deviceName + "\nType: " + deviceType + "\nID: " + id + "\nAddress: " + address + "\n";
            }
        }
        [Serializable]
        public class AudioOutputs {
            public List<AudioOutput> outputs = new List<AudioOutput>();
            public AudioOutputs(List<AudioOutput> outputs) {
                this.outputs = new List<AudioOutput>(outputs);
            }
            public override string ToString() {
                string s = "Android Audio Outputs \n";
                foreach (AudioOutput ao in outputs) s += ao.ToString() + "\n";
                return s;
            }
        }


        //Installed apps

        [Serializable]
        public class InstalledAppsInfo {
            public List<InstalledAppInfo> apps = new List<InstalledAppInfo>();

            public void createIcons() {
                foreach (InstalledAppInfo app in apps) app.createIcon();
            }

            public Sprite getIcon(string packageName) {
                foreach (InstalledAppInfo info in apps) {
                    if (info.packageName.Equals(packageName)) return info.renderedIcon;
                }
                return null;
            }

            public override string ToString() {
                string s = "Installed Apps: " + apps.Count + " \n";
                foreach (InstalledAppInfo app in apps) s += app.ToString();
                return s;
            }
        }

        [Serializable]
        public class InstalledAppInfo {
            [Serializable]
            public class InstalledAppIcon {
                public int[] iconData;
            }
            public string packageName;
            public string readableName;
            public InstalledAppIcon icon;
            public Sprite renderedIcon;

            public void createIcon() {
                if (icon.iconData == null || icon.iconData.Length == 0) {
                    renderedIcon = null;
                    return;
                }
                byte[] bitmap = new byte[icon.iconData.Length];
                for (int i = 0; i < icon.iconData.Length; i++) bitmap[i] = (byte)icon.iconData[i];
                Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture2D.LoadImage(bitmap);
                renderedIcon = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                icon = null;
            }

            public override string ToString() {
                return packageName + " " + readableName + " " + renderedIcon.texture.width + "\n";
            }

        }

        #endregion
    }
}