using System;
using System.IO;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using UnityEditor;
using UnityEngine;

namespace Assets.HeroEditor4D.FantasyInventory.Editor
{
    /// <summary>
    /// Split multiple sprite, move pivots to center and crop if needed.
    /// </summary>
    public class SpriteImportSettings : EditorWindow
    {
        public UnityEngine.Object SpritesFolder;
        public bool ForceSingle;
        public bool ForceFullRect;
        public bool EnableReadWrite;
        public string PackingTag = "";
        public FilterMode FilterMode = FilterMode.Bilinear;
        public int MaxTextureSize = 2048;
        public TextureImporterCompression Compression = TextureImporterCompression.CompressedHQ;
        public bool CrunchedCompression = true;
        public int CompressionQuality = 100;

        [MenuItem("Window/Hippo/Sprite Import Settings")]
        public static void Init()
        {
            var window = GetWindow<SpriteImportSettings>(false, "Sprite Import Settings");

            window.minSize = window.maxSize = new Vector2(400, 240);
            window.Show();
        }

        public void OnEnable()
        {
            SpritesFolder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/FantasyHeroes/Sprites");
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("This tool is designed for handling sprite collection import settings.", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } });
            SpritesFolder = EditorGUILayout.ObjectField(new GUIContent("Sprites (Folder):", "This should be sprites root folder."), SpritesFolder, typeof(UnityEngine.Object), false);
            ForceSingle = EditorGUILayout.Toggle(new GUIContent("Set Sprite Mode To Single:", "Check to override sprite mode to Single (this may break a layout)."), ForceSingle);
            ForceFullRect = EditorGUILayout.Toggle(new GUIContent("Set Mesh Type = Full Rect:", "Check to override mesh @type to Full Rect."), ForceFullRect);
            EnableReadWrite = EditorGUILayout.Toggle(new GUIContent("Enable Read/Write:", ""), EnableReadWrite);
            PackingTag = EditorGUILayout.TextField(new GUIContent("Set Packing Tag:", ""), PackingTag);
            FilterMode = EditorGUILayout.TextField(new GUIContent("Filter Mode:", ""), FilterMode.ToString()).ToEnum<FilterMode>();
            MaxTextureSize = EditorGUILayout.IntField(new GUIContent("Texture size (32, 64...):", ""), MaxTextureSize);
            Compression = (TextureImporterCompression)EditorGUILayout.Popup(new GUIContent("Compression:", ""), (int) Compression, Enum.GetValues(typeof(TextureImporterCompression)).Cast<TextureImporterCompression>().Select(i => new GUIContent(i.ToString())).ToArray());
            CrunchedCompression = EditorGUILayout.Toggle(new GUIContent("Use Crunch Compression:", ""), CrunchedCompression);
            CompressionQuality = EditorGUILayout.IntSlider(new GUIContent("Compressor Quality:", ""), CompressionQuality, 0, 100);

            if (GUILayout.Button("Setup"))
            {
                if (SpritesFolder == null)
                {
                    Debug.LogWarning("SpritesFolder is null");
                }
                else
                {
                    var root = AssetDatabase.GetAssetPath(SpritesFolder);
                    var files = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories).Union(Directory.GetFiles(root, "*.psd", SearchOption.AllDirectories)).ToList();

                    for (var i = 0; i < files.Count; i++)
                    {
                        var progress = (float)i / files.Count;

                        SetImportSettings(files[i], ForceSingle, ForceFullRect, PackingTag, EnableReadWrite, FilterMode, MaxTextureSize, Compression, CrunchedCompression, CompressionQuality);

                        if (EditorUtility.DisplayCancelableProgressBar("Processing sprites", $"[{(int)(100 * progress)}%] [{i}/{files.Count}] Processing {files[i]}", progress))
                        {
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
        }

        public static void SetImportSettings(string path, bool forceSingle, bool forceFullRect, string packingTag, bool enableReadWrite, FilterMode filterMode, int maxTextureSize, TextureImporterCompression compression, bool crunchedCompression, int compressionQuality)
        {
            path = path.Replace("\\", "/");

            var targetImporter = (TextureImporter) AssetImporter.GetAtPath(path);

            targetImporter.spriteImportMode = forceSingle ? SpriteImportMode.Single : targetImporter.spriteImportMode;
            targetImporter.textureType = TextureImporterType.Sprite;
            targetImporter.spritePackingTag = packingTag;
            targetImporter.alphaIsTransparency = true;
            targetImporter.isReadable = enableReadWrite;
            targetImporter.mipmapEnabled = false;
            targetImporter.wrapMode = TextureWrapMode.Clamp;
            targetImporter.filterMode = filterMode;
            targetImporter.maxTextureSize = maxTextureSize;
            targetImporter.textureCompression = compression;
            targetImporter.compressionQuality = compressionQuality;
            targetImporter.crunchedCompression = crunchedCompression;

            if (forceFullRect)
            {
                var textureSettings = new TextureImporterSettings();

                targetImporter.ReadTextureSettings(textureSettings);
                textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                textureSettings.spriteExtrude = 0;
                targetImporter.SetTextureSettings(textureSettings);
            }

            targetImporter.SaveAndReimport();

            Debug.LogFormat("Import Settings set for: {0}", path);
        }
    }
}