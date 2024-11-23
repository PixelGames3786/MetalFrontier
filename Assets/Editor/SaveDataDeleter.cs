using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveDataDeleter : Editor
{
    [MenuItem("Tools/DeleteSaveData")]
    public static void DeleteSpecificFile()
    {
        // �v���W�F�N�g�O�̃t�@�C���̐�΃p�X
        string filePath = Application.persistentDataPath + "/SaveData/SaveData.sav";

        // �t�@�C���̑��݂��m�F
        if (File.Exists(filePath))
        {
            try
            {
                // �t�@�C�����폜
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

