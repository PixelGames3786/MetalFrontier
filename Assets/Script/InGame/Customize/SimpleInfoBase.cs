using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SimpleInfoBase : MonoBehaviour
{
    [SerializeField]
    private Image forcusImage; //フォーカス（現在選択中かの画像）

    public void OnForcus()
    {
        forcusImage.DOFade(1f, 0.3f);
    }

    public void OffForcus()
    {
        forcusImage.DOFade(0f, 0.3f);
    }
}
