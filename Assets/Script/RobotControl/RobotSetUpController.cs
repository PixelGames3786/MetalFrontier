using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponSetDict = SerializableDictionary<LegacySettingData.WeaponSetPosi,UnityEngine.Transform>;

public class RobotSetUpController : MonoBehaviour
{
    //���b�V���ƃI�u�W�F�N�g����R�Â��鎫��
    private Dictionary<string,MeshFilter> AllChildMeshes = new Dictionary<string,MeshFilter>();

    //�����z�u����e�I�u�W�F�N�g��SetPosi��R�Â���
    public WeaponSetDict WeaponSetDic = new();

    //���ۂ�Instantiate���Ă��镐��̃��X�g
    public Dictionary<LegacySettingData.WeaponSetPosi,Transform> createdWeaponsObj = new Dictionary<LegacySettingData.WeaponSetPosi, Transform>
    {
        {LegacySettingData.WeaponSetPosi.LeftArm,null },
        {LegacySettingData.WeaponSetPosi.RightArm,null },
        {LegacySettingData.WeaponSetPosi.LeftShoulder,null },
        {LegacySettingData.WeaponSetPosi.RightShoulder,null },
    };

    private RobotController controller;

    private void GetAllChildMesh()
    {
        //���g�̎q������MeshFilter�R���|�[�l���g�����q�����ׂĎ擾���Ď����ɓo�^
        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.tag == "Weapon") continue;
            if (meshFilter.gameObject.layer == LayerMask.NameToLayer("MiniMap")) continue;

            AllChildMeshes[meshFilter.name] = meshFilter;
        }
    }

    public void LegacySetUp(List<BodyPartsData> bodyParts,Dictionary<LegacySettingData.WeaponSetPosi,WeaponPartsData> weaponParts)
    {
        controller = GetComponent<RobotController>();

        GetAllChildMesh();

        //��v���閼�O�̃��b�V����o�^
        foreach (BodyPartsData data in bodyParts)
        {
            foreach (KeyValuePair<string, Mesh> keyValue in data.ObjnameMeshPairs)
            {
                AllChildMeshes[keyValue.Key].GetComponent<MeshFilter>().mesh = keyValue.Value;
            }
        }

        //����p�[�c���f
        //�p�[�c�f�[�^�擾
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi, WeaponPartsData> keyValue in weaponParts)
        {
            WeaponPartsData data = keyValue.Value;
            GameObject weaponPrefab = data.setPosiWithPrefab[keyValue.Key];

            Transform weaponParent = WeaponSetDic[keyValue.Key];

            //����ɑ����ʒu��o�^
            createdWeaponsObj[keyValue.Key] = Instantiate(weaponPrefab, weaponParent).transform;

            WeaponBase weaponBase = createdWeaponsObj[keyValue.Key].GetComponent<WeaponBase>();

            weaponBase.DataInitialize(data);
            weaponBase.weaponPosition = keyValue.Key;
            weaponBase.legacyStatus = GetComponent<RobotStatusController>().nowStatus;

            weaponBase.controller = controller;
        }
    }

}