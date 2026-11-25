using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardScanner
{
    // シーン上のGameObject名は "QRManager" にすること。
    // JavaScript側(jsQR.jslib経由)から SendMessage("QRManager", "OnQRCodeScanned", text) で呼ばれる。
    public class QRManager : MonoBehaviour
    {
        [SerializeField] private Text resultText;
        [SerializeField] private bool startCameraAutomatically = true;

        // カメラ映像を表示する枠の初期位置・大きさ(0〜1の画面に対する割合)。
        // Rect.x/y: 左上の位置、Rect.width/height: どちらも画面幅に対する割合。
        [SerializeField] private Rect cameraRect = new Rect(0.3f, 0.5f, 0.4f, 0.4f);

        // エディタ専用のカメラ表示(QREditorWebcamView)などが、スマホと同じ枠位置を参照するために使う
        public Rect CameraRect => cameraRect;
#if UNITY_EDITOR
        private QREditorWebcamView editorWebcam;
        private QREditorZXingReader editorReader;
#endif

        public event Action<string> CodeScanned;
        public event Action<string> CameraStatus;
        public event Action<string> PhotoReceived;

        private void Start()
        {
            if (!startCameraAutomatically)
                return;

#if UNITY_EDITOR
            SetCameraMode(1);
#else
            QRScannerBridge.StartScanner();
            QRScannerBridge.SetCameraRect(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height);
#endif
        }

        public void SetCameraMode(int mode)
        {
#if UNITY_EDITOR
            if (mode != 1)
            {
                StopEditorCamera();
                return;
            }

            if (editorWebcam == null)
            {
                var cameraObject = new GameObject("Editor Webcam");
                cameraObject.transform.SetParent(transform, false);
                editorWebcam = cameraObject.AddComponent<QREditorWebcamView>();
                editorReader = cameraObject.AddComponent<QREditorZXingReader>();
            }

            StopEditorCamera();
            if (editorWebcam.Open(this))
                editorReader.Begin(this, editorWebcam);
#else
            QRScannerBridge.SetCameraRect(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height);
            QRScannerBridge.SetMode(mode);
#endif
        }

#if UNITY_EDITOR
        private void OnDisable() => StopEditorCamera();

        private void StopEditorCamera()
        {
            if (editorReader != null)
                editorReader.enabled = false;
            if (editorWebcam != null)
                editorWebcam.Close();
        }
#endif

        public void CapturePhoto() => QRScannerBridge.CapturePhoto();
        public void OnQRCameraStatus(string status)
        {
            CameraStatus?.Invoke(status);
            if (startCameraAutomatically && status != "ready")
                Debug.LogWarning(status, this);
        }
        public void OnQRCameraPhoto(string jpeg) => PhotoReceived?.Invoke(jpeg);

        // ブラウザ側のJavaScriptから SendMessage で呼び出される
        public void OnQRCodeScanned(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
            Debug.Log($"QRManager: QRコードを読み取りました: {text}");
            CodeScanned?.Invoke(text);
        }
    }
}
