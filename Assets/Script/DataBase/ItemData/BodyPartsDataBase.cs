using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BodyPartsDataBase", menuName = "PartsDataBase/CreateBodyParts")]
public class BodyPartsDataBase : ScriptableObject
{
    public List<BodyPartsData> bodyPartsDatas = new List<BodyPartsData>();

}
