using System.Runtime.InteropServices;
using UnityEngine;

namespace CardScanner
{
    public static class QRScannerBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StartQRScanner();

        [DllImport("__Internal")]
        private static extern void SetQRCameraRect(float x, float y, float width, float height);
#endif

        public static void StartScanner()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            StartQRScanner();
#else
            Debug.Log("QRScannerBridge: WebGLビルド以外ではカメラを起動できません。");
#endif
        }

        // x, y, width, height はすべて 0〜1 の画面に対する割合。
        // x: 画面幅に対する左端 / y: 画面高さに対する上端
        // width, height: どちらも画面幅に対する割合(同じ値にすると正方形になる)
        public static void SetCameraRect(float x, float y, float width, float height)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SetQRCameraRect(x, y, width, height);
#else
            Debug.Log($"QRScannerBridge: SetCameraRect({x}, {y}, {width}, {height}) はWebGLビルド以外では無視されます。");
#endif
        }
    }
}
