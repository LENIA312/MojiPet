using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Mojipet.Editor.CI
{
    // Regenerates the embedded Japanese + emoji TMP font assets from their
    // source .ttf files. Runtime OS font lookup (Font.CreateDynamicFontFromOSFont)
    // is unreliable on iOS/Android builds -- it works in the Windows Editor but
    // silently fails to produce a font on device, which is why Japanese text
    // rendered as tofu/mojibake on iPhone. Prebaked dynamic-atlas font assets
    // sourced from embedded font files work on every platform without depending
    // on the OS having a matching font installed.
    //
    // Emoji (e.g. status icons over character heads) need a separate font: Noto
    // Sans JP has no emoji glyphs, and color emoji fonts aren't renderable via
    // regular TMP SDF atlases. Noto Emoji is a monochrome/outline emoji font,
    // which *is* SDF-renderable, so it's wired in as a fallback on the Japanese
    // font asset -- any character missing from Noto Sans JP is looked up there.
    public static class FontAssetGenerator
    {
        private const string JapaneseSourcePath = "Assets/Fonts/NotoSansJP-Regular.ttf";
        private const string JapaneseOutputPath = "Assets/Fonts/Resources/NotoSansJP-Regular SDF.asset";
        private const string EmojiSourcePath = "Assets/Fonts/NotoEmoji-Regular.ttf";
        private const string EmojiOutputPath = "Assets/Fonts/Resources/NotoEmoji-Regular SDF.asset";

        [MenuItem("Tools/Generate Japanese Font Asset")]
        public static void Generate()
        {
            var emojiFont = CreateAndSaveFontAsset(EmojiSourcePath, EmojiOutputPath);
            var japaneseFont = CreateAndSaveFontAsset(JapaneseSourcePath, JapaneseOutputPath);

            if (japaneseFont == null)
            {
                return;
            }

            if (emojiFont != null)
            {
                japaneseFont.fallbackFontAssetTable = new List<TMP_FontAsset> { emojiFont };
                EditorUtility.SetDirty(japaneseFont);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
            Debug.Log("Font asset generation complete.");
        }

        private static TMP_FontAsset CreateAndSaveFontAsset(string sourceFontPath, string outputAssetPath)
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            if (sourceFont == null)
            {
                Debug.LogError($"Source font not found at {sourceFontPath}");
                return null;
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
                Debug.LogError($"Failed to create TMP font asset from {sourceFontPath}.");
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputAssetPath)!);

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputAssetPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(outputAssetPath);
            }

            AssetDatabase.CreateAsset(fontAsset, outputAssetPath);

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

            Debug.Log($"Generated font asset at {outputAssetPath}");
            return fontAsset;
        }
    }
}
