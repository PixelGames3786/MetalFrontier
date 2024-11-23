using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartsSelectButton : MonoBehaviour,IButton
{
    public CustomizeUIController controller;

    public BodyPartsData.PartsType partsType;

    public string FuncName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        object[] args = {partsType };

        controller.CallStateFuncArg(FuncName,args);
    }
}
