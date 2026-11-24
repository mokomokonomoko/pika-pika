using UnityEditor;

namespace Cards.Editor
{
    // Assets/Cards/Images/ 配下の画像を(再)インポートするたびに、
    // Sprite用のインポート設定とWebGL向けの圧縮設定を自動で適用する。
    public class CardImagePostprocessor : AssetPostprocessor
    {
        private const string TargetFolder = "Assets/Cards/Images/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(TargetFolder))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            var webglSettings = importer.GetPlatformTextureSettings("WebGL");
            webglSettings.overridden = true;
            webglSettings.maxTextureSize = 1024;
            webglSettings.format = TextureImporterFormat.Automatic;
            webglSettings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(webglSettings);
        }
    }
}
