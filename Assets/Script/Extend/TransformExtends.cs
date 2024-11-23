using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtends
{
    //全子オブジェクト削除
    public static void DestroyAllChilds(this Transform Object)
    {
        foreach (Transform child in Object)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    //全子オブジェクトの中から特定の名前のオブジェクトを一つ探す
    public static Transform FindFromAllChildren(this Transform Object,string findName)
    {
        // 子オブジェクトを再帰的にチェック
        foreach (Transform child in Object)
        {
            if (child.name == findName)
            {
                return child;
            }

            // 子オブジェクトの中に検索対象があるか確認
            Transform found = child.FindFromAllChildren(findName);

            if (found != null)
            {
                return found;
            }
        }

        // 見つからない場合はnullを返す
        return null;
    }
}