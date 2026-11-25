using UnityEngine;

namespace UI
{
    // 親の中に収まる最大の矩形(指定した縦横比を保つ)になるよう、毎フレーム自分のサイズを合わせる。
    // AspectRatioFitter(FitInParent)と同じ目的だが、エディタでのシーン保存タイミングに
    // 左右されてサイズ0のまま保存されてしまう問題を避けるため、自前で計算する。
    [RequireComponent(typeof(RectTransform))]
    public class FitAreaToParent : MonoBehaviour
    {
        public float aspectWidth = 1f;
        public float aspectHeight = 1f;

        private RectTransform rt;
        private RectTransform parent;

        private void Awake()
        {
            rt = (RectTransform)transform;
            parent = transform.parent as RectTransform;
        }

        private void Update()
        {
            if (parent == null || aspectHeight <= 0f)
            {
                return;
            }

            float parentW = parent.rect.width;
            float parentH = parent.rect.height;
            if (parentW <= 0f || parentH <= 0f)
            {
                return;
            }

            float targetAspect = aspectWidth / aspectHeight;
            float parentAspect = parentW / parentH;

            float w, h;
            if (parentAspect > targetAspect)
            {
                h = parentH;
                w = h * targetAspect;
            }
            else
            {
                w = parentW;
                h = w / targetAspect;
            }

            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
