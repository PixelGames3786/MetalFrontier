using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackSlashSource : MonoBehaviour
{
    public List<ItemData> selectItemList = new List<ItemData>();

    public bool isGeted { get; private set; } //既に入手されているか

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SourceSetUp(List<ItemData> itemList)
    {
        //確率でitemListからselectItemListを出力
        //確定でひとつは入手可能

        List<int> selectItemIndex=new List<int>();
        int random = 0;

        bool isContinue = true;

        while (isContinue)
        {
            if (selectItemIndex.Count >= itemList.Count) break;

            random = Random.Range(0, selectItemList.Count);

            //被らないように
            while (selectItemIndex.Contains(random))
            {
                random = Random.Range(0, selectItemList.Count);
            }

            selectItemIndex.Add(random);


            //ガチャが継続するか
            int continueRam = Random.Range(0, 100);
            if (continueRam>=25)
            {
                isContinue = false;
            }

            //四個以上は獲得できない
            if (selectItemIndex.Count >= 4)
            {
                isContinue = false;
            }
        }

        List<ItemData> randomResult = new List<ItemData>();

        foreach (int index in selectItemIndex)
        {
            randomResult.Add(itemList[index]);
        }

        selectItemList = randomResult;
    }

    public void HackGet()
    {
        isGeted = true;
    }
}
