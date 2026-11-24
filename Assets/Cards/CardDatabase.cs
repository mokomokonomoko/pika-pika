using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    // カードID(整数)とSpriteの対応表。
    // Assets/Cards/Editor/CardDatabaseImporter.cs の「Rebuild Card Database」で自動生成・更新する。
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Card Scanner/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public int id;
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<int, Sprite> lookup;

        private void BuildLookupIfNeeded()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<int, Sprite>(entries.Count);
            foreach (var entry in entries)
            {
                lookup[entry.id] = entry.sprite;
            }
        }

        // カードIDから画像を取得する。見つからなければ null を返し、Console に警告を出す。
        public Sprite GetSprite(int cardId)
        {
            BuildLookupIfNeeded();

            if (lookup.TryGetValue(cardId, out var sprite) && sprite != null)
            {
                return sprite;
            }

            Debug.LogWarning($"CardDatabase: カードID CARD:{cardId:D4} の画像が登録されていません。");
            return null;
        }
    }
}
