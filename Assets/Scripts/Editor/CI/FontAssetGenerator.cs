using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Mojipet.Editor.CI
{
    // Regenerates the embedded Japanese TMP font asset from the source .ttf.
    // Runtime OS font lookup (Font.CreateDynamicFontFromOSFont) is unreliable
    // on iOS/Android builds -- it works in the Windows Editor but silently
    // fails to produce a font on device, which is why Japanese text rendered
    // as tofu/mojibake on iPhone. A prebaked dynamic-atlas font asset sourced
    // from an embedded font file works on every platform without depending
    // on the OS having a matching font installed.
    public static class FontAssetGenerator
    {
        private const string SourceFontPath = "Assets/Fonts/NotoSansJP-Regular.ttf";
        private const string OutputAssetPath = "Assets/Fonts/Resources/NotoSansJP-Regular SDF.asset";

        [MenuItem("Tools/Generate Japanese Font Asset")]
        public static void Generate()
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
            {
                Debug.LogError($"Source font not found at {SourceFontPath}");
                return;
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);

            if (fontAsset == null)
            {
                Debug.LogError("Failed to create TMP font asset from source font.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(OutputAssetPath)!);

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputAssetPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(OutputAssetPath);
            }

            AssetDatabase.CreateAsset(fontAsset, OutputAssetPath);

            // CreateFontAsset() builds the atlas texture(s) and material in memory
            // only; they must be explicitly persisted as sub-assets of the font
            // asset file (this is what the Font Asset Creator window's own Save
            // routine does), otherwise the atlas texture reference is left
            // unassigned once Unity reloads the asset -- which is what caused
            // TMP_PreBuildProcessor's UnassignedReferenceException on m_AtlasTextures.
            if (fontAsset.atlasTextures != null)
            {
                foreach (var texture in fontAsset.atlasTextures)
                {
                    if (texture != null)
                    {
                        AssetDatabase.AddObjectToAsset(texture, fontAsset);
                    }
                }
            }

            if (fontAsset.material != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated Japanese font asset at {OutputAssetPath}");
        }
    }
}
