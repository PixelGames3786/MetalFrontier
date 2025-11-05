using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class OutOfAreaUI : MonoBehaviour
{
    public TextMeshProUGUI countText;

    public CanvasGroup thisGroup;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CountDownStart()
    {
        thisGroup.DOFade(1f,0.5f);
    }

    public void CountDownEnd()
    {
        thisGroup.DOFade(0f, 0.5f).OnComplete(() => 
        {
            countText.text = "20";
        });
    }

    //1•bŒo‰ß‚·‚é‚²‚Æ‚ÉŒÄ‚Î‚ê‚é
    public void CountUpChange(int count)
    {
        countText.text = count.ToString();
    }
}
