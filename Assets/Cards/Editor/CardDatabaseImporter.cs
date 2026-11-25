using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Cards;

namespace Cards.Editor
{
    // Assets/Cards/Images/card_XXXX.png を走査して CardDatabase.asset を自動生成・更新するツール。
    // カードの枚数を増やす場合は ExpectedMinId/ExpectedMaxId を変更する。
    public static class CardDatabaseImporter
    {
        private const string ImagesFolder = "Assets/Cards/Images";
        private const string DatabasePath = "Assets/Cards/CardDatabase.asset";
        private const int ExpectedMinId = 1;
        private const int ExpectedMaxId = 30;

        private static readonly Regex FileNamePattern = new Regex(@"^card_(\d{4})\.png$", RegexOptions.IgnoreCase);

        [MenuItem("Tools/Card Scanner/Rebuild Card Database")]
        public static void RebuildCardDatabase()
        {
            if (!Directory.Exists(ImagesFolder))
            {
                EditorUtility.DisplayDialog("CardDatabase 更新", $"{ImagesFolder} が見つかりません。", "OK");
                return;
            }

            var files = Directory.GetFiles(ImagesFolder, "*.png", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray();

            var validEntries = new List<CardDatabase.Entry>();
            var seenIds = new HashSet<int>();
            var invalidNames = new List<string>();
            var duplicates = new List<string>();

            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                var match = FileNamePattern.Match(fileName);
                if (!match.Success)
                {
                    invalidNames.Add(fileName);
                    continue;
                }

                int id = int.Parse(match.Groups[1].Value);

                if (!seenIds.Add(id))
                {
                    duplicates.Add(fileName);
                    continue;
                }

                string assetPath = filePath.Replace('\\', '/');
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null)
                {
                    invalidNames.Add($"{fileName}(Spriteとしてインポートできていません。Texture Typeを確認してください)");
                    continue;
                }

                validEntries.Add(new CardDatabase.Entry { id = id, sprite = sprite });
            }

            var missingIds = Enumerable.Range(ExpectedMinId, ExpectedMaxId - ExpectedMinId + 1)
                .Where(id => !seenIds.Contains(id))
                .ToList();

            validEntries = validEntries.OrderBy(e => e.id).ToList();

            var database = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CardDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            var serializedObject = new SerializedObject(database);
            var entriesProperty = serializedObject.FindProperty("entries");
            entriesProperty.ClearArray();
            for (int i = 0; i < validEntries.Count; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                var element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("id").intValue = validEntries[i].id;
                element.FindPropertyRelative("sprite").objectReferenceValue = validEntries[i].sprite;
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            ShowReport(validEntries.Count, missingIds, duplicates, invalidNames);
        }

        private static void ShowReport(int registeredCount, List<int> missingIds, List<string> duplicates, List<string> invalidNames)
        {
            var report = new StringBuilder();
            report.AppendLine($"CardDatabase 更新完了: {registeredCount}件登録 ({DatabasePath})");

            if (missingIds.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("[不足] 画像が見つからない番号:");
                report.AppendLine(string.Join(", ", missingIds.Select(id => $"CARD:{id:D4}")));
            }

            if (duplicates.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("[重複] 番号が重複しているファイル:");
                report.AppendLine(string.Join("\n", duplicates));
            }

            if (invalidNames.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("[不正] 命名規則に合わないファイル:");
                report.AppendLine(string.Join("\n", invalidNames));
            }

            string message = report.ToString();
            Debug.Log(message);

            bool hasIssue = missingIds.Count > 0 || duplicates.Count > 0 || invalidNames.Count > 0;
            EditorUtility.DisplayDialog(
                hasIssue ? "CardDatabase 更新: 要確認あり" : "CardDatabase 更新完了",
                message,
                "OK");
        }
    }
}
