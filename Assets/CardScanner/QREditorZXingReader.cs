using UnityEngine;

namespace CardScanner
{
#if UNITY_EDITOR
    using ZXing;

    // Unityエディタの Play モード専用。QREditorWebcamView が表示しているMacカメラの映像
    // (枠に表示されている範囲)を一定間隔でZXing.Netに渡してQRコードを読み取る。
    // 検出した文字列は、スマホ側(JS)と同じ公開メソッド QRManager.OnQRCodeScanned を直接呼ぶ。
    // WebGLビルドには含まれない(クラス全体がUNITY_EDITOR、zxing.dllもWebGL対象外に設定済み)。
    public class QREditorZXingReader : MonoBehaviour
    {
        private const float ScanIntervalSeconds = 0.2f;
        private const float DebounceSeconds = 3f;

        private QRManager qrManager;
        private QREditorWebcamView webcamView;
        private BarcodeReaderGeneric barcodeReader;
        private byte[] rgbaBuffer;

        private float scanTimer;
        private string lastText;
        private float lastSentAt = -DebounceSeconds;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<QREditorZXingReader>() != null)
            {
                return;
            }

            var go = new GameObject("QREditorZXingReader (Editor Only)");
            go.AddComponent<QREditorZXingReader>();
        }

        private void Start()
        {
            qrManager = FindAnyObjectByType<QRManager>();
            webcamView = FindAnyObjectByType<QREditorWebcamView>();

            if (qrManager == null)
            {
                Debug.LogWarning("QREditorZXingReader: シーン内に QRManager が見つかりません。");
            }
            if (webcamView == null)
            {
                Debug.LogWarning("QREditorZXingReader: QREditorWebcamView が見つかりません。");
            }

            barcodeReader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = { TryHarder = false, PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
            };
        }

        private void Update()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer < ScanIntervalSeconds)
            {
                return;
            }
            scanTimer = 0f;

            TryDecode();
        }

        private void TryDecode()
        {
            if (webcamView == null)
            {
                return;
            }

            Color32[] pixels = webcamView.GetCroppedPixels(out int width, out int height);
            if (pixels == null)
            {
                return;
            }

            byte[] rgba = ToRgbaBytes(pixels);
            var result = barcodeReader.Decode(rgba, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                OnDetected(result.Text);
            }
        }

        // ZXing.Net(netstandard版)はColor32[]を直接受け取れないため、RGBA32のバイト列に詰め替える
        private byte[] ToRgbaBytes(Color32[] pixels)
        {
            int byteCount = pixels.Length * 4;
            if (rgbaBuffer == null || rgbaBuffer.Length != byteCount)
            {
                rgbaBuffer = new byte[byteCount];
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                int o = i * 4;
                rgbaBuffer[o] = pixels[i].r;
                rgbaBuffer[o + 1] = pixels[i].g;
                rgbaBuffer[o + 2] = pixels[i].b;
                rgbaBuffer[o + 3] = pixels[i].a;
            }

            return rgbaBuffer;
        }

        private void OnDetected(string text)
        {
            float now = Time.unscaledTime;
            bool isSameAsLast = text == lastText;
            if (isSameAsLast && (now - lastSentAt) < DebounceSeconds)
            {
                return;
            }

            lastText = text;
            lastSentAt = now;

            if (qrManager != null)
            {
                qrManager.OnQRCodeScanned(text);
            }
        }
    }
#endif
}
