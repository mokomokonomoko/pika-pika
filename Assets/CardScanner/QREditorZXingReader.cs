#if UNITY_EDITOR
using UnityEngine;
using ZXing;

namespace CardScanner
{
    // Decodes the Editor webcam preview and forwards the result through the same callback as WebGL.
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

        public void Begin(QRManager manager, QREditorWebcamView preview)
        {
            qrManager = manager;
            webcamView = preview;
            scanTimer = 0f;
            lastText = null;
            barcodeReader ??= new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = { TryHarder = false, PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
            };
            enabled = true;
        }

        private void Update()
        {
            scanTimer += Time.unscaledDeltaTime;
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
}
#endif
