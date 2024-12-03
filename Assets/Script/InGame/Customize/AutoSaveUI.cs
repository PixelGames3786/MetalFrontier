using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class AutoSaveUI : MonoBehaviour
{
    public TextMeshProUGUI savingUIText;
    private Sequence savingSequence; // DOTweenのシーケンス

    public void StartSavingText()
    {
        savingUIText.text = "Saving";

        savingUIText.DOFade(1f, 0.3f).OnComplete(() =>
        {
            // DOTweenシーケンスの初期化
            savingSequence = DOTween.Sequence();

            // 各ステップでテキストを変更
            savingSequence.AppendCallback(() => savingUIText.text = "Saving.")
                          .AppendInterval(0.5f)
                          .AppendCallback(() => savingUIText.text = "Saving..")
                          .AppendInterval(0.5f)
                          .AppendCallback(() => savingUIText.text = "Saving...")
                          .AppendInterval(0.5f);

            // 無限ループ
            savingSequence.SetLoops(-1);
        });
    }

    public void CompleteSavingText()
    {
        // シーケンスを停止
        if (savingSequence != null && savingSequence.IsActive())
        {
            savingSequence.Kill();

            savingUIText.text = "Save\nComplete!";


            savingSequence = DOTween.Sequence();
            savingSequence.AppendInterval(1f).Append(savingUIText.DOFade(0f,0.3f));

            savingSequence.Play();
        }
    }
}
