namespace AbyssWalkerDev.NativeNFC {
    /*
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

        public string vivokeyApiKey = "";

        static string VIVOKEY_URL = "https://auth.vivokey.com/";

        public static Action<NFCTag> onNFCScanFail;
        public static Action<string> onNFCDebugLog;
        public static Action<string> onNFCDebugError;
        public static Action<string> onNFCScanStart;
        public static Action<NFCTag> onNFCScanProgress;
        public static Action<NFCTag> onNFCScanEnd;

        static bool available = false;
        static AndroidJavaClass unityClass;
        static AndroidJavaObject unityActivity;



        public static InstalledAppsInfo installedApps = new InstalledAppsInfo();

        private void Awake() {
            gameObject.name = "NativeNFC";

            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (unityActivity != null && unityActivity.Call<bool>("androidAvailable")) {
                    available = true;
                }
            }
        }

        public static bool isAvailable() { return available; }

        #region SCAN

        public static void startScan(AndroidActionType type) {
            Debug.Log("Unity request scan " + type);
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", true, type.ToString());
        }

        public static void startOperation(AndroidActionType type, string data) {
            Debug.Log("Unity request operation " + type);
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", true, type.ToString(), data);
        }

        public static void stopScan() {
            Debug.Log("Unity request stop scan");
            if (!available) return;
            unityActivity.Call("toggleNFCIntentCapture", false, AndroidActionType.NONE.ToString());
        }

        #endregion
        #region ANDROID_UTILITIES

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


        #endregion
        #region VIVOKEY

        IEnumerator vivokeyChallenge(string scheme, string uid = null, string message = null) {
            WWWForm form = new WWWForm();
            form.headers.Add("Content-Type", "application/json");
            form.headers.Add("X-API-VIVOKEY", vivokeyApiKey);
            form.AddField("scheme", scheme);
            if (!string.IsNullOrWhiteSpace(uid)) form.AddField("uid", uid);
            if (!string.IsNullOrWhiteSpace(message)) form.AddField("message", message);

            using (UnityWebRequest www = UnityWebRequest.Post(VIVOKEY_URL + "challenge", form)) {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.Log("Vivokey challenge failed: " + www.error + " " + www.responseCode);
                } else {
                    Debug.Log("Form upload complete!");
                    Debug.Log("payload: " + www.GetResponseHeader("payload"));
                    Debug.Log("token: " + www.GetResponseHeader("token"));
                }
            }
        }

        IEnumerator vivokeySession(string uid, string response, string token, string cld = null) {
            WWWForm form = new WWWForm();
            form.headers.Add("Content-Type", "application/json");
            form.headers.Add("X-API-VIVOKEY", vivokeyApiKey);
            form.AddField("uid", uid);
            form.AddField("response", response);
            form.AddField("token", token);
            if (!string.IsNullOrWhiteSpace(cld)) form.AddField("cld", cld);

            using (UnityWebRequest www = UnityWebRequest.Post(VIVOKEY_URL + "session", form)) {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.Log("Vivokey session failed: " + www.error + " " + www.responseCode);
                } else {
                    Debug.Log("Form upload complete!");
                    Debug.Log("token: " + www.GetResponseHeader("token"));
                }
            }
        }

        IEnumerator vivokeyValidate(string signature) {
            WWWForm form = new WWWForm();
            form.headers.Add("Content-Type", "application/json");
            form.headers.Add("X-API-VIVOKEY", vivokeyApiKey);
            form.AddField("signature", signature);

            using (UnityWebRequest www = UnityWebRequest.Post(VIVOKEY_URL + "validate", form)) {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.Log("Vivokey validate failed: " + www.error + " " + www.responseCode);
                } else {
                    Debug.Log("Form upload complete!");
                    Debug.Log("result: " + www.GetResponseHeader("result"));
                    Debug.Log("token: " + www.GetResponseHeader("token"));
                }
            }
        }

        #endregion
        #region FROM_UNITY

        void installedAppsLoaded(string apps) {
            InstalledAppsInfo iai = JsonUtility.FromJson<InstalledAppsInfo>(apps);
            if (iai != null) {
                iai.createIcons();
                installedApps = iai;
                Debug.Log("NativeNFC received " + installedApps.apps.Count + " installed app infos.");
            } else {
                Debug.Log("NativeNFC received no installed app infos.");
            }
        }

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
                            break;
                        case AndroidMessagePrefix.DEBUG_ERROR:
                            message = "NativeNFC: " + message;
                            Debug.LogError(message);
                            onNFCDebugError?.Invoke(message);
                            break;

                        case AndroidMessagePrefix.SCAN_FAIL:
                            //NFCManager.instance.androidScanFail(message);
                            NFCTag nfcTagFail = JsonUtility.FromJson<NFCTag>(message);
                            if (nfcTagFail != null) {
                                onNFCScanFail?.Invoke(nfcTagFail);
                            } else {
                                Debug.LogError("Tag de-serialization failed, NativeNFC lib returned invalid data");
                            }
                            break;

                        case AndroidMessagePrefix.SCAN_START:
                            //NFCManager.instance.androidStartScan(message);
                            onNFCScanStart?.Invoke(message);
                            break;

                        case AndroidMessagePrefix.SCAN_PROGRESS:
                            //NFCManager.instance.androidScanProgress(message);
                            NFCTag nfcTagProgress = JsonUtility.FromJson<NFCTag>(message);
                            if (nfcTagProgress != null) {
                                onNFCScanProgress?.Invoke(nfcTagProgress);
                            } else {
                                Debug.LogError("Tag de-serialization failed, NativeNFC lib returned invalid data");
                            }
                            break;

                        case AndroidMessagePrefix.SCAN_END:
                            //NFCManager.instance.androidScanEnd(message);
                            NFCTag nfcTag = JsonUtility.FromJson<NFCTag>(message);
                            if (nfcTag != null) {
                                onNFCScanEnd?.Invoke(nfcTag);
                            } else {
                                Debug.LogError("Tag de-serialization failed, NativeNFC lib returned invalid data");
                            }
                            break;
                    }
                }
            }
        }

        #endregion


        #region UTILITY_CLASSES



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
    */
}