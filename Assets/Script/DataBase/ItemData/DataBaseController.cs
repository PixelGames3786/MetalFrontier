using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataBaseController : MonoBehaviour
{
    public static DataBaseController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DataBaseController>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    private static DataBaseController _instance;

    public ItemDataBase itemDataBase;
    public MissionDataBase missionDataBase;

    //‹^Ž—ƒVƒ“ƒOƒ‹ƒgƒ“
    public void Awake()
    {
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
