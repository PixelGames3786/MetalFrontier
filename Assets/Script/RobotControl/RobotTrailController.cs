using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RobotTrailController : MonoBehaviour
{

    //メッシュとオブジェクト名を紐づける辞書
    public Dictionary<string, MeshFilter> AllChildMeshes = new Dictionary<string, MeshFilter>();

    private RobotController controller;

    private List<TrailRenderer> trailRenderers = new List<TrailRenderer>();

    public Material normalTrailMate, awakeTrailMate;

    private void Start()
    {
        controller = GetComponent<RobotController>();

        controller.onBoostStart+=AwakeTrailSet;
        controller.onBoostEnd+=NormalTrailSet;
    }

    private void GetAllChildMesh()
    {
        //自身の子供からMeshFilterコンポーネントを持つ子をすべて取得して辞書に登録
        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.tag == "Weapon") continue;
            if (meshFilter.gameObject.layer == LayerMask.NameToLayer("MiniMap")) continue;

            AllChildMeshes[meshFilter.name] = meshFilter;
        }
    }

    public void TrailSetUp(List<BodyPartsData> bodyParts)
    {
        GetAllChildMesh();

        //一致する名前のメッシュを登録
        foreach (BodyPartsData data in bodyParts)
        {
            foreach (KeyValuePair<string, Transform> keyValue in data.ObjnameTrailPairs)
            {
                Transform obj = AllChildMeshes[keyValue.Key].transform;

                foreach (Transform trailPrefab in keyValue.Value.GetChildren()) 
                {
                    TrailRenderer trail = Instantiate(trailPrefab,obj).GetComponent<TrailRenderer>();
                
                    trailRenderers.Add(trail);
                }
            }
        }
    }

    private void AwakeTrailSet()
    {
        foreach (TrailRenderer trail in trailRenderers)
        {
            trail.startWidth = 0.05f;
            trail.material = awakeTrailMate;
            trail.time = 0.1f;
        }
    }

    private void NormalTrailSet()
    {
        foreach (TrailRenderer trail in trailRenderers)
        {
            trail.startWidth = 0.01f;

            trail.material = normalTrailMate;
            trail.time = 0.05f;
        }
    }
}
