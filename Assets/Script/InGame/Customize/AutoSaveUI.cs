using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class AutoSaveUI : MonoBehaviour
{
    public TextMeshProUGUI savingUIText;
    private Sequence savingSequence; // DOTween�̃V�[�P���X

    public void StartSavingText()
    {
        savingUIText.text = "Saving";

        savingUIText.DOFade(1f, 0.3f).OnComplete(() =>
        {
            // DOTween�V�[�P���X�̏�����
            savingSequence = DOTween.Sequence();

            // �e�X�e�b�v�Ńe�L�X�g��ύX
            savingSequence.AppendCallback(() => savingUIText.text = "Saving.")
                          .AppendInterval(0.5f)
                          .AppendCallback(() => savingUIText.text = "Saving..")
                          .AppendInterval(0.5f)
                          .AppendCallback(() => savingUIText.text = "Saving...")
                          .AppendInterval(0.5f);

            // �������[�v
            savingSequence.SetLoops(-1);
        });
    }

    public void CompleteSavingText()
    {
        // �V�[�P���X���~
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
