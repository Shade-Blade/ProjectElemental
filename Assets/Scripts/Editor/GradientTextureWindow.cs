using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GradientWindow : EditorWindow
{
    private Gradient gradientA = new Gradient();
    private Gradient gradientB = new Gradient();
    private Gradient gradientC = new Gradient();
    private Gradient gradientD = new Gradient();
    private int resolution = 64;
    private string filename = "New Texture";
    private FileFormat format = FileFormat.PNG;

    [MenuItem("Window/Gradient2PNG", priority = 10000)]
    public static void ShowWindow()
    {
        GetWindow<GradientWindow>(false, "Gradient2PNG", true);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Gradient to File Converter");
        gradientA = EditorGUILayout.GradientField("Gradient A", gradientA);
        gradientB = EditorGUILayout.GradientField("Gradient B", gradientB);
        gradientC = EditorGUILayout.GradientField("Gradient C", gradientC);
        gradientD = EditorGUILayout.GradientField("Gradient D", gradientD);
        resolution = EditorGUILayout.IntField("Resolution", resolution);
        filename = EditorGUILayout.TextField("Filename", filename);
        format = (FileFormat)EditorGUILayout.EnumPopup("Format", format);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Convert to asset", GUILayout.Height(50)))
        {
            ConvertGradient();
        }
    }

    private void ConvertGradient()
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] texColors = new Color[resolution * resolution];

        int g = 0;
        for (int x = 0; x < resolution * resolution; ++x)
        {
            g = x % resolution;
            if (g < resolution / 4)
            {
                texColors[x] = gradientA.Evaluate((float)(x / (resolution - 1)) / resolution);
            }
            else if (g < resolution / 2)
            {
                texColors[x] = gradientB.Evaluate((float)(x / (resolution - 1)) / resolution);
            }
            else if (g < (resolution / 4) + (resolution / 2))
            {
                texColors[x] = gradientC.Evaluate((float)(x / (resolution - 1)) / resolution);
            }
            else
            {
                texColors[x] = gradientD.Evaluate((float)(x / (resolution - 1)) / resolution);
            }
        }

        tex.SetPixels(texColors);

        byte[] bytes = null;
        string filenameWithExtension = null;

        switch (format)
        {
            case FileFormat.JPG:
                bytes = tex.EncodeToJPG();
                filenameWithExtension = "/" + filename + ".jpg";
                break;
            case FileFormat.PNG:
                bytes = tex.EncodeToPNG();
                filenameWithExtension = "/" + filename + ".png";
                break;
            case FileFormat.TGA:
                bytes = tex.EncodeToTGA();
                filenameWithExtension = "/" + filename + ".tga";
                break;
        }

        try
        {
            string path = Application.dataPath + filenameWithExtension;

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
        catch (IOException e)
        {
            Debug.LogError(e);
        }
    }
}

public enum FileFormat
{
    JPG,
    PNG,
    TGA
}