using UnityEngine;
using UnityEngine.UI;

namespace CardScanner
{
    // シーン上のGameObject名は "QRManager" にすること。
    // JavaScript側(jsQR.jslib経由)から SendMessage("QRManager", "OnQRCodeScanned", text) で呼ばれる。
    public class QRManager : MonoBehaviour
    {
        [SerializeField] private Text resultText;

        // カメラ映像を表示する枠の初期位置・大きさ(0〜1の画面に対する割合)。
        // Rect.x/y: 左上の位置、Rect.width/height: どちらも画面幅に対する割合。
        [SerializeField] private Rect cameraRect = new Rect(0.3f, 0.5f, 0.4f, 0.4f);

        private void Start()
        {
            QRScannerBridge.StartScanner();
            QRScannerBridge.SetCameraRect(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height);
        }

        // ブラウザ側のJavaScriptから SendMessage で呼び出される
        public void OnQRCodeScanned(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
            Debug.Log($"QRManager: QRコードを読み取りました: {text}");
        }
    }
}
