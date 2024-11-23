using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutlineScript : MonoBehaviour
{

    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScaleFactor;
    [SerializeField] private Color outlineColor;
    private Renderer outlineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        outlineRenderer = CreateOutline(outlineMaterial,outlineScaleFactor,outlineColor);

        outlineRenderer.enabled = true;
    }

    Renderer CreateOutline(Material outlineMat,float scaleFactor,Color color)
    {
        GameObject outlineObject = Instantiate(gameObject,transform.position,transform.rotation,transform);
        Renderer rend=outlineObject.GetComponent<Renderer>();

        rend.material = outlineMat;
        rend.material.SetColor("_OutlineColor",color);
        rend.material.SetFloat("_Scale",scaleFactor);

        rend.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;

        outlineObject.GetComponent<OutlineScript>().enabled = false;

        rend.enabled = false;

        return rend;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
