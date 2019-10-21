using UnityEngine;
using System.Collections;
using System.IO;

namespace Dreamteck
{
    public static class ResourceUtility
    {
        //Attempts to find the input directory pattern inside a given directory and if it fails, proceeds with looking up all subfolders
        public static string FindFolder(string dir, string folderPattern)
        {
            if (folderPattern.StartsWith("/")) folderPattern = folderPattern.Substring(1);
            if (!dir.EndsWith("/")) dir += "/";
            if (folderPattern == "") return "";
            string[] folders = folderPattern.Split('/');
            if (folders.Length == 0) return "";
            string foundDir = "";
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(d);
                    if (dirInfo.Name == folders[0])
                    {
                        foundDir = d;
                        string searchDir = FindFolder(d, string.Join("/", folders, 1, folders.Length - 1));
                        if (searchDir != "")
                        {
                            foundDir = searchDir;
                            break;
                        }
                    }
                }
                if (foundDir == "")
                {
                    foreach (string d in Directory.GetDirectories(dir))
                    {
                        foundDir = FindFolder(d, string.Join("/", folders));
                        if (foundDir != "") break;
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Debug.LogError(excpt.Message);
                return "";
            }
            return foundDir;
        }

        public static Texture2D LoadTexture(string dreamteckPath, string textureFileName)
        {
            string path = Application.dataPath + "/Dreamteck/" + dreamteckPath;
            if (!Directory.Exists(path))
            {
                path = FindFolder(Application.dataPath, "Dreamteck/" + dreamteckPath);
                if (!Directory.Exists(path)) return null;
            }
            if (!File.Exists(path + "/" + textureFileName)) return null;
            byte[] bytes = File.ReadAllBytes(path + "/" + textureFileName);
            Texture2D result = new Texture2D(1, 1);
            result.name = textureFileName;
            result.LoadImage(bytes);
            return result;
        }

        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path)) return null;
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D result = new Texture2D(1, 1);
            FileInfo finfo = new FileInfo(path);
            result.name = finfo.Name;
            result.LoadImage(bytes);
            return result;
        }
    }
}