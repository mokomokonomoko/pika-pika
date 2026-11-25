using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    // ボタンを押したときに少し縮む視覚フィードバックだけを行う(ゲームロジックは一切持たない)。
    // targetGraphicの色を変えられないボタン(例: ページ切り替え時に色が変わるタブ)で使う。
    public class IconButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float PressedScale = 0.92f;
        private RectTransform rt;

        private void Awake() => rt = (RectTransform)transform;

        public void OnPointerDown(PointerEventData eventData) => rt.localScale = Vector3.one * PressedScale;
        public void OnPointerUp(PointerEventData eventData) => rt.localScale = Vector3.one;
        public void OnPointerExit(PointerEventData eventData) => rt.localScale = Vector3.one;
    }
}
