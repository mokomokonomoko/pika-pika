using System;
using UnityEngine;
using UnityEngine.UI;
using Cards;

namespace CardScanner
{
    // シーン上のGameObject名は "QRManager" にすること。
    // JavaScript側(jsQR.jslib経由)から SendMessage("QRManager", "OnQRCodeScanned", text) で呼ばれる。
    public class QRManager : MonoBehaviour
    {
        [SerializeField] private Text resultText;
        [SerializeField] private Image cardImage;
        [SerializeField] private CardDatabase cardDatabase;

        // カメラ映像を表示する枠の初期位置・大きさ(0〜1の画面に対する割合)。
        // Rect.x/y: 左上の位置、Rect.width/height: どちらも画面幅に対する割合。
        [SerializeField] private Rect cameraRect = new Rect(0.3f, 0.5f, 0.4f, 0.4f);

        // エディタ専用のカメラ表示(QREditorWebcamView)などが、スマホと同じ枠位置を参照するために使う
        public Rect CameraRect => cameraRect;

        // ゲーム側がカードIDを受け取りたいときはこれを購読する(例: CARD:0001 なら 1)。
        public event Action<int> CardScanned;

        // ゲーム側がIDから画像を取得したいときは、このデータベースの GetSprite(id) を使う。
        public CardDatabase Database => cardDatabase;

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

            if (!CardIdParser.TryParse(text, out int cardId))
            {
                return;
            }

            if (cardDatabase != null)
            {
                var sprite = cardDatabase.GetSprite(cardId);
                if (cardImage != null && sprite != null)
                {
                    cardImage.sprite = sprite;
                    cardImage.enabled = true;
                }
            }

            CardScanned?.Invoke(cardId);
        }
    }
}
