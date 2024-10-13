#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class IconCreator : MonoBehaviour
{
    [SerializeField] private string subPath;
    [SerializeField] private string fileName;
    [SerializeField, ReadOnly] private string fullPath;

    [SerializeField] private Color backgroundColor;
    [SerializeField] private float tolerance;
    [SerializeField] private Vector2 iconSize = new Vector2(256, 256);

    private Texture2D destinationTexture;
    private bool createIcon;

    private void OnValidate()
    {
        Camera.main.backgroundColor = backgroundColor;
        GetFullPath();
    }

    private void OnEndCameraRendering(ScriptableRenderContext arg1, Camera arg2)
    {
        if (!createIcon) return;
        if (arg2 == Camera.main)
        {
            createIcon = false;

            Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height);
            int xPosToWriteTo = 0;
            int yPosToWriteTo = 0;
            bool updateMipMapsAutomatically = false;

            destinationTexture.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo, updateMipMapsAutomatically);

            destinationTexture.Apply();

            for (int width = 0; width < iconSize.x; width++)
            {
                for (int height = 0; height < iconSize.y; height++)
                {
                    Color pixel = destinationTexture.GetPixel(width, height);

                    if (AreColorsSimilar(pixel, backgroundColor, tolerance))
                    {
                        destinationTexture.SetPixel(width, height, Color.clear);
                    }
                }
            }

            destinationTexture.Apply();

            byte[] imageData = destinationTexture.EncodeToPNG();

            string folderPath = Path.Combine(Application.dataPath, subPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllBytes(fullPath, imageData);

            AssetDatabase.Refresh();

            TextureImporter textureImporter = TextureImporter.GetAtPath(Path.Combine("Assets", subPath, fileName + ".png")) as TextureImporter;

            if (textureImporter)
            {
                textureImporter.alphaIsTransparency = true;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }
    }

    public bool AreColorsSimilar(Color c1, Color c2, float tolerance)
    {
        return Math.Abs(c1.r - c2.r) < tolerance &&
               Math.Abs(c1.g - c2.g) < tolerance &&
               Math.Abs(c1.b - c2.b) < tolerance;
    }

    private void GetFullPath()
    {
        if (!string.IsNullOrWhiteSpace(subPath))
        {
            fullPath = Path.Combine(Application.dataPath, subPath);
        }
        else
        {
            fullPath = Application.dataPath;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "Icon_" + Guid.NewGuid().ToString();
        }

        fullPath = Path.Combine(fullPath, fileName + ".png");
    }

    [Button]
    public void CreateIcon()
    {
        destinationTexture = new Texture2D((int)iconSize.x, (int)iconSize.y, TextureFormat.RGBA32, false);

        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

        createIcon = true;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
}
#endif
