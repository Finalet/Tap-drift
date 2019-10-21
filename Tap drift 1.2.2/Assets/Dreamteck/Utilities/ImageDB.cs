#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dreamteck
{
    public static class ImageDB
    {
        private static List<Texture2D> images = new List<Texture2D>();

        public static Texture2D GetImage(string name, string searchDir = "")
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] == null)
                {
                    images.RemoveAt(i);
                    i--;
                    continue;
                }
                if (images[i].name.ToLower() == name.ToLower()) return images[i];
            }
            if (searchDir != "") return LoadImage(searchDir, name);
            return null;
        }

        public static Texture2D LoadImage(string localDirectory, string filename)
        {
            Texture2D image = ResourceUtility.LoadTexture(localDirectory, filename);
            if (image != null)
            {
                images.Add(image);
                return images[images.Count - 1];
            }
            return null;
        }

        public static void LoadImages(string localDirectory, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                images.Add(ResourceUtility.LoadTexture(localDirectory, names[i]));
            }
        }

        public static void LoadImages(string localDirectory)
        {
            string path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/" + localDirectory);
            List<string> files = new List<string>();
            string[] extensions = new string[] { "png", "PNG", "jpg", "JPG", "jpeg", "JPEG" };
            for (int i = 0; i < extensions.Length; i++) files.AddRange(Directory.GetFiles(path, "*."+extensions[i], SearchOption.TopDirectoryOnly));
            for (int i =0; i < files.Count; i++) images.Add(ResourceUtility.LoadTexture(files[i]));
        }

        public static void UnloadImages(string[] imageNames)
        {
            for(int i = images.Count-1; i >= 0 ; i--)
            {
                for(int n = 0; n < imageNames.Length; n++)
                {
                    if(images[i].name.ToLower() == imageNames[n].ToLower())
                    {
                        images.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }
    }
}
#endif