using System;
using UnityEngine;
namespace AbyssWalkerDev.NativeNFC {

    public class NativeNFCManager : MonoBehaviour {

        public Action<Connection> onTagConnected;
        public Action<Connection> onTagDisconnected;
        public Action<Connection> onTagLost;
        public Action<Connection> onTagUpdated;

        static bool available = false;
        static AndroidJavaClass unityClass;
        static AndroidJavaObject unityActivity;

        private void Awake() {
            gameObject.name = "NativeNFC";

            if (Application.platform == RuntimePlatform.Android) {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }


        #region SCAN

        public static bool startScan() {
            if (!available) return false;
            unityActivity.Call("scan", true);
            return true;
        }

        public static void stopScan() {
            if (!available) return;
            unityActivity.Call("scan", false);
        }

        #endregion

        #region FROM_ANDROID

        void debugFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.Log(message);
        }

        void warningFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.LogWarning(message);
        }

        void errorFromAndroid(string message) {
            message = "NativeNFC: " + message;
            Debug.LogError(message);
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

    }

    [Serializable]
    public class Connection {
        public enum OperationStatus { SUCCESS, FAILED, PARTIAL, NONE }
        public enum ConnectionStatus { CONNECTED, DISCONNECTED, LOST }

        public NFCTag tag;
        public OperationStatus operationStatus = OperationStatus.NONE;
        public ConnectionStatus connectionStatus = ConnectionStatus.CONNECTED;
        public long operationDuration = 0;

    }

}