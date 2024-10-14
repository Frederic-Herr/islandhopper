#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This script creates an image of the current game view. Mostly used for creating icons of 3D objects.
/// </summary>
[ExecuteAlways]
public class IconCreator : MonoBehaviour
{
    [SerializeField] private string subPath; // Path of the target folder inside the Assets folder. If left empty, the icon will be created in the Assets folder.
    [SerializeField] private string fileName; // Name if the icon wihtout extension.
    [SerializeField, ReadOnly] private string fullPath; // Displays the entire path of the icon to be generated. This path will update with the subPath and fileName if changed.

    [SerializeField] private Color backgroundColor; // Sets the background color of the camera. Serves as a green screen, which is removed when the icon is created.
    [SerializeField] private float tolerance; // Value to adjust the color keying.
    [SerializeField] private Vector2 iconSize = new Vector2(256, 256); // Pixel size of the icon. Should be a power of 2.

    private Texture2D destinationTexture; // The temporary texture to write the icon to.
    private bool createIcon; // Indicates if the icon can be created on the next frame.

    /// <summary>
    /// Update fullPath with the subPath and fileName if changed.
    /// Also updates the background color of the camera.
    /// </summary>
    private void OnValidate()
    {
        Camera.main.backgroundColor = backgroundColor;
        GetFullPath();
    }

    /// <summary>
    /// Called when the camera is done rendering. If the camera is the main camera, this function will read the pixels from the camera and write them to the destinationTexture.
    /// It will then remove the background color of the image and save it as a PNG file in the specified folder.
    /// </summary>
    /// <param name="arg1">The ScriptableRenderContext of the camera.</param>
    /// <param name="arg2">The camera that finished rendering.</param>
    private void OnEndCameraRendering(ScriptableRenderContext arg1, Camera arg2)
    {
        if (!createIcon) return;
        if (arg2 == Camera.main)
        {
            createIcon = false;

            Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height); // Create a Rect of the size of the game view
            int xPosToWriteTo = 0;
            int yPosToWriteTo = 0;
            bool updateMipMapsAutomatically = false;

            destinationTexture.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo, updateMipMapsAutomatically);

            destinationTexture.Apply();

            for (int width = 0; width < iconSize.x; width++) // Loop through the pixels of the texture
            {
                for (int height = 0; height < iconSize.y; height++)
                {
                    Color pixel = destinationTexture.GetPixel(width, height);

                    if (AreColorsSimilar(pixel, backgroundColor, tolerance)) // Check if current pixel colors are similar to the background color
                    {
                        destinationTexture.SetPixel(width, height, Color.clear); // If so, set it to transparent
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

            File.WriteAllBytes(fullPath, imageData); // Safe the image as a PNG file

            AssetDatabase.Refresh();

            TextureImporter textureImporter = TextureImporter.GetAtPath(Path.Combine("Assets", subPath, fileName + ".png")) as TextureImporter; // Get the texture importer of the created icon

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


    /// <summary>
    /// Compares two colors to determine if they are similar.
    /// The two colors are considered similar if the absolute difference between the red, green, and blue components of the two colors is less than the given tolerance.
    /// This is used to remove the background color of the image.
    /// </summary>
    /// <param name="c1">The first color to compare.</param>
    /// <param name="c2">The second color to compare.</param>
    /// <param name="tolerance">The maximum absolute difference between the red, green, and blue components of the two colors.</param>
    /// <returns>True if the two colors are similar, false otherwise.</returns>
    public bool AreColorsSimilar(Color c1, Color c2, float tolerance)
    {
        return Math.Abs(c1.r - c2.r) < tolerance &&
               Math.Abs(c1.g - c2.g) < tolerance &&
               Math.Abs(c1.b - c2.b) < tolerance;
    }

    /// <summary>
    /// Updates the fullPath with the subPath and fileName if changed.
    /// If subPath is empty, the path is set to the Application.dataPath (Assets folder).
    /// If fileName is empty, a new GUID is generated and used as the fileName.
    /// The fileName is appended with ".png".
    /// </summary>
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

    /// <summary>
    /// This function is called when the user clicks the "Create Icon" button in the inspector of this script.
    /// It prepares a new texture and registers the OnEndCameraRendering callback.
    /// </summary>
    [Button]
    public void CreateIcon()
    {
        destinationTexture = new Texture2D((int)iconSize.x, (int)iconSize.y, TextureFormat.RGBA32, false);

        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

        createIcon = true;
    }

    /// <summary>
    /// This function is called when this script is destroyed.
    /// It removes the OnEndCameraRendering callback from the RenderPipelineManager.
    /// </summary>
    private void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
}
#endif
