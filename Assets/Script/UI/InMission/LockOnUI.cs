using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    private RectTransform parentRect,UIRect;

    private RectTransform mainLockMarkRect;
    private List<RectTransform> subLockMarkRects = new List<RectTransform>();

    private Transform mainTarget;
    private List<Transform> subTargets=new List<Transform>();

    private Dictionary<Transform,RectTransform> targetWithMarkDic = new Dictionary<Transform,RectTransform>();

    [SerializeField]
    private RectTransform mainLockMarkPrefab,subLockMarkPrefab;

    [SerializeField]
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        parentRect=transform.parent.GetComponent<RectTransform>();
        UIRect= GetComponent<RectTransform>();

        //メインのロックオンマークだけ作成しておいて非表示にしておく
        mainLockMarkRect = Instantiate(mainLockMarkPrefab, transform);
        mainLockMarkRect.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RockOnUIPositionUpdate()
    {
        //メインターゲットUI位置更新
        if (mainTarget)
        {
            // ワールド座標からスクリーン座標に変換
            Vector2 screenPos = Camera.main.WorldToScreenPoint(mainTarget.position);
            Vector2 localPos = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPos);

            // UI要素の位置をスクリーン座標に設定
            mainLockMarkRect.localPosition = localPos;
        }

        if (subTargets.Count == 0) return;

        //サブターゲットUI位置更新
        for (int i=0;i<subTargets.Count;i++)
        {
            // ワールド座標からスクリーン座標に変換
            Vector2 screenPos = Camera.main.WorldToScreenPoint(subTargets[i].position);
            Vector2 localPos = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPos);

            // UI要素の位置をスクリーン座標に設定
            subLockMarkRects[i].localPosition = localPos;
        }
    }

    public void LockOnChange(Transform mainTarget,List<Transform> subTargets)
    {
        //メインターゲットがいるかどうか
        if (mainTarget)
        {
            mainLockMarkRect.gameObject.SetActive(true);
            this.mainTarget = mainTarget;
        }
        else
        {
            this.mainTarget = null;
            mainLockMarkRect.gameObject.SetActive(false);
        }

        //生成しているサブターゲットUIとサブターゲットの数を合わせる
        this.subTargets = subTargets;

        //UIの方が少なかったらその差分生成する
        if (subLockMarkRects.Count < subTargets.Count)
        {
            foreach (var rect in subLockMarkRects) rect.gameObject.SetActive(true); //全表示

            RectTransform rockOnUI = Instantiate(subLockMarkPrefab, transform);
            subLockMarkRects.Add(rockOnUI);
        }
        //UIの方が多かったらその差分非表示にする
        else if (subTargets.Count<subLockMarkRects.Count)
        {
            int diffCount=subLockMarkRects.Count-subTargets.Count;

            for (int i=subLockMarkRects.Count-1;i>=subLockMarkRects.Count-diffCount; i--)
            {
                subLockMarkRects[i].gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var rect in subLockMarkRects) rect.gameObject.SetActive(true); //全表示
        }
    }
}
