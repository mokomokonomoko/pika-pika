using System.Runtime.InteropServices;
using UnityEngine;

namespace CardScanner
{
    public static class QRScannerBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StartQRScanner();
#endif

        public static void StartScanner()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            StartQRScanner();
#else
            Debug.Log("QRScannerBridge: WebGLビルド以外ではカメラを起動できません。");
#endif
        }
    }
}
