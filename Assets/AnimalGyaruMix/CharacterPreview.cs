using UnityEngine;
using UnityEngine.UI;

namespace AnimalGyaruMix
{
    // Sprites share a transparent artboard so parts stay registered when swapped.
    public sealed class CharacterPreview : MonoBehaviour
    {
        [SerializeField] private Image baseBody;
        [SerializeField] private Image[] slotImages;
        [SerializeField] private RawImage photoBackground;

        private RectTransform artwork;
        private Vector2 restPosition;
        private Quaternion restRotation;
        private Vector3 restScale;

        internal void SetRevealPose(float weight)
        {
            if (artwork == null)
            {
                if (weight == 0f)
                    return;

                artwork = (RectTransform)baseBody.transform.parent;
                restPosition = artwork.anchoredPosition;
                restRotation = artwork.localRotation;
                restScale = artwork.localScale;
            }

            // Lean and lift the existing sprite layers together until pose art is available.
            artwork.anchoredPosition = restPosition + Vector2.up * (artwork.rect.height * .04f * weight);
            artwork.localRotation = restRotation * Quaternion.Euler(0, 0, -10f * weight);
            artwork.localScale = restScale * (1f + .06f * weight);
        }

        public void SetLook(CardDefinition[] cards)
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                Image image = slotImages[i];
                image.sprite = i < cards.Length ? cards[i]?.Sprite : null;
                image.enabled = image.sprite != null;
            }
        }

        public void SetPhoto(Texture2D photo)
        {
            if (photoBackground == null)
                return;

            photoBackground.texture = photo;
            photoBackground.enabled = photo != null;
        }

        public string ExportOverlay()
        {
            const int size = 256;
            var composite = new Color[size * size];
            var pixels = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            var target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);

            try
            {
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                // Walk the shared artboard in Canvas draw order, including the fixed body.
                foreach (Transform child in baseBody.transform.parent)
                {
                    if (!child.TryGetComponent(out Image layer) || !layer.isActiveAndEnabled || layer.sprite == null)
                        continue;

                    Sprite sprite = layer.sprite;
                    Texture2D texture = sprite.texture;
                    Rect rect = sprite.textureRect;
                    var scale = new Vector2(rect.width / texture.width, rect.height / texture.height);
                    var offset = new Vector2(rect.x / texture.width, rect.y / texture.height);
                    Graphics.Blit(texture, target, scale, offset);
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
                    BlendLayer(composite, pixels.GetPixels32());
                }

                pixels.SetPixels(composite);
                pixels.Apply(false);
                return System.Convert.ToBase64String(pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                GL.sRGBWrite = previousSrgbWrite;
                RenderTexture.ReleaseTemporary(target);
                Destroy(pixels);
            }
        }

        private static void BlendLayer(Color[] composite, Color32[] layer)
        {
            for (int i = 0; i < composite.Length; i++)
            {
                Color front = layer[i];
                if (front.a <= 0f)
                    continue;

                Color back = composite[i];
                float backWeight = back.a * (1f - front.a);
                float alpha = front.a + backWeight;
                Color mixed = (front * front.a + back * backWeight) / alpha;
                mixed.a = alpha;
                composite[i] = mixed;
            }
        }
    }
}
