using UnityEngine;
using UnityEngine.InputSystem;

namespace CardScanner
{
#if UNITY_EDITOR
    // Unityエディタの Play モード専用のカード読み取りシミュレーター。
    // QRManager.OnQRCodeScanned を直接呼ぶため、ゲーム側からは実機のJS経由との区別がつかない。
    // クラス全体が UNITY_EDITOR で囲まれているため、WebGLビルドには含まれない。
    public class QRCardScannerSimulator : MonoBehaviour
    {
        // QRCardScannerWindow の自由入力欄と同じ名前を使う(フォーカス中は数字キーを無効化するため)
        internal const string FreeInputControlName = "QRCardScanner_FreeInput";

        // [0] は TEST123、[1]以降が CARD:0001〜CARD:0030(カード枚数を増やす場合はここに追加する)
        internal static readonly string[] PresetCards = BuildPresetCards();

        private static string[] BuildPresetCards()
        {
            const int cardCount = 30;
            var cards = new string[cardCount + 1];
            cards[0] = "TEST123";
            for (int i = 1; i <= cardCount; i++)
            {
                cards[i] = $"CARD:{i:D4}";
            }
            return cards;
        }

        // このプロジェクトは Active Input Handling が "Input System Package" のため、
        // レガシーな UnityEngine.Input ではなく新しい Input System を使う。
        private static readonly Key[] DigitKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
            Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8,
        };

        private QRManager qrManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<QRCardScannerSimulator>() != null)
            {
                return;
            }

            var go = new GameObject("QRCardScannerSimulator (Editor Only)");
            go.AddComponent<QRCardScannerSimulator>();
        }

        private void Start()
        {
            qrManager = FindAnyObjectByType<QRManager>();
            if (qrManager == null)
            {
                Debug.LogWarning("QRCardScannerSimulator: シーン内に QRManager が見つかりません。");
            }
        }

        private void Update()
        {
            if (GUI.GetNameOfFocusedControl() == FreeInputControlName)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            for (int i = 0; i < DigitKeys.Length; i++)
            {
                if (keyboard[DigitKeys[i]].wasPressedThisFrame)
                {
                    Send(PresetCards[i + 1]); // PresetCards[0] は TEST123 なので +1
                }
            }
        }

        private void Send(string text)
        {
            if (qrManager == null)
            {
                qrManager = FindAnyObjectByType<QRManager>();
            }

            if (qrManager != null)
            {
                qrManager.OnQRCodeScanned(text);
            }
            else
            {
                Debug.LogWarning("QRCardScannerSimulator: QRManager が見つからないため送信できません。");
            }
        }
    }
#endif
}
