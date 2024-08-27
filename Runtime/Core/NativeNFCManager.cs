using AbyssWalkerDev.NativeNFC;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AbyssWalkerDev.NativeNFC {

    public class NativeNFCManager : MonoBehaviour {

        public enum ActionType { NONE, DUMP, NDEF, POWER, WNDEF, FNDEF, FRONDEF, MRONDEF, OPERATION };

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

        public static bool connect() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if(instance.fakeTagResponses) onTagConnected?.Invoke(instance.fakeConnection);
                    return true;
                }
                else return false;
            }

            unityActivity.Call("connect");
            return true;
        }
        public static bool connect(ActionType actionOnConnect) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) onTagConnected?.Invoke(instance.fakeConnection);
                    switch (actionOnConnect) {
                        case ActionType.DUMP: dumpMemory(); break;
                        case ActionType.MRONDEF: makeNDEFReadOnly(); break;
                        case ActionType.NDEF: readNDEF(); break;
                        case ActionType.POWER: power(); break;
                    }
                    return true;
                } else return false;
            }

            unityActivity.Call("connect", (int)actionOnConnect);
            return true;
        }
        public static bool connect(ActionType actionOnConnect, string actionOnConnectParameters) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) onTagConnected?.Invoke(instance.fakeConnection);
                    switch (actionOnConnect) {
                        case ActionType.FNDEF: formatNDEF(new NDEFContent()); break;
                        case ActionType.FRONDEF: formatNDEFReadOnly(new NDEFContent()); break;
                        case ActionType.OPERATION: performOperation(new Operation()); break;
                        case ActionType.WNDEF: writeNDEF(new NDEFContent()); break;
                    }
                    return true;
                } else return false;
            }

            unityActivity.Call("connect", (int)actionOnConnect, actionOnConnectParameters);
            return true;
        }

        public static void disconnect() {
            if (!available) {
                return;
            }
            unityActivity.Call("disconnect");
        }

        public static bool dumpMemory() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagConnected?.Invoke(instance.fakeConnection);
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("dumpMemory");
            return true;
        }

        public static bool readNDEF() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("readNDEF");
            return true;
        }

        public static bool writeNDEF(NDEFContent content) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("writeNDEF", JsonUtility.ToJson(content));
            return true;
        }

        public static bool formatNDEF(NDEFContent content) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("formatNDEF", JsonUtility.ToJson(content));
            return true;
        }

        public static bool formatNDEFReadOnly(NDEFContent content) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("formatNDEFReadOnly", JsonUtility.ToJson(content));
            return true;
        }

        public static bool makeNDEFReadOnly() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("makeNDEFReadOnly");
            return true;
        }

        public static bool power() {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("power");
            return true;
        }

        public static bool performOperation(Operation operation) {
            if (!available) {
                if (instance != null && instance.fakeAvailability) {
                    if (instance.fakeTagResponses) {
                        onTagUpdated?.Invoke(instance.fakeConnection);
                    }
                    return true;
                } else return false;
            }
            unityActivity.Call("performOperation", JsonUtility.ToJson(operation));
            return true;
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
            if (c != null) {
                onTagConnected?.Invoke(c);
                Debug.Log("[Tag connected]" + message);
            } else errorFromAndroid("[Tag connected] Can't parse received connection object: " + message);
            Debug.Log(message);
        }

        void tagDisconnectedFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) {
                onTagDisconnected?.Invoke(c);
                Debug.Log("[Tag disconnected]" + message);
            } else errorFromAndroid("[Tag disconnected] Can't parse received connection object: " + message); 
        }

        void tagLostFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) {
                onTagLost?.Invoke(c);
                Debug.Log("[Tag lost]" + message);
            } else errorFromAndroid("[Tag lost] Can't parse received connection object: " + message);
        }

        void tagUpdatedFromAndroid(string message) {
            Connection c = JsonUtility.FromJson<Connection>(message);
            if (c != null) {
                onTagUpdated?.Invoke(c);
                Debug.Log("[Tag update]" + message);
            } else errorFromAndroid("[Tag update] Can't parse received connection object: " + message);
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

        public NFCTag tag = new NFCTag();
        public OperationStatus operationStatus = OperationStatus.NONE;
        public long operationDuration = 0;
        public string error;

    }

}