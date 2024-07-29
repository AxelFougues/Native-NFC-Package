using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AbyssWalkerDev.NativeNFC {

    public class NativeNFCManager : MonoBehaviour {

        public static Action<Connection> onTagConnected;
        public static Action<Connection> onTagDisconnected;
        public static Action<Connection> onTagLost;
        public static Action<Connection> onTagUpdated;

        public static Action<string> onDebug;
        public static Action<string> onWarning;
        public static Action<string> onError;

        static bool available = false;
        static AndroidJavaClass unityClass;
        static AndroidJavaObject unityActivity;

        static NativeNFCManager instance = null;

        [Header("Editor debug tools")]
        public bool fakeAvailability;
        public bool fakeTagResponses;
        public Connection fakeConnection = new Connection();

        private void Awake() {
            if (instance != null) {
                Debug.Log("Multiple instances of NativeNFCManager detected. Only one must be present. Destroying self.");
                Destroy(this);
                gameObject.name = "Duplicate NativeNFC - Remove me";
                return;
            }
            instance = this;
            gameObject.name = "NativeNFCManager";

            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                available = true;
            }
        }

        public static bool isAvailable() {
            if (!available && instance != null && instance.fakeAvailability) return true; 
            return available;
        }

        #region SCAN

        public static bool startScan() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if(instance.fakeTagResponses) onTagConnected?.Invoke(instance.fakeConnection);
                    return true;
                }
                else return false;
            }

            unityActivity.Call("scan", true);
            return true;
        }

        public static void stopScan() {
            if (!available) {
                return;
            }
            unityActivity.Call("scan", false);
        }

        public static bool startNDEFScan() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagConnected?.Invoke(instance.fakeConnection);
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("scanNDEF", true);
            return true;
        }

        public static void stopNDEFScan() {
            if (!available) {
                return;
            }
            unityActivity.Call("scanNDEF", false);
        }

        public static bool startNDEFWrite(NDEFContent content) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagConnected?.Invoke(instance.fakeConnection);
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("writeNDEF", true, content);
            return true;
        }

        public static bool stopNDEFWrite() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagConnected?.Invoke(instance.fakeConnection);
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("writeNDEF", false, "");
            return true;
        }

        public static bool startPowerScan() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagConnected?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("scanPower", true);
            return true;
        }

        public static void stopPowerScan() {
            if (!available) {
                return;
            }
            unityActivity.Call("scanPower", false);
        }

        public static void performOperation(Operation operation) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    onTagUpdated?.Invoke(instance.fakeConnection);
                }
                return;
            }
        }

        #endregion

        #region FROM_ANDROID

        void debugFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.Log(message);
            onDebug?.Invoke(message);
        }

        void warningFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.LogWarning(message);
            onWarning?.Invoke(message);
        }

        void errorFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.LogError(message);
            onError?.Invoke(message);
        }

        void tagConnectedFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) onTagConnected?.Invoke(c);
            else errorFromAndroid("[Tag connected] Can't parse received connection object: " + message);
        }

        void tagDisconnectedFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) onTagDisconnected?.Invoke(c);
            else errorFromAndroid("[Tag disconnected] Can't parse received connection object: " + message);
        }

        void tagLostFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) onTagLost?.Invoke(c);
            else errorFromAndroid("[Tag lost] Can't parse received connection object: " + message);
        }

        void tagUpdatedFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) onTagUpdated?.Invoke(c);
            else errorFromAndroid("[Tag update] Can't parse received connection object: " + message);
        }

        #endregion





        //TOOLS
        public enum DeviceTheme {
            UNSPECIFIED, LIGHT, DARK
        };

        public static InstalledAppsInfo installedApps = new InstalledAppsInfo();

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
    }


    [Serializable]
    public class Connection {
        public enum OperationStatus { SUCCESS, FAILED, PARTIAL, NONE }
        public enum ConnectionStatus { CONNECTED, DISCONNECTED, LOST }

        public NFCTag tag = new NFCTag();
        public OperationStatus operationStatus = OperationStatus.NONE;
        public ConnectionStatus connectionStatus = ConnectionStatus.CONNECTED;
        public long operationDuration = 0;
        public string error;

    }

}