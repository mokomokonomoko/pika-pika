using UnityEngine;
using UnityEngine.UI;

namespace CardScanner
{
#if UNITY_EDITOR
    // Unityエディタの Play モード専用。Macのカメラをスマホと同じ枠(QRManager.CameraRect)の
    // 位置・大きさでRawImageに表示する。WebGLビルドには含まれない(クラス全体がUNITY_EDITOR)。
    public class QREditorWebcamView : MonoBehaviour
    {
        private QRManager qrManager;
        private WebCamTexture webCamTexture;
        private RawImage rawImage;
        private RectTransform rawImageRect;
        private RectInt cropPixelRect;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<QREditorWebcamView>() != null)
            {
                return;
            }

            var go = new GameObject("QREditorWebcamView (Editor Only)");
            go.AddComponent<QREditorWebcamView>();
        }

        private void Start()
        {
            qrManager = FindAnyObjectByType<QRManager>();
            if (qrManager == null)
            {
                Debug.LogWarning("QREditorWebcamView: シーン内に QRManager が見つかりません。");
            }

            BuildUi();

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogWarning("QREditorWebcamView: 利用できるカメラが見つかりません。");
                return;
            }

            webCamTexture = new WebCamTexture();
            rawImage.texture = webCamTexture;
            webCamTexture.Play();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            var imageGo = new GameObject("WebcamRawImage");
            imageGo.transform.SetParent(canvasGo.transform, false);
            rawImage = imageGo.AddComponent<RawImage>();
            rawImage.color = Color.white;

            rawImageRect = rawImage.rectTransform;
            rawImageRect.anchorMin = new Vector2(0f, 1f);
            rawImageRect.anchorMax = new Vector2(0f, 1f);
            rawImageRect.pivot = new Vector2(0f, 1f);
        }

        private void Update()
        {
            if (rawImageRect == null)
            {
                return;
            }

            Rect rect = qrManager != null ? qrManager.CameraRect : new Rect(0.3f, 0.5f, 0.4f, 0.4f);

            float widthPx = rect.width * Screen.width;
            float heightPx = rect.height * Screen.width; // 幅基準にして、スマホ側(JS)と同じ正方形の規約を保つ
            float leftPx = rect.x * Screen.width;
            float topPx = rect.y * Screen.height;

            rawImageRect.sizeDelta = new Vector2(widthPx, heightPx);
            rawImageRect.anchoredPosition = new Vector2(leftPx, -topPx);

            UpdateCrop(widthPx, heightPx);
        }

        // RawImageはデフォルトでテクスチャを枠いっぱいに引き伸ばすため、
        // 縦横比を保ったまま枠に合わせて切り取る(CSSのobject-fit: coverと同じ考え方)。
        // ここで計算した切り取り範囲(cropPixelRect)は、QREditorZXingReaderのQR読み取りにも
        // そのまま使われるので、「画面に表示されている範囲だけを読み取る」が自然に成立する。
        private void UpdateCrop(float boxWidthPx, float boxHeightPx)
        {
            if (webCamTexture == null || webCamTexture.width <= 16 || boxWidthPx <= 0f || boxHeightPx <= 0f)
            {
                return;
            }

            int texW = webCamTexture.width;
            int texH = webCamTexture.height;
            float texAspect = (float)texW / texH;
            float boxAspect = boxWidthPx / boxHeightPx;

            int cropW, cropH, cropX, cropY;
            if (texAspect > boxAspect)
            {
                cropH = texH;
                cropW = Mathf.RoundToInt(cropH * boxAspect);
                cropX = (texW - cropW) / 2;
                cropY = 0;
            }
            else
            {
                cropW = texW;
                cropH = Mathf.RoundToInt(cropW / boxAspect);
                cropX = 0;
                cropY = (texH - cropH) / 2;
            }

            cropPixelRect = new RectInt(cropX, cropY, cropW, cropH);
            rawImage.uvRect = new Rect(
                (float)cropX / texW,
                (float)cropY / texH,
                (float)cropW / texW,
                (float)cropH / texH);
        }

        // 現在表示中のクロップ範囲のピクセルを返す(QR読み取り用)。表示できていない場合はnull。
        public Color32[] GetCroppedPixels(out int width, out int height)
        {
            width = 0;
            height = 0;

            if (webCamTexture == null || webCamTexture.width <= 16 || cropPixelRect.width <= 0 || cropPixelRect.height <= 0)
            {
                return null;
            }

            int texW = webCamTexture.width;
            int texH = webCamTexture.height;
            Color32[] full = webCamTexture.GetPixels32();

            int cropX = Mathf.Clamp(cropPixelRect.x, 0, texW - 1);
            int cropY = Mathf.Clamp(cropPixelRect.y, 0, texH - 1);
            int cropW = Mathf.Clamp(cropPixelRect.width, 1, texW - cropX);
            int cropH = Mathf.Clamp(cropPixelRect.height, 1, texH - cropY);

            var cropped = new Color32[cropW * cropH];
            for (int row = 0; row < cropH; row++)
            {
                int srcRowStart = (cropY + row) * texW + cropX;
                int dstRowStart = row * cropW;
                System.Array.Copy(full, srcRowStart, cropped, dstRowStart, cropW);
            }

            width = cropW;
            height = cropH;
            return cropped;
        }

        private void OnDestroy()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
        }
    }
#endif
}
