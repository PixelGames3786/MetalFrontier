using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RockOnUI : MonoBehaviour
{
    private RectTransform parentRect,UIRect;

    private Dictionary<Transform,RectTransform> targetWithMarkDic = new Dictionary<Transform,RectTransform>();

    [SerializeField]
    private RectTransform rockMarkPrefab;

    [SerializeField]
    private Transform target;

    private Transform[] targets;

    [SerializeField]
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        parentRect=transform.parent.GetComponent<RectTransform>();
        UIRect= GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        RockOnUIPositionUpdate();
    }

    private void RockOnUIPositionUpdate()
    {
        //UI�ʒu�X�V
        foreach (var keyValue in targetWithMarkDic)
        {
            // ���[���h���W����X�N���[�����W�ɕϊ�
            Vector2 screenPos = Camera.main.WorldToScreenPoint(keyValue.Key.position);
            Vector2 localPos = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPos);

            // UI�v�f�̈ʒu���X�N���[�����W�ɐݒ�
            keyValue.Value.localPosition = localPos;
        }
    }

    public void RockOnChange(List<Transform> targets)
    {
        foreach (Transform target in targets)
        {
            //�o�^����Ă��Ȃ�������V�������b�N�I���\�������
            if (!targetWithMarkDic.Keys.Contains(target))
            {
                RectTransform rockOnUI = Instantiate(rockMarkPrefab,transform);

                targetWithMarkDic[target] = rockOnUI;

                Vector2 screenPos = Camera.main.WorldToScreenPoint(target.position);
                Vector2 localPos = Vector2.zero;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPos);

                // UI�v�f�̈ʒu���X�N���[�����W�ɐݒ�
                rockOnUI.localPosition = localPos;
            }
        }

        List<Transform> removeList = new List<Transform>();

        //���������b�N�I������Ă����I�u�W�F�N�g�������Ă�����폜
        foreach (var keyValue in targetWithMarkDic)
        {
            if (!targets.Contains(keyValue.Key))
            {
                Destroy(keyValue.Value.gameObject);
                removeList.Add(keyValue.Key);
            }
        }

        foreach (Transform remove in removeList)
        {
            targetWithMarkDic.Remove(remove);
        }
    }

    public void RockOn(Transform tag)
    {
        target = tag;

        if (target)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void StartRockOn(Transform tag)
    {
        target = tag;

        gameObject.SetActive(true);
    }

    public void EndRockOn()
    {
        gameObject.SetActive(false);
    }
}
