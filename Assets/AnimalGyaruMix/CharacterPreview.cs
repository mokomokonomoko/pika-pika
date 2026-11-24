using UnityEngine;
using UnityEngine.UI;

namespace AnimalGyaruMix
{
    // Sprites share a transparent artboard so parts stay registered when swapped.
    public sealed class CharacterPreview : MonoBehaviour
    {
        [SerializeField] private Image[] slotImages;
        [SerializeField] private RawImage photoBackground;

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
            const int width = 192;
            const int height = 224;
            var composite = new Color[width * height];
            foreach (Image layer in GetComponentsInChildren<Image>())
            {
                if (!layer.enabled || layer.sprite == null)
                    continue;

                Sprite sprite = layer.sprite;
                Texture2D texture = sprite.texture;
                if (!texture.isReadable)
                    return "";

                Color[] source = texture.GetPixels();
                Rect rect = sprite.textureRect;
                for (int y = 0; y < height; y++)
                {
                    int row = ((int)rect.y + y * (int)rect.height / height) * texture.width;
                    for (int x = 0; x < width; x++)
                    {
                        int pixel = y * width + x;
                        Color front = source[row + (int)rect.x + x * (int)rect.width / width];
                        Color back = composite[pixel];
                        float alpha = front.a + back.a * (1f - front.a);
                        if (alpha > 0)
                        {
                            Color mixed = (front * front.a + back * (back.a * (1f - front.a))) / alpha;
                            mixed.a = alpha;
                            composite[pixel] = mixed;
                        }
                    }
                }
            }

            var overlay = new Texture2D(width, height, TextureFormat.RGBA32, false);
            overlay.SetPixels(composite);
            overlay.Apply();
            string png = System.Convert.ToBase64String(overlay.EncodeToPNG());
            Destroy(overlay);
            return png;
        }
    }
}
