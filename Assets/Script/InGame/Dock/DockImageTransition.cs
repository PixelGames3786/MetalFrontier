using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System;
using static WeaponPartsData;

public class DockImageTransition : MonoBehaviour
{
    [Serializable]
    public class MenuTextureSet
    {
        public MenuType type;
        public Texture tex;
        public Camera cam;
    }

    public enum MenuType
    {
        Terminal,
        Mission,
        CustomGenre,
        CustomPartsType,
        CustomSelectParts,
        Shop
    }

    public Camera activeCamera1, activeCamera2; //二つしかカメラをオンにしないので
    public List<MenuTextureSet> menuTextureList=new List<MenuTextureSet>();

    public Material dockImageMaterial;

    public Action onTransitionComplete;

    // Start is called before the first frame update
    void Start()
    {
        SetDefaultMaterial();
    }

    private void SetDefaultMaterial()
    {
        activeCamera1 = menuTextureList.First(set => set.type == MenuType.Terminal).cam;
        activeCamera2 = menuTextureList.First(set => set.type == MenuType.Terminal).cam;
        activeCamera1.gameObject.SetActive(true);

        dockImageMaterial.SetTexture("_LeftTex", menuTextureList.First(set => set.type == MenuType.Terminal).tex);

        dockImageMaterial.SetFloat("_ChangeFactor", 0f);
    }

    public void TransitionToRight(MenuType leftType,MenuType rightType)
    {
        CameraReset(leftType, rightType);

        dockImageMaterial.SetFloat("_ChangeFactor", 0f);

        dockImageMaterial.SetTexture("_LeftTex", menuTextureList.First(set=>set.type==leftType).tex);
        dockImageMaterial.SetTexture("_RightTex", menuTextureList.First(set => set.type == rightType).tex);

        dockImageMaterial.DOFloat(1f, "_ChangeFactor", 0.5f).OnComplete(() => { onTransitionComplete?.Invoke(); });
    }

    public void TransitionToLeft(MenuType leftType, MenuType rightType)
    {
        CameraReset(leftType,rightType);

        dockImageMaterial.SetFloat("_ChangeFactor", 1f);

        dockImageMaterial.SetTexture("_LeftTex", menuTextureList.First(set => set.type == leftType).tex);
        dockImageMaterial.SetTexture("_RightTex", menuTextureList.First(set => set.type == rightType).tex);

        dockImageMaterial.DOFloat(0f, "_ChangeFactor", 0.5f).OnComplete(() => { onTransitionComplete?.Invoke(); });
    }

    //表示しない画面をレンダリングするカメラは普段表示しておかないようにする
    private void CameraReset(MenuType leftType,MenuType rightType)
    {
        activeCamera1.gameObject.SetActive(false);
        activeCamera2.gameObject.SetActive(false);

        activeCamera1 = menuTextureList.First(set => set.type == leftType).cam;
        activeCamera2 = menuTextureList.First(set => set.type == rightType).cam;

        activeCamera1.gameObject.SetActive(true);
        activeCamera2.gameObject.SetActive(true);
    }
}
