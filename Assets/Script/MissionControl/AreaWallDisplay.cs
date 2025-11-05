using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AreaWallDisplay : MonoBehaviour
{
    public Material[] wallMaterials;

    private void Start()
    {
        //‰ŠúÝ’è
        foreach (Material mate in wallMaterials)
        {
            mate.SetFloat("_Alpha",0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player") return;

        foreach (Material mate in wallMaterials)
        {
            DOTween.To(() => mate.GetFloat("_Alpha"),
                   x => mate.SetFloat("_Alpha", x),
                   1f,
                   1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;

        foreach (Material mate in wallMaterials)
        {
            DOTween.To(() => mate.GetFloat("_Alpha"),
                   x => mate.SetFloat("_Alpha", x),
                   0f,
                   1f);
        }
    }
}
