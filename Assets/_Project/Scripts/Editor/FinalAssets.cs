using System;
using System.Collections.Generic;
using System.IO;
using Starfall.Audio;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// Drop-in integration of FINAL art and audio (docs/ASSET_SPEC.md). Anything found under
    /// Assets/_Project/Art/Final or Assets/_Project/Audio/Final replaces the corresponding placeholder when the
    /// bootstrap runs; anything missing keeps the procedural placeholder. No code changes are needed per asset.
    ///
    /// Sprites are matched by file name (case-insensitive, any subfolder) and imported so they occupy the same
    /// world size as the placeholder they replace: pixels-per-unit = 100 * width / placeholderCanvas.
    /// </summary>
    public static class FinalAssets
    {
        public const string ArtRoot = "Assets/_Project/Art/Final";
        public const string AudioRoot = "Assets/_Project/Audio/Final";
        public const string FontRoot = ArtRoot + "/Fonts";
        public const string IconRoot = ArtRoot + "/Icon";

        private static readonly HashSet<Sprite> FinalSprites = new HashSet<Sprite>();
        private static readonly HashSet<string> FinalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _index;

        public static int SpritesReplaced => FinalSprites.Count;

        // ---- Art -----------------------------------------------------------------------------------------

        /// <summary>Loads the final sprite for a placeholder name, or returns false when the artist has not delivered it.</summary>
        public static bool TryLoadSprite(string name, int placeholderCanvas, bool fullRect, out Sprite sprite)
        {
            sprite = null;
            string path = FindArt(name);
            if (path == null) return false;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int width = tex != null ? tex.width : placeholderCanvas;
            int ppu = Mathf.Max(1, Mathf.RoundToInt(100f * width / Mathf.Max(1, placeholderCanvas)));

            bool dirty = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single
                         || Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.5f || importer.filterMode != FilterMode.Point
                         || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.mipmapEnabled;
            if (dirty)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point; // pixel art
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) return false;
            FinalSprites.Add(sprite);
            FinalNames.Add(name);
            Debug.Log($"[Starfall] Final art: {name} <- {path} (ppu {ppu})");
            return true;
        }

        public static bool IsFinal(Sprite sprite) => sprite != null && FinalSprites.Contains(sprite);
        public static bool IsFinal(string name) => FinalNames.Contains(name);

        /// <summary>Placeholders are white and tinted at runtime; final art is already coloured, so its tint is white.</summary>
        public static Color Tint(Sprite sprite, Color placeholderTint) => IsFinal(sprite) ? Color.white : placeholderTint;

        private static string FindArt(string name)
        {
            if (_index == null)
            {
                _index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (Directory.Exists(ArtRoot))
                {
                    foreach (var file in Directory.GetFiles(ArtRoot, "*.png", SearchOption.AllDirectories))
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        string rel = file.Replace('\\', '/');
                        if (rel.StartsWith(FontRoot, StringComparison.OrdinalIgnoreCase) || rel.StartsWith(IconRoot, StringComparison.OrdinalIgnoreCase)) continue;
                        if (!_index.ContainsKey(key)) _index[key] = rel;
                        else Debug.LogWarning($"[Starfall] Final art: duplicated name '{key}' ({rel} ignored, using {_index[key]})");
                    }
                }
            }
            return _index.TryGetValue(name, out var path) ? path : null;
        }

        public static void ResetCache()
        {
            _index = null;
            FinalSprites.Clear();
            FinalNames.Clear();
        }

        // ---- Font --------------------------------------------------------------------------------------

        /// <summary>First .ttf/.otf under Art/Final/Fonts becomes the UI font (a TMP font asset is generated next to it).</summary>
        public static TMP_FontAsset TryLoadFont()
        {
            if (!Directory.Exists(FontRoot)) return null;
            foreach (var file in Directory.GetFiles(FontRoot))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".ttf" && ext != ".otf") continue;
                string fontPath = file.Replace('\\', '/');
                string assetPath = Path.ChangeExtension(fontPath, null) + " SDF.asset";
                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                if (existing != null) return existing;
                var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (font == null) continue;
                var asset = TMP_FontAsset.CreateFontAsset(font, 64, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
                if (asset == null) continue;
                AssetDatabase.CreateAsset(asset, assetPath);
                if (asset.material != null) AssetDatabase.AddObjectToAsset(asset.material, asset);
                if (asset.atlasTexture != null) AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Starfall] Final font: {fontPath} -> {assetPath}");
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            }
            return null;
        }

        // ---- Icon --------------------------------------------------------------------------------------

        /// <summary>Art/Final/Icon/icon.png (1024x1024, opaque) becomes the app icon for every platform.</summary>
        public static bool TryApplyIcon()
        {
            string path = IconRoot + "/icon.png";
            if (!File.Exists(path)) return false;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Default || importer.isReadable == false))
            {
                importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) return false;
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { tex });
            Debug.Log($"[Starfall] App icon applied from {path}");
            return true;
        }

        // ---- Audio -----------------------------------------------------------------------------------------

        /// <summary>
        /// Fills the AudioLibrary from Audio/Final: SFX/&lt;SfxId&gt;[_n].wav|ogg, Music/&lt;MusicId&gt;.ogg, Ambient/&lt;AmbientId&gt;.ogg.
        /// Slots without files stay empty and fall back to the synthesized placeholder at runtime.
        /// </summary>
        public static int FillAudioLibrary(AudioLibrary library)
        {
            if (library == null || !Directory.Exists(AudioRoot)) return 0;
            int found = 0;

            var sfx = new List<AudioLibrary.SfxEntry>();
            foreach (SfxId id in Enum.GetValues(typeof(SfxId)))
            {
                var clips = LoadClips(AudioRoot + "/SFX", id.ToString(), false);
                if (clips.Count == 0) continue;
                sfx.Add(new AudioLibrary.SfxEntry { Id = id, Clips = clips.ToArray(), Volume = 1f });
                found += clips.Count;
            }
            library.Sfx = sfx.ToArray();

            var music = new List<AudioLibrary.MusicEntry>();
            foreach (MusicId id in Enum.GetValues(typeof(MusicId)))
            {
                if (id == MusicId.None) continue;
                var clips = LoadClips(AudioRoot + "/Music", id.ToString(), true);
                if (clips.Count == 0) continue;
                music.Add(new AudioLibrary.MusicEntry { Id = id, Clip = clips[0], Volume = 1f });
                found++;
            }
            library.Music = music.ToArray();

            var ambient = new List<AudioLibrary.AmbientEntry>();
            foreach (AmbientId id in Enum.GetValues(typeof(AmbientId)))
            {
                if (id == AmbientId.None) continue;
                var clips = LoadClips(AudioRoot + "/Ambient", id.ToString(), true);
                if (clips.Count == 0) continue;
                ambient.Add(new AudioLibrary.AmbientEntry { Id = id, Clip = clips[0], Volume = 1f });
                found++;
            }
            library.Ambient = ambient.ToArray();

            EditorUtility.SetDirty(library);
            if (found > 0) Debug.Log($"[Starfall] Final audio: {found} clip(s) wired into AudioLibrary");
            return found;
        }

        private static List<AudioClip> LoadClips(string folder, string id, bool streaming)
        {
            var result = new List<AudioClip>();
            if (!Directory.Exists(folder)) return result;
            var files = new List<string>();
            foreach (var f in Directory.GetFiles(folder))
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext != ".wav" && ext != ".ogg" && ext != ".mp3") continue;
                string stem = Path.GetFileNameWithoutExtension(f);
                // "Laser", "Laser_1", "Laser_2" all belong to SfxId.Laser
                if (stem.Equals(id, StringComparison.OrdinalIgnoreCase) || stem.StartsWith(id + "_", StringComparison.OrdinalIgnoreCase))
                    files.Add(f.Replace('\\', '/'));
            }
            files.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (var path in files)
            {
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer != null)
                {
                    var s = importer.defaultSampleSettings;
                    var wanted = streaming ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                    if (s.loadType != wanted || importer.forceToMono != !streaming)
                    {
                        s.loadType = wanted;
                        s.compressionFormat = AudioCompressionFormat.Vorbis;
                        s.quality = streaming ? 0.7f : 0.8f;
                        importer.defaultSampleSettings = s;
                        importer.forceToMono = !streaming;
                        importer.SaveAndReimport();
                    }
                }
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) result.Add(clip);
            }
            return result;
        }
    }
}
