using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class Image_out {

    public enum AZTFTextureFormat {
        rgba32,
        etc1,
        pvrtc1,
        rgba16
    }

    public enum AZTFAlphaType {
        alphachannel,
        separate,
        noalpha
    }
    public class AZTF {
        public AZTFTextureFormat format;
        public AZTFAlphaType alphatype;
        public int version;
        public int originWidth;
        public int originHeight;
        public int width;
        public int height;
        public byte[] rawData;
        public byte[] rawDataAlpha;

        public bool LoadFromBytes(byte[] bytes) {
            using (MemoryStream memoryStream = new MemoryStream(bytes)) {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream)) {
                    this.version = binaryReader.ReadInt32();
                    this.format = (AZTFTextureFormat)binaryReader.ReadInt32();
                    this.alphatype = (AZTFAlphaType)binaryReader.ReadInt32();
                    this.originWidth = binaryReader.ReadInt32();
                    this.originHeight = binaryReader.ReadInt32();
                    this.width = binaryReader.ReadInt32();
                    this.height = binaryReader.ReadInt32();
                    int count = binaryReader.ReadInt32();
                    this.rawData = binaryReader.ReadBytes(count);
                    int num = binaryReader.ReadInt32();
                    if (num > 0) {
                        this.rawDataAlpha = binaryReader.ReadBytes(num);
                    }
                }
            }
            return true;
        }
    }
    [MenuItem("Image/Export")]
    public static void publishPNG() {
        string fold = EditorUtility.OpenFolderPanel("", "", "");

        if (Directory.Exists(fold)) {

            DirectoryInfo dir = new DirectoryInfo(fold);
            FindAZTFFile(dir);
        }
        Debug.Log("complete");
    }
    private static void FindAZTFFile(DirectoryInfo dir) {
        FileInfo[] files = dir.GetFiles("*.aztf");
        foreach (FileInfo item in files) {
            loadFromFile(item.FullName);
        }
        DirectoryInfo[] dis = dir.GetDirectories();
        foreach (DirectoryInfo di in dis) {
            FindAZTFFile(di);
        }
    }

    public static void loadFromFile(string path) {
        AZTF file = new AZTF();

        if (File.Exists(path)) {
            byte[] bytes = File.ReadAllBytes(path);
            file.LoadFromBytes(bytes);
        }
        Texture2D texture = null;
        Texture2D alphatexture = null;
        switch (file.format) {
            case AZTFTextureFormat.rgba32:
                texture = new Texture2D(file.width, file.height, TextureFormat.RGBA32, false);
                break;
            case AZTFTextureFormat.etc1:
                texture = new Texture2D(file.width, file.height, TextureFormat.ETC_RGB4, false);
                if (file.alphatype == AZTFAlphaType.separate) {
                    alphatexture = new Texture2D(file.width, file.height, TextureFormat.ETC_RGB4, false);
                }
                break;
            case AZTFTextureFormat.pvrtc1:
                texture = new Texture2D(file.width, file.height, TextureFormat.PVRTC_RGBA4, false);
                break;
            case AZTFTextureFormat.rgba16:
                texture = new Texture2D(file.width, file.height, TextureFormat.RGBA4444, false, false);
                break;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadRawTextureData(file.rawData);
        texture.Apply(false);
        if (alphatexture != null) {
            alphatexture.LoadRawTextureData(file.rawDataAlpha);
            alphatexture.Apply(false);
        }
        string fileName = Path.GetFileNameWithoutExtension(path);
        Texture2D tex = null;
        if (texture != null && alphatexture != null) {
            tex = ChangeToRGB32(texture, alphatexture);
        }
        else if (texture != null) {
            tex = texture;
        }

        byte[] tb = tex.EncodeToPNG();
        File.Delete(path);
        string outPath = path.Replace("aztf", "png");
        File.WriteAllBytes(outPath, tb);
    }
    public static Texture2D ChangeToRGB32(Texture2D ect1, Texture2D ahpla) {

        Texture2D tex = new Texture2D(ect1.width, ect1.height, TextureFormat.ARGB32, false);
        Color newColor = new Color(0f, 0f, 0f);
        for (int i = 0; i < ect1.width; ++i) {
            for (int j = 0; j < ect1.height; ++j) {
                newColor = ect1.GetPixel(i, j);
                newColor.a = ahpla.GetPixel(i, j).r;
                tex.SetPixel(i, j, newColor);
            }
        }
        return tex;
    }
}

