using UnityEngine;

namespace CardScanner
{
    // QRコードの文字列("CARD:0001"など)からカードIDへ変換する。
    // 実機(WebGL)・エディタの両方で使われるため UNITY_EDITOR では囲まない。
    public static class CardIdParser
    {
        private const string Prefix = "CARD:";
        private const int NumberDigits = 4;

        // "CARD:0001" -> true, cardId = 1
        // 形式が違う場合は false を返し、Console に警告を出す。
        public static bool TryParse(string raw, out int cardId)
        {
            cardId = 0;

            if (string.IsNullOrEmpty(raw) || !raw.StartsWith(Prefix, System.StringComparison.Ordinal))
            {
                Debug.LogWarning($"CardIdParser: 形式が不正なQRコードです(先頭が '{Prefix}' ではありません): {raw}");
                return false;
            }

            string numberPart = raw.Substring(Prefix.Length);
            if (numberPart.Length != NumberDigits)
            {
                Debug.LogWarning($"CardIdParser: 形式が不正なQRコードです(番号が{NumberDigits}桁ではありません): {raw}");
                return false;
            }

            for (int i = 0; i < numberPart.Length; i++)
            {
                if (!char.IsDigit(numberPart[i]))
                {
                    Debug.LogWarning($"CardIdParser: 形式が不正なQRコードです(番号部分が数字ではありません): {raw}");
                    return false;
                }
            }

            cardId = int.Parse(numberPart);
            return true;
        }
    }
}
