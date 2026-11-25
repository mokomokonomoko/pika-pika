using UnityEditor;

namespace UI.Backgrounds.Editor
{
    // Assets/UI/Backgrounds/ 配下の画像を(再)インポートするたびに、
    // Sprite用のインポート設定とWebGL向けの圧縮設定を自動で適用する。
    // (Assets/Cards/Editor/CardImagePostprocessor.cs と同じ考え方)
    public class BackgroundImagePostprocessor : AssetPostprocessor
    {
        private const string TargetFolder = "Assets/UI/Backgrounds/";

        // 画質の劣化を抑えつつ容量を減らすため、2048角の元画像を半分の1024に制限する。
        // 差し替え後に粗さが気になる場合はここを上げる。
        private const int MaxTextureSize = 1024;

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

            var webglSettings = importer.GetPlatformTextureSettings("WebGL");
            webglSettings.overridden = true;
            webglSettings.maxTextureSize = MaxTextureSize;
            webglSettings.format = TextureImporterFormat.Automatic;
            webglSettings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(webglSettings);
        }
    }
}
