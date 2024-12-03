using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// �^�G�C���A�X�̒�`
using WeaponSetDict = SerializableDictionary<LegacySettingData.WeaponSetPosi,UnityEngine.Transform>;

public class CustomRobotController : MonoBehaviour
{
    //���b�V���ƃI�u�W�F�N�g����R�Â��鎫��
    public Dictionary<string,MeshFilter> AllChildMeshes = new Dictionary<string,MeshFilter>();

    //�����z�u����e�I�u�W�F�N�g��SetPosi��R�Â���
    public WeaponSetDict WeaponSetDic = new();

    //���ۂ�Instantiate���Ă��镐��̃��X�g
    private WeaponSetDict createdWeaponsObj = new WeaponSetDict() 
    {
        {LegacySettingData.WeaponSetPosi.LeftArm,null },
        {LegacySettingData.WeaponSetPosi.RightArm,null },
        {LegacySettingData.WeaponSetPosi.LeftShoulder,null },
        {LegacySettingData.WeaponSetPosi.RightShoulder,null },
    };

    // Start is called before the first frame update
    void Awake()
    {
        //���g�̎q������MeshFilter�R���|�[�l���g�����q�����ׂĎ擾���Ď����ɓo�^
        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.tag == "Weapon") continue;

            AllChildMeshes[meshFilter.name]= meshFilter;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //�Z�[�u�f�[�^�̃p�[�c�\����{�̂ɔ��f
    public void PartsSetReflect()
    {
        //�f�[�^�擾
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //�{�f�B�p�[�c���f
        //�J��Ԃ������p�Ƀ��X�g��p��
        List<BodyPartsData> BodyParts = new List<BodyPartsData>();

        //�S�p�[�c�̃f�[�^�擾
        foreach (KeyValuePair<BodyPartsData.PartsType,HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            BodyParts.Add(Data);
        }

        foreach (BodyPartsData data in BodyParts)
        {
            foreach (KeyValuePair<string,Mesh> keyValue in data.ObjnameMeshPairs)
            {
                //�q�I�u�W�F�N�g������
                //GameObject meshChangeObj = transform.FindFromAllChildren(keyValue.Key).gameObject;

                AllChildMeshes[keyValue.Key].GetComponent<MeshFilter>().mesh = keyValue.Value;
            }
        }

        //����p�[�c���f
        //�p�[�c�f�[�^�擾
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi,HavingItem> keyValue in settingData.WeaponsNumber)
        {
            //������������������Ă��Ȃ����ʂ������玟�ɍs��
            if (keyValue.Value == null)
            {
                //���킪���ɐ�������Ă���A�O���Ȃǂɂ���ĕ��킪�O�ꂽ�ꍇ�I�u�W�F�N�g����������
                if (createdWeaponsObj[keyValue.Key]!=null)
                {
                    GameObject destoryObj = createdWeaponsObj[keyValue.Key].gameObject;

                    createdWeaponsObj[keyValue.Key] = null;

                    Destroy(destoryObj);
                }
                continue;
            }

            WeaponPartsData data = (WeaponPartsData)keyValue.Value.itemData;
            GameObject weaponPrefab = data.setPosiWithPrefab[keyValue.Key];

            //�������킪���ɔz�u����Ă���Ȃ�s��Ȃ�
            if (createdWeaponsObj[keyValue.Key]!=null && createdWeaponsObj[keyValue.Key].name==weaponPrefab.name)
            {
                continue;
            }


            //����������������ւ���Ă���Ȃ�΁@�ȑO�̕�����폜����
            if (createdWeaponsObj[keyValue.Key] != null && createdWeaponsObj[keyValue.Key].name != weaponPrefab.name)
            {
                GameObject destoryObj = createdWeaponsObj[keyValue.Key].gameObject;

                Destroy(destoryObj);
            }

            Transform weaponParent = WeaponSetDic[keyValue.Key];

            createdWeaponsObj[keyValue.Key] = Instantiate(weaponPrefab,weaponParent).transform;
            createdWeaponsObj[keyValue.Key].name= weaponPrefab.name;

            createdWeaponsObj[keyValue.Key].RecursiveSetLayer(LayerMask.NameToLayer("CustomizeRobot"));
        }

    }

    //�S�A�E�g���C����\��or��\��
    public void SetAllOutlines(bool active)
    {
        foreach (KeyValuePair<string,MeshFilter> keyValue in AllChildMeshes)
        {
            keyValue.Value.GetComponent<Outline>().enabled = active;
        }
    }

    //�w�肵�����b�V���̃A�E�g���C����\��or��\��
    public void SetMeshOutline(string meshName,bool active)
    {
        AllChildMeshes[meshName].GetComponent<Outline>().enabled = active;
    }
}

