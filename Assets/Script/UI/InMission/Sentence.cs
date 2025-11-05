using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Sentence   //センテンスと読みます。文章という意味です。シナリオは文章で構成されているので、この名前にしました。
{
    public int id;              //検索するための一意な数値です。
    public string message;      //メッセージの本文です。
    public bool branch;         //選択肢があるかを判定するために使います。
    public string yesMessage;   //肯定的な選択肢です。
    public string noMessage;    //否定的な選択肢です。
    public int choseYes;        //肯定的な選択肢を選んだとき、どの文章に分岐するかのIDです。
    public int choseNo;         //否定的な選択肢を選んだとき、どの文章に分岐するかのIDです。
    public bool doConnect;      //分岐した後に収束するときなどに使います。
    public int skipId;          //収束するとき、どの文章から読むのかのIDです。
    public bool endOfTalk;      //対話が終了する最後の文章なのか判定するのに使います。
    public string sceneChange;  //シーンを移行するときに移行するシーンの名前を入れます。
    public string nextBattle;   //バトルシーンに移行するときに行うバトルを設定します。
    public string Talker;
    public string Tatie;
    public string BackGround;
    public string SE;
    public string BGM;
    public string Special;
    public bool NeedClick;
}
