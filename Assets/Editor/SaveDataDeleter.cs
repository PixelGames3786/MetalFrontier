using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveDataDeleter : Editor
{
    [MenuItem("Tools/DeleteSaveData")]
    public static void DeleteSpecificFile()
    {
        // プロジェクト外のファイルの絶対パス
        string filePath = Application.persistentDataPath + "/SaveData/SaveData.sav";

        // ファイルの存在を確認
        if (File.Exists(filePath))
        {
            try
            {
                // ファイルを削除
                File.Delete(filePath);
                Debug.Log($"File deleted: {filePath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to delete file: {filePath}\n{ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"File does not exist: {filePath}");
        }
    }
}

