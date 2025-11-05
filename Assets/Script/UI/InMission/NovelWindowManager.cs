using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class NovelWindowManager : MonoBehaviour
{
    public List<NovelSentenceData> textDatas= new List<NovelSentenceData>();

    [SerializeField] TextMeshProUGUI massage = null;

    private string showText, addText;

    private int currentSentenceID = -1, nowLineNum = 1,nowCharaNum=0;

    public float needAddTime;
    private float addTime;

    public bool canNext = true, showing,canText=false;

    public RectTransform windowRect;

    public Action onTextShowEnd; //テキストの表示が最後まで行ったら

    void Start()
    {
    }

    private void Update()
    {
        if (showing)
        {
            addTime += Time.deltaTime;

            if (addTime >= needAddTime)
            {
                string RichText = "";

                addTime = 0;

                if (showText[nowCharaNum] == '<')
                {
                    while (true)
                    {
                        RichText += showText[nowCharaNum];

                        if (showText[nowCharaNum] == '>')
                        {
                            break;
                        }

                        nowCharaNum++;
                    }

                    addText += RichText;
                }
                else
                {
                    addText += showText[nowCharaNum];
                }

                massage.text = addText;

                nowCharaNum++;

                //もし全部表示し終わったなら
                if (nowCharaNum >= showText.Length)
                {
                    addText = "";

                    nowCharaNum = 0;

                    showing = false;
                    canNext = true;

                    onTextShowEnd?.Invoke();
                }
            }
        }
    }

    public void OpenWindow()
    {
        windowRect.DOScaleX(1f,0.5f);
    }

    public void CloseWindow()
    {
        windowRect.DOScaleX(0f,0.5f);
    }

    //メッセージ表示を始める
    public void StartMessage(int messageIndex)
    {
        OpenWindow();

        nowLineNum = messageIndex;
        showText = textDatas[nowLineNum].sentence;

        showing = true;
        canText = true;
    }

    public void ReadmoreMessage()
    {
        if(!canNext ||!canText) return;

        nowLineNum++;
        showText = textDatas[nowLineNum].sentence;

        if (showText=="Close")
        {
            ReadEndMessage();
            return;
        }

        showing = true;
    }

    private void ReadEndMessage()
    {
        CloseWindow();

        canNext = false;
        canText = false;
    }

    /*

    //文章の続きを表示
    public void ReadmoreMessage()
    {
        //文章番号をもとに文を検索します。
        Sentence result = SerchSentence(currentSentenceID);

        //シーンチェンジ
        SceneChange(result);

        //喋り手のウィンドウを出す
        TalkerWindow(result);

        AboutTatie(result);

        BackGroundChange(result);

        BGM(result);

        SE(result);

        //特殊な演出がないかチェック
        StartCoroutine("CheckSpecial", result);

        //メッセージがこれ以上ない場合はダイアログUIを非アクティブにする。
        EndOfTalk(result);
        //得られたメッセージを表示します。

        if (result.message != "" && canNext)
        {
            ShowingMassage(result);
        }

        //文章番号をひとつだけ次に進めます。
        currentSentenceID++;
    }*/

    //メッセージを表示させる
    void ShowingMassage(Sentence sentence)
    {
        showText = sentence.message;
        massage.text = "";

        showing = true;
        canNext = false;

        //massage.text = sentence.message;
    }

    /*

    Sentence SerchSentence(int Id)
    {
        //シナリオのメッセージ配列の中から文章をIDで検索して取得
        Sentence result = TextDatas.sentences.First(
            (Sentence line) => { return line.id == Id; }
        );
        return result;
    }

    */

    //文章を読み飛ばします。
    //選択の結果、シナリオが分岐したのち収束するときなどに使用します。
    void Connect(Sentence sentence)
    {
        if (sentence.doConnect)
        {
            //Skip(sentence.skipId);
        }
    }

    /*
    //別の文章に読み飛ばします。
    void Skip(int id)
    {
        Sentence result = SerchSentence(id);
        currentSentenceID = result.id;
        ShowingMassage(result);
    }

    */
}
