using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BRDFGenerator : EditorWindow
{
    private int resolution = 64;
    private string filename = "New Texture";
    private FileFormat format = FileFormat.PNG;

    public enum GenerationType
    {
        Lambert,
        Specular,
        Minnaert,
        OrenNayar,
        SubsurfaceScattering
    }

    [MenuItem("Window/BRDF Generator", priority = 10000)]
    public static void ShowWindow()
    {
        GetWindow<BRDFGenerator>(false, "BRDF Generator", true);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BRDF Generator");
        resolution = EditorGUILayout.IntField("Resolution", resolution);
        filename = EditorGUILayout.TextField("Filename", filename);
        format = (FileFormat)EditorGUILayout.EnumPopup("Format", format);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Lambert", GUILayout.Height(50)))
        {
            GenerateFile(GenerationType.Lambert);
        }
    }

    private Color[] GenerateLambertInternal(Color[] input, int resolution)
    {
        int x = 0;
        int y = 0;

        float lDot;
        float vDot;

        for (int p = 0; p < resolution * resolution; ++p)
        {
            x = p % resolution;
            y = (p / resolution);

            vDot = (x / (float)resolution);
            lDot = (y / (float)resolution);
            lDot = 2 * (lDot - 0.5f);

            // half diff = (NdotL * 0.5) + 0.5; 
            // 2 * (diff - 0.5) = NdotL

            lDot = Mathf.Clamp(lDot, 0, 1);

            input[p] = new Color(lDot, lDot, lDot);
        }

        return input;
    }

    private Color[] GenerateSpecularInternal(Color[] input, int resolution)
    {
        int x = 0;
        int y = 0;

        float lDot;
        float vDot;

        for (int p = 0; p < resolution * resolution; ++p)
        {
            x = p % resolution;
            y = (p / resolution);

            vDot = x / (float)resolution;
            lDot = y / (float)resolution;
            lDot = 2 * (lDot - 0.5f);

            // half diff = (NdotL * 0.5) + 0.5; 
            // 2 * (diff - 0.5) = NdotL

            //float h = 

            lDot = Mathf.Clamp(lDot, 0, 1);

            input[p] = new Color(lDot, lDot, lDot);
        }

        return input;
    }

    private void GenerateFile(GenerationType gt)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] texColors = new Color[resolution * resolution];

        switch (gt)
        {
            case GenerationType.Lambert:
                texColors = GenerateLambertInternal(texColors, resolution);
                break;
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
