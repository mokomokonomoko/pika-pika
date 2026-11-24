using UnityEngine;
using UnityEngine.UI;

namespace CardScanner
{
    // シーン上のGameObject名は "QRManager" にすること。
    // JavaScript側(jsQR.jslib経由)から SendMessage("QRManager", "OnQRCodeScanned", text) で呼ばれる。
    public class QRManager : MonoBehaviour
    {
        [SerializeField] private Text resultText;

        private void Start()
        {
            QRScannerBridge.StartScanner();
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
