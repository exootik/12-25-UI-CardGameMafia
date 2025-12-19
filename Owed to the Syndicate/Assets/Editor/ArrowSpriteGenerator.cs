// Place this script in an "Editor" folder
// Unity Menu: Tools > Generate Arrow Sprites

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ArrowSpriteGenerator : EditorWindow
{
    private int textureSize = 64;
    private Color arrowColor = Color.white;
    private ArrowStyle arrowStyle = ArrowStyle.Triangle;

    private enum ArrowStyle
    {
        Triangle,
        Sharp,
        Rounded
    }

    [MenuItem("Tools/Generate Arrow Sprites")]
    public static void ShowWindow()
    {
        GetWindow<ArrowSpriteGenerator>("Arrow Sprite Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arrow Sprite Generator", EditorStyles.boldLabel);

        textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 32, 128);
        arrowColor = EditorGUILayout.ColorField("Arrow Color", arrowColor);
        arrowStyle = (ArrowStyle)EditorGUILayout.EnumPopup("Arrow Style", arrowStyle);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Arrow Head", GUILayout.Height(40)))
        {
            GenerateArrowHead();
        }

        if (GUILayout.Button("Generate Arrow Line", GUILayout.Height(40)))
        {
            GenerateArrowLine();
        }
    }

    private void GenerateArrowHead()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // Remplir avec transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Dessiner la flèche selon le style
        switch (arrowStyle)
        {
            case ArrowStyle.Triangle:
                DrawTriangle(pixels, textureSize);
                break;
            case ArrowStyle.Sharp:
                DrawSharpArrow(pixels, textureSize);
                break;
            case ArrowStyle.Rounded:
                DrawRoundedArrow(pixels, textureSize);
                break;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        SaveTexture(texture, $"ArrowHead_{arrowStyle}");
    }

    private void GenerateArrowLine()
    {
        int width = textureSize * 2;
        int height = textureSize / 4;

        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        // Ligne simple avec dégradé aux extrémités
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = (float)x / width;
                float alpha = 1f;

                // Fade aux extrémités
                if (normalizedX < 0.1f)
                    alpha = normalizedX / 0.1f;
                else if (normalizedX > 0.9f)
                    alpha = (1f - normalizedX) / 0.1f;

                pixels[y * width + x] = new Color(arrowColor.r, arrowColor.g, arrowColor.b, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        SaveTexture(texture, "ArrowLine");
    }

    private void DrawTriangle(Color[] pixels, int size)
    {
        float centerY = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (float)x / size;
                float normalizedY = (float)y / size;

                // Triangle pointant à droite
                float topEdge = 0.5f + (normalizedX * 0.5f);
                float bottomEdge = 0.5f - (normalizedX * 0.5f);

                if (normalizedX > 0.2f &&
                    normalizedY < topEdge &&
                    normalizedY > bottomEdge)
                {
                    // Anti-aliasing basique
                    float distToEdge = Mathf.Min(
                        topEdge - normalizedY,
                        normalizedY - bottomEdge
                    ) * size;

                    float alpha = Mathf.Clamp01(distToEdge);
                    pixels[y * size + x] = new Color(arrowColor.r, arrowColor.g, arrowColor.b, alpha);
                }
            }
        }
    }

    private void DrawSharpArrow(Color[] pixels, int size)
    {
        float centerY = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (float)x / size;
                float normalizedY = (float)y / size;

                // Flèche plus pointue
                float topEdge = 0.5f + (normalizedX * 0.6f);
                float bottomEdge = 0.5f - (normalizedX * 0.6f);

                // Base plus épaisse
                if (normalizedX < 0.3f)
                {
                    topEdge = 0.65f;
                    bottomEdge = 0.35f;
                }

                if (normalizedY < topEdge && normalizedY > bottomEdge)
                {
                    float distToEdge = Mathf.Min(
                        topEdge - normalizedY,
                        normalizedY - bottomEdge
                    ) * size;

                    float alpha = Mathf.Clamp01(distToEdge);
                    pixels[y * size + x] = new Color(arrowColor.r, arrowColor.g, arrowColor.b, alpha);
                }
            }
        }
    }

    private void DrawRoundedArrow(Color[] pixels, int size)
    {
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size * 0.4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (float)x / size;
                float normalizedY = (float)y / size;

                // Corps de la flèche
                float topEdge = 0.5f + (normalizedX * 0.5f);
                float bottomEdge = 0.5f - (normalizedX * 0.5f);

                bool inBody = normalizedX > 0.2f &&
                              normalizedY < topEdge &&
                              normalizedY > bottomEdge;

                // Pointe arrondie
                if (normalizedX > 0.7f)
                {
                    Vector2 point = new Vector2(x, y);
                    Vector2 tipCenter = new Vector2(size * 0.9f, size / 2f);
                    float dist = Vector2.Distance(point, tipCenter);

                    if (dist < size * 0.25f)
                    {
                        inBody = true;
                    }
                }

                if (inBody)
                {
                    float distToEdge = Mathf.Min(
                        topEdge - normalizedY,
                        normalizedY - bottomEdge
                    ) * size;

                    float alpha = Mathf.Clamp01(distToEdge);
                    pixels[y * size + x] = new Color(arrowColor.r, arrowColor.g, arrowColor.b, alpha);
                }
            }
        }
    }

    private void SaveTexture(Texture2D texture, string name)
    {
        string path = EditorUtility.SaveFilePanel(
            "Save Arrow Sprite",
            "Assets",
            name + ".png",
            "png"
        );

        if (string.IsNullOrEmpty(path)) return;

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);

        // Rafraîchir l'asset database
        string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.ImportAsset(relativePath);

        // Configurer comme sprite
        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log($"Arrow sprite saved to: {relativePath}");

        // Sélectionner le sprite dans le projet
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
}
#endif