using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBaseController : MonoBehaviour
{
    public static DataBaseController instance;

    public ItemDataBase itemDataBase;
    public MissionDataBase missionDataBase;

    //‹^Ž—ƒVƒ“ƒOƒ‹ƒgƒ“
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
