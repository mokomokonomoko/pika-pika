using System.Collections.Generic;
using UnityEngine;

namespace CardScanner
{
#if UNITY_EDITOR
    using UnityEditor;

    // Unityエディタ専用のツールウィンドウ。Gameビューの解像度に関係なく、
    // TEST123 / CARD:0001〜0008 / 自由入力でカード読み取りをシミュレートできる。
    // QRManager.OnQRCodeScanned を直接呼ぶため、実機のJS経由と同じ送信経路を通る。
    // WebGLビルドには含まれない(クラス全体がUNITY_EDITOR)。
    public class QRCardScannerWindow : EditorWindow
    {
        private const int MaxHistory = 10;

        [SerializeField] private string freeInputText = "";
        [SerializeField] private List<string> history = new List<string>();
        private Vector2 historyScroll;

        [MenuItem("Window/Card Scanner/Simulator")]
        private static void Open()
        {
            GetWindow<QRCardScannerWindow>("Card Scanner");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("カード読み取りシミュレーター", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Play中のみ送信できます。", MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                EditorGUILayout.Space();
                foreach (var card in QRCardScannerSimulator.PresetCards)
                {
                    if (GUILayout.Button(card))
                    {
                        Send(card);
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("自由入力");
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName(QRCardScannerSimulator.FreeInputControlName);
                freeInputText = EditorGUILayout.TextField(freeInputText);
                bool canSend = !string.IsNullOrEmpty(freeInputText);
                using (new EditorGUI.DisabledScope(!canSend))
                {
                    if (GUILayout.Button("送信", GUILayout.Width(60)))
                    {
                        Send(freeInputText);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField($"送信履歴(直近{MaxHistory}件)", EditorStyles.boldLabel);
            historyScroll = EditorGUILayout.BeginScrollView(historyScroll);
            for (int i = history.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.LabelField(history[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        private void Send(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var qrManager = FindAnyObjectByType<QRManager>();
            if (qrManager == null)
            {
                Debug.LogWarning("QRCardScannerWindow: シーン内に QRManager が見つからないため送信できません。");
                return;
            }

            qrManager.OnQRCodeScanned(text);
            AddHistory(text);
        }

        private void AddHistory(string text)
        {
            history.Add($"{System.DateTime.Now:HH:mm:ss}  {text}");
            while (history.Count > MaxHistory)
            {
                history.RemoveAt(0);
            }
            Repaint();
        }
    }
#endif
}
