using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Decel
{
    public static class FileManager
    {
        public delegate void SaveHandler(object obj, string path);
        public delegate void LoadHandler(object loadedObj, string path);

        public static event SaveHandler OnSaving;
        public static event SaveHandler OnSaved;
        public static event LoadHandler OnLoading;
        public static event LoadHandler OnLoaded;

        public static SaveHandler SaveCallback;
        public static LoadHandler LoadCallback;

        private static List<string> ignoredFiles = new List<string>() { "Player.log", "output_log.txt" };
        private static List<string> ignoredDirectories = new List<string>() { "Analytics" };

        public static List<string> IgnoredFiles
        {
            get { return ignoredFiles; }
        }

        public static List<string> IgnoredDirectories
        {
            get { return ignoredDirectories; }
        }

        public static List<string> ReadTextFile(string path, bool includeBlankLines = true)
        {
            if (!path.StartsWith("/"))
            {
                path = $"{Application.dataPath}/Data/" + path;
            }

            List<string> lines = new List<string>();
            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
            catch (FileNotFoundException exception)
            {
                Debug.LogError($"File not found: {exception.FileName}");
            }

            return lines;
        }

        public static List<string> ReadTextAsset(string path, bool includeBlankLines = true)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"Asset not found: {path}");
                return null;
            }

            return ReadTextAsset(textAsset, includeBlankLines);
        }

        public static List<string> ReadTextAsset(TextAsset textAsset, bool includeBlankLines = true)
        {
            List<string> lines = new List<string>();
            using (StringReader stringReader = new StringReader(textAsset.text))
            {
                while (stringReader.Peek() > -1)
                {
                    string line = stringReader.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        public static void Save<T>(string path, T obj)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException("identifier");
            }
            if (OnSaving != null)
            {
                OnSaving(obj, path);
            }
            if (obj == null)
            {
                obj = default;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string data = JsonUtility.ToJson(obj, true);
            File.WriteAllText(path, data);

            if (SaveCallback != null)
            {
                SaveCallback.Invoke(obj, path);
            }
            if (OnSaved != null)
            {
                OnSaved(obj, path);
            }
        }

        public static T Load<T>(string path)
        {
            return Load<T>(path, default);
        }

        public static T Load<T>(string path, T defaultValue)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException("identifier");
            }
            if (OnLoading != null)
            {
                OnLoading(null, path);
            }
            if (defaultValue == null)
            {
                defaultValue = default;
            }

            T result = defaultValue;

            if (!Exists(path))
            {
                Debug.LogWarningFormat(
                    "The specified identifier ({1}) does not exists. please use Exists () to check for existent before calling Load.\n" +
                    "returning the default(T) instance.", path);
                return result;
            }

            string data;
            data = File.ReadAllText(path);
            result = JsonUtility.FromJson<T>(data);

            if (result == null)
            {
                result = defaultValue;
            }
            if (LoadCallback != null)
            {
                LoadCallback.Invoke(
                    result,
                    path);
            }
            if (OnLoaded != null)
            {
                OnLoaded(
                    result,
                    path);
            }
            return result;
        }

        public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException("identifier");
            }

            bool exists = Directory.Exists(path);
            if (!exists)
            {
                exists = File.Exists(path);
            }
            return exists;
        }

        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException("identifier");
            }
            if (!Exists(path))
            {
                return;
            }

            var fileName = Path.GetFileName(path);
            if (ignoredFiles.Contains(fileName) || ignoredDirectories.Contains(fileName))
            {
                return;
            }
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static void Clear(string path)
        {
            DeleteAll(path);
        }

        public static void DeleteAll(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (ignoredFiles.Contains(files[i].Name))
                {
                    continue;
                }
                files[i].Delete();
            }
            DirectoryInfo[] dirs = info.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (ignoredDirectories.Contains(dirs[i].Name))
                {
                    continue;
                }
                dirs[i].Delete(true);
            }
        }

        public static FileInfo[] GetFiles(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }
            FileInfo[] files = new FileInfo[0];
            if (!Exists(path))
            {
                return files;
            }
            if (Directory.Exists(path))
            {
                DirectoryInfo info = new DirectoryInfo(path);
                files = info.GetFiles();
            }
            return files;
        }

        public static DirectoryInfo[] GetDirectories(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }
            DirectoryInfo[] directories = new DirectoryInfo[0];
            if (!Exists(path))
            {
                return directories;
            }
            if (Directory.Exists(path))
            {
                DirectoryInfo info = new DirectoryInfo(path);
                directories = info.GetDirectories();
            }
            return directories;
        }

        public static bool IsFilePath(string path)
        {
            bool result = false;
            if (Path.IsPathRooted(path))
            {
                try
                {
                    Path.GetFullPath(path);
                    result = true;
                }
                catch (System.Exception)
                {
                    result = false;
                }
            }
            return result;
        }
    }
}