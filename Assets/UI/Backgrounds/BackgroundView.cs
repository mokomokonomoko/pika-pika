using UnityEngine;
using UnityEngine.UI;

namespace UI.Backgrounds
{
    // 画面全体を覆う背景画像。AspectRatioFitter(Envelope Parent)で
    // 縦横比を保ったまま画面を覆い、はみ出た部分は切り取る(CSSのbackground-size: coverと同じ)。
    // 画像を差し替えるときは Image の Sprite を変えるだけでよいように、
    // 縦横比はここで Sprite から自動取得して設定する。
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(AspectRatioFitter))]
    public class BackgroundView : MonoBehaviour
    {
        private void Awake()
        {
            ApplyAspectRatioFromSprite();
        }

        private void ApplyAspectRatioFromSprite()
        {
            var image = GetComponent<Image>();
            var fitter = GetComponent<AspectRatioFitter>();

            if (image.sprite == null || image.sprite.rect.height <= 0)
            {
                return;
            }

            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        }
    }
}
