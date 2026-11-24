using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardScanner
{
#if UNITY_EDITOR
    // The webcam preview uses the same normalized screen rectangle as the browser scanner.
    public class QREditorWebcamView : MonoBehaviour
    {
        private const float StartTimeout = 8f;
        private QRManager manager;
        private WebCamTexture webcam;
        private RawImage preview;
        private RectInt crop;
        private Color32[] framePixels;
        private Color32[] croppedPixels;
        private float openedAt;
        private bool ready;

        public bool Open(QRManager scanner)
        {
            Close();
            manager = scanner;
            if (WebCamTexture.devices.Length == 0)
            {
                manager.OnQRCameraStatus("No webcam detected. Connect a camera or check your system camera permissions.");
                return false;
            }

            if (preview == null)
                BuildPreview();

            try
            {
                webcam = new WebCamTexture(640, 480, 24);
                preview.texture = webcam;
                webcam.Play();
                openedAt = Time.unscaledTime;
                return true;
            }
            catch (UnityException)
            {
                ReportStartFailure();
                return false;
            }
        }

        private void BuildPreview()
        {
            var overlay = new GameObject("Webcam Overlay", typeof(Canvas));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var image = new GameObject("Live Preview", typeof(RectTransform));
            image.transform.SetParent(overlay.transform, false);
            preview = image.AddComponent<RawImage>();
            preview.raycastTarget = false;
            preview.enabled = false;
            preview.rectTransform.anchorMin = new Vector2(0, 1);
            preview.rectTransform.anchorMax = new Vector2(0, 1);
        }

        private void Update()
        {
            if (webcam == null)
                return;

            if (!ready && (!webcam.didUpdateThisFrame || webcam.width <= 16 || webcam.height <= 16))
            {
                if (Time.unscaledTime - openedAt >= StartTimeout)
                    ReportStartFailure();
                return;
            }

            UpdatePreview();
            if (!ready && crop.width > 0 && crop.height > 0)
            {
                ready = true;
                preview.enabled = true;
                manager.OnQRCameraStatus("ready");
            }
        }

        private void UpdatePreview()
        {
            Rect rect = manager.CameraRect;
            float maxSide = Mathf.Max(rect.width, rect.height);
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            float scale = Mathf.Min(Screen.width, Screen.height * 0.64f / maxSide);
            float width = rect.width * scale;
            float height = rect.height * scale;
            int angle = webcam.videoRotationAngle;
            var size = angle % 180 == 0 ? new Vector2(width, height) : new Vector2(height, width);
            RectTransform frame = preview.rectTransform;
            frame.sizeDelta = size;
            frame.anchoredPosition = new Vector2(
                (rect.x + rect.width / 2) * Screen.width, -(rect.y * Screen.height + height / 2));
            frame.localEulerAngles = new Vector3(0, 0, -angle);

            int videoWidth = webcam.width;
            int videoHeight = webcam.height;
            float frameAspect = size.x / size.y;
            int cropWidth = Mathf.Clamp(Mathf.RoundToInt(videoHeight * frameAspect), 1, videoWidth);
            int cropHeight = Mathf.Clamp(Mathf.RoundToInt(videoWidth / frameAspect), 1, videoHeight);
            crop = new RectInt((videoWidth - cropWidth) / 2, (videoHeight - cropHeight) / 2, cropWidth, cropHeight);
            var uv = new Rect((float)crop.x / videoWidth, (float)crop.y / videoHeight,
                (float)crop.width / videoWidth, (float)crop.height / videoHeight);
            if (webcam.videoVerticallyMirrored)
            {
                uv.y += uv.height;
                uv.height = -uv.height;
            }

            preview.uvRect = uv;
        }

        // The reader receives only pixels within the visible frame.
        public Color32[] GetCroppedPixels(out int width, out int height)
        {
            width = crop.width;
            height = crop.height;
            if (!ready || webcam == null)
                return null;

            int videoWidth = webcam.width;
            int videoHeight = webcam.height;
            if (crop.xMax > videoWidth || crop.yMax > videoHeight)
                return null;

            if (framePixels == null || framePixels.Length != videoWidth * videoHeight)
                framePixels = new Color32[videoWidth * videoHeight];
            webcam.GetPixels32(framePixels);
            if (croppedPixels == null || croppedPixels.Length != width * height)
                croppedPixels = new Color32[width * height];

            for (int row = 0; row < height; row++)
                Array.Copy(framePixels, (crop.y + row) * videoWidth + crop.x, croppedPixels, row * width, width);

            return croppedPixels;
        }

        public void Close()
        {
            ready = false;
            crop = default;
            if (preview != null)
            {
                preview.enabled = false;
                preview.texture = null;
            }

            if (webcam == null)
                return;

            webcam.Stop();
            Destroy(webcam);
            webcam = null;
            framePixels = null;
            croppedPixels = null;
        }

        private void ReportStartFailure()
        {
            Close();
            manager.OnQRCameraStatus("Couldn't get video. Check camera permissions and close other apps using the webcam.");
        }

        private void OnDisable() => Close();
    }
#endif
}
