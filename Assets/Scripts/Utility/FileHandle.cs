using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
    public static class FileHandle
    {

    #region File Json
    public static void SaveToJson<T>(T toSave, string fileName)
        {
            string jsonFile = JsonUtility.ToJson(toSave, true);
            // Debug.Log(jsonFile);
            string filePath = Application.dataPath + fileName;

            Debug.Log(filePath + " " + jsonFile);
            File.WriteAllText(filePath, jsonFile);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        public static T LoadToJson<T>(string fileName)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
            //  Debug.Log(fileName);
            if (jsonFile != null)
            {
                // Debug.Log("Load file json: " + jsonFile);
                return JsonConvert.DeserializeObject<T>(jsonFile.text);
            }
            else
            {
                Debug.Log(jsonFile);
                return default;
            }
        }
    #endregion

    #region File Text
    //  private string fileName = "Level/words";
    public static string LoadText(string fileName)
        {
            //Debug.Log("Load Txt -------");
            TextAsset txt = Resources.Load<TextAsset>(fileName);
            //Debug.Log(txt);
            if (txt != null)
            {
                string s = txt.text.Trim();
                return s;
            }
            return null;
        }

        //Luu level moi lam xong

        public static void SaveText(string data, string fileName)
        {
            string filePath = "Assets/Resources/" + fileName;
            //   filePath = "Assets/Resources/LevelText/Normal/1.txt";
            Debug.Log(filePath);
            try
            {
                File.WriteAllText(filePath, data);
                Debug.Log("Luu file thanh cong + " + data);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    #endregion

    #region Load Prefab from resource
    public static List<T> LoadPrefabs<T>(string fileName) where T : UnityEngine.Object
        {

            UnityEngine.Object[] objects = Resources.LoadAll(fileName);
            if (objects.Length == 0)
            {
                Debug.Log("Duong dan bi sai+ " + fileName);
            }

            List<T> listT = new List<T>();
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj as Texture2D)
                {
                    continue;
                }
                T prefab = (T)obj;
                listT.Add(prefab);
            }
            // Debug.Log(listT.Count.ToString());
            return listT;
        }
        public static int GetLengthPrefabs(string fileName)
        {
            UnityEngine.Object[] objects = Resources.LoadAll(fileName);
            return objects.Length;
        }
    #endregion

    #region Read File csv
    #endregion
}

