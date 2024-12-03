using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtends
{
    //�S�q�I�u�W�F�N�g�폜
    public static void DestroyAllChilds(this Transform Object)
    {
        foreach (Transform child in Object)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    //�S�q�I�u�W�F�N�g�̒��������̖��O�̃I�u�W�F�N�g����T��
    public static Transform FindFromAllChildren(this Transform Object,string findName)
    {
        // �q�I�u�W�F�N�g���ċA�I�Ƀ`�F�b�N
        foreach (Transform child in Object)
        {
            if (child.name == findName)
            {
                return child;
            }

            // �q�I�u�W�F�N�g�̒��Ɍ����Ώۂ����邩�m�F
            Transform found = child.FindFromAllChildren(findName);

            if (found != null)
            {
                return found;
            }
        }

        // ������Ȃ��ꍇ��null��Ԃ�
        return null;
    }

    //�q�I�u�W�F�N�g�̃��C���[��S�ĕύX
    public static void RecursiveSetLayer(this Transform target, int layer)
    {
        // �ΏۃI�u�W�F�N�g�̎q�I�u�W�F�N�g���`�F�b�N����
        foreach (Transform child in target)
        {
            // �q�I�u�W�F�N�g�̃��C���[��؂�ւ���
            GameObject childObject = child.gameObject;
            childObject.layer = layer;

            // �ċA�I�ɑS�Ă̎q�I�u�W�F�N�g����������
            RecursiveSetLayer(childObject.transform, layer);
        }
    }
}