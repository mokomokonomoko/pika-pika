using UnityEditor;

namespace UI.Editor
{
    // Assets/UI/parts/ と Assets/UI/character/ の画像を自動でSprite化し、
    // WebGL向けの圧縮設定を適用する(Assets/Cards, Assets/UI/Backgrounds と同じ考え方)。
    public class UIAssetPostprocessor : AssetPostprocessor
    {
        private static readonly string[] TargetFolders =
        {
            "Assets/UI/parts/",
            "Assets/UI/character/",
        };

        private const int MaxTextureSize = 1024;

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            bool isTarget = false;
            foreach (var folder in TargetFolders)
            {
                if (path.StartsWith(folder))
                {
                    isTarget = true;
                    break;
                }
            }
            if (!isTarget)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
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
