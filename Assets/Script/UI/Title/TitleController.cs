using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private Canvas titleCanvas;

    [SerializeField]
    private CanvasGroup curtainCanvas;

    private bool sceneLoading;

    private InputAction confirmAct;

    // Start is called before the first frame update
    void Start()
    {
        curtainCanvas.DOFade(0f, 0.5f).OnComplete(() => 
        {
            confirmAct = new InputControls().UI.Confirm;

            confirmAct.performed += ConfirmButton;

            confirmAct.Enable();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmButton(InputAction.CallbackContext context)
    {
        if (!SaveDataManager.instance.isLoadComplete) return;

        curtainCanvas.gameObject.SetActive(true);

        curtainCanvas.DOFade(1f, 0.5f).OnComplete(() => 
        {
            SceneChangeManager.instance.StartCoroutine("SceneTransition", "DockScene");
        });
    }

    public void OnDisable()
    {
        confirmAct.performed -= ConfirmButton;

        confirmAct.Disable();
    }
}
