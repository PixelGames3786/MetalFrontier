using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public class RobotPlayerInput : MonoBehaviour,IDamageable
{
    private MissionManager missionManager;

    //通常カメラとロックオンカメラ
    [SerializeField]
    private CinemachineVirtualCamera normalCam, TargetCam;

    //レガシーコントローラー
    public RobotController controller { get; private set; }
    private PlayerUIController uiController;

    // 2軸入力を受け取る想定のAction
    private InputAction moveAction, LArmShotAct, RArmShotAct, boostAct, jumpAct, riseAct,camResetAct,LShoulderAct,RShoulderAct;
    private InputAction LArmLongAct, RArmLongAct, LShoulderLongAct, RShoulderLongAct; //各武器位置の長押しを検知するAction

    private Dictionary<InputAction, Action<InputAction.CallbackContext>> actionMap; //InputActionと対応する関数を入れる辞書 1か0のActionのみ
    private Dictionary<InputAction, Action<InputAction.CallbackContext>> canselActionMap;

    //一定範囲内の敵を取得するための半径
    public float enemyRadius,enemyRockOffRadius,rotateFactor,camResetTime;

    //ロックオン範囲に入っているターゲット一覧
    private List<Transform> targets=new List<Transform>();

    //最も近い敵
    public Transform mostNearEnemy { get; private set; }

    private Transform lookObj,target;
    private Vector3 camFrontVector;

    //武器長押しフラグ
    private bool IsLArmLongPress,IsRArmLongPress,IsLShoulderLongPress,IsRShoulderLongPress;

    private bool camReseting,isFindingTarget=true;

    private bool isWorking=false;

    //敵を検索する際のタグ
    public string enemyTag;

    //デリゲート類
    public Action<string> OnDeathWithName;
    public Action<int> OutOfAreaCountDown;

    //ミッション範囲外に出た際のカウントダウン　10秒外にいるとミッション失敗
    private bool IsOutOfArea;
    private int maxOutOfAreaCount=20,outOfAreaCount;
    private Coroutine countdownCoroutine; // コルーチンの参照を保持


    // Start is called before the first frame update
    void Awake()
    {
        missionManager = FindObjectOfType<MissionManager>();

        controller = GetComponent<RobotController>();
        uiController = GetComponent<PlayerUIController>();

        RegisterAction();

        TestControls testControl = new TestControls();
    }

    private void Start()
    {
        outOfAreaCount = maxOutOfAreaCount;

        //注視点を設定
        Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 5000);
        Vector3 lookPosi = Camera.main.ScreenToWorldPoint(screenPoint);

        camFrontVector = lookPosi;

        lookObj = new GameObject("LookObject").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWorking) return;

        Vector2 inputVec = moveAction.ReadValue<Vector2>();

        controller.moveDirInput(inputVec);

        //範囲内の敵をチェック
        CheckEnemyInRange();
        CheckEnemyOutRange();
        GetMostNearEnemy();

        //RotateToTarget(inputVec);

        LookToCameraPoint();


        //武器を長押し中なら
        if (IsLArmLongPress) controller.LeftArmShot();
        if (IsRArmLongPress) controller.RightArmShot();
        if (IsLShoulderLongPress) controller.LeftShoulderShot();
        if (IsRShoulderLongPress) controller.RightShoulderShot();

    }

    //一定範囲内に敵がいないかチェックしてロックオンする
    public void CheckEnemyInRange()
    {
        //もしもターゲッティングしていたら行わない
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,enemyRadius,Vector3.up,0);
        List<Transform> enemiesInRange = new List<Transform>();

        foreach (var hit in hits)
        {
            //敵タグがついているか確認
            if (hit.collider.CompareTag(enemyTag))
            {
                ITargetable targetable = hit.collider.GetComponent<ITargetable>();

                if (targetable==null)
                {
                    continue;
                }

                //既にターゲッティングしていないかつ現在ターゲッティング可能ならばターゲッティングする
                if (!targets.Contains(hit.transform) && targetable.CanTarget())
                {
                    targets.Add(hit.transform);
                }

                //enemiesInRange.Add(hit.collider.transform);
            }
        }

        //TargetCam.LookAt = mostNearEnemy.transform;
        //TargetCam.Priority = 11;
    }

    //ロックオンした敵が範囲外に出たらロックオンを外す
    private void CheckEnemyOutRange()
    {
        List<Transform> removeList = new List<Transform>();

        foreach (Transform enemy in targets)
        {
            //もしカメラ外に出ているなら問答無用でターゲットから外す
            if (!enemy.GetComponent<ITargetable>().IsVisible())
            {
                removeList.Add(enemy);

                continue;
            }

            //ターゲット不能になっていたら外す
            if (!enemy.GetComponent<ITargetable>().CanTarget())
            {
                removeList.Add(enemy);
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance>enemyRockOffRadius)
            {
                removeList.Add(enemy);
            }
        }

        foreach (Transform remove in removeList)
        {
            targets.Remove(remove);
        }

        //ターゲット一覧に何らかの変更があったならターゲットUIの更新を行う
        uiController.SetRockOnUI(targets);
    }

    //ロックオンしている敵の中で最も近い敵を取得
    private void GetMostNearEnemy()
    {
        if (targets.Count == 0)
        {
            mostNearEnemy = null;
            controller.SetTarget(null);
            return;
        }

        Transform mostNear = targets[0];
        //範囲内で最も近い敵を取る
        float distance = Vector3.Distance(transform.position, mostNear.transform.position);

        foreach (Transform enemy in targets)
        {
            float dis = Vector3.Distance(transform.position, enemy.transform.position);

            if (dis < distance)
            {
                mostNear = enemy;
                distance = dis;
            }
        }

        //もしも最も近い敵が変わっていたらデリゲートを呼ぶ
        if (mostNear!=mostNearEnemy)
        {
            mostNearEnemy = mostNear;

            controller.SetTarget(mostNearEnemy);
        }
    }

    void OnDrawGizmosSelected()
    {
        // シーンビューで範囲を可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
    } //索敵範囲をUI上で表示する

    //ターゲットの方向を向く
    private void RotateToTarget(Vector2 input)
    {
        if (target)
        {
            if (camReseting) return;

            lookObj.position = camFrontVector;
            normalCam.LookAt = lookObj;
        }
        else
        {
            transform.Rotate(Vector3.up, input.x * rotateFactor);
        }
    }

    //レガシーのデータなどをセットアップする
    public void LegacySetUp()
    {
        //データ取得
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //ボディパーツ反映
        List<BodyPartsData> bodyParts = new List<BodyPartsData>();
        Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponParts = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

        //全パーツのデータ取得
        foreach (KeyValuePair<BodyPartsData.PartsType, HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        //武器パーツ反映
        //パーツデータ取得
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi, HavingItem> keyValue in settingData.WeaponsNumber)
        {
            if (keyValue.Value == null) continue;

            WeaponPartsData data = (WeaponPartsData)keyValue.Value.itemData;
            weaponParts[keyValue.Key]=data;
        }

        controller.setupControl.LegacySetUp(bodyParts,weaponParts);
        controller.statusControl.StatusInitalize(bodyParts);

        uiController.UIInitlaize(controller.statusControl);
    }

    //動作状態を変更
    public void SetWorking(bool val)
    {
        isWorking = val;
    }

    //レガシーの操作可能フラグを変更
    public void SetOparete(bool val)
    {
        controller.canOperate = val;
    }

    //InputActionの登録を行う
    public void RegisterAction()
    {
        TestControls testControl = new TestControls();

        moveAction = testControl.Player.Move;

        actionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
        {
            { testControl.Player.LeftArmPress, LeftArmShot },
            { testControl.Player.RightArmPress, RightArmShot },
            { testControl.Player.LeftShoulderPress, LeftShoulderShot },
            { testControl.Player.RightShoulderPress, RightShoulderShot },
            { testControl.Player.LeftArmLongPress,StartLeftArmLongPress},
            { testControl.Player.RightArmLongPress,StartRightArmLongPress},
            { testControl.Player.LeftShoulderLongPress,StartLeftShoulderLongPress},
            { testControl.Player.RightShoulderLongPress,StartRightShoulderLongPress},
            { testControl.Player.Boost, StartBoost },
            { testControl.Player.Jump, OnJump },
            { testControl.Player.Rise, StartRise },
            { testControl.Player.CameraReset,CameraReset},
        };

        canselActionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
        {
            { testControl.Player.LeftArmLongPress,EndLeftArmLongPress},
            { testControl.Player.RightArmLongPress,EndRightArmLongPress},
            { testControl.Player.LeftShoulderLongPress,EndLeftShoulderLongPress},
            { testControl.Player.RightShoulderLongPress,EndRightShoulderLongPress},
            { testControl.Player.Boost,EndBoost},
            { testControl.Player.Rise,EndRise},
        };

        //すべてのアクションを一括登録(performed)
        foreach (var entry in actionMap)
        {
            entry.Key.performed += entry.Value;
        }

        //全てのアクションを一括登録(canseled)
        foreach (var entry in canselActionMap)
        {
            entry.Key.canceled += entry.Value;
        }
    }

    //ダメージを受ける
    public void Damage(float damageVal)
    {
        controller.statusControl.HPChange(-damageVal);

        //死亡処理
        if (controller.statusControl.hp<=0)
        {
            print("死んだぜ！");
            Die();
        }
    }

    public void Damage(AttackData attack)
    {
        //攻撃タイプと耐性を考慮してダメージを決定
        float damage = attack.damage;

        switch (attack.type)
        {
            case WeaponPartsData.AttackType.Physicial:

                damage = damage / (1+controller.statusControl.nowStatus.physicalRes);

                break;

            case WeaponPartsData.AttackType.Beam:

                damage = damage / (1 + controller.statusControl.nowStatus.beamRes);

                break;
        }

        controller.statusControl.HPChange(-damage);

        //死亡処理
        if (controller.statusControl.hp <= 0)
        {
            print("死んだぜ！");
            Die();
        }
    }

    //死亡処理
    private void Die()
    {
        SetWorking(false);
        SetOparete(false);

        //デリゲートの呼び出し
        OnDeathWithName?.Invoke(gameObject.name);

        //数秒後に破壊
        Destroy(gameObject, 5f);
    }

    IEnumerator CountdownCoroutine()
    {
        while (outOfAreaCount > 0)
        {
            outOfAreaCount--;
            OutOfAreaCountDown?.Invoke(outOfAreaCount); //登録したデリゲートを呼び出し

            yield return new WaitForSeconds(1f); // 1秒待機
        }

        //任務失敗
        FindObjectOfType<MissionManager>().MissionFail();
    }

    private void OnTriggerExit(Collider other)
    {
        //作戦エリア外に出たなら
        if (other.tag=="OperationArea")
        {
            IsOutOfArea = true;

            uiController.StartCountDown();

            if (countdownCoroutine==null)
            {
                countdownCoroutine = StartCoroutine(CountdownCoroutine());
            }
        }

        //作戦最大エリア外に出たなら即座にミッション失敗
        if (other.tag=="OperationMaxArea")
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            //任務失敗
            FindObjectOfType<MissionManager>().MissionFail();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //作戦エリア内に入ったなら
        if (other.tag == "OperationArea")
        {
            IsOutOfArea = false;

            uiController.EndCountDown();
            if (countdownCoroutine != null) 
            { 
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            outOfAreaCount = maxOutOfAreaCount;
        }
    }

    //左手武器射撃ボタンが押されたとき
    private void LeftArmShot(InputAction.CallbackContext context)=>controller.LeftArmShot();

    //右手武器射撃ボタンが押されたとき
    private void RightArmShot(InputAction.CallbackContext context)=>controller.RightArmShot();

    //左肩武器使用ボタンが押されたとき
    private void LeftShoulderShot(InputAction.CallbackContext context)=> controller.LeftShoulderShot();

    //右肩武器使用ボタンが押されたとき
    private void RightShoulderShot(InputAction.CallbackContext context)=> controller.RightShoulderShot();

    //左腕武器長押し開始/終了
    private void StartLeftArmLongPress(InputAction.CallbackContext context)=> IsLArmLongPress = true;
    private void EndLeftArmLongPress(InputAction.CallbackContext context)=> IsLArmLongPress = false;

    //右腕武器長押し開始/終了
    private void StartRightArmLongPress(InputAction.CallbackContext context) => IsRArmLongPress = true;
    private void EndRightArmLongPress(InputAction.CallbackContext context) => IsRArmLongPress = false;

    //左肩武器長押し開始/終了
    private void StartLeftShoulderLongPress(InputAction.CallbackContext context) => IsLShoulderLongPress = true;
    private void EndLeftShoulderLongPress(InputAction.CallbackContext context) => IsLShoulderLongPress = false;

    //右肩武器長押し開始/終了
    private void StartRightShoulderLongPress(InputAction.CallbackContext context) => IsRShoulderLongPress = true;
    private void EndRightShoulderLongPress(InputAction.CallbackContext context) => IsRShoulderLongPress = false;

    //ブーストボタン長押し開始/終了
    private void StartBoost(InputAction.CallbackContext context)=> controller.StartBoost();
    private void EndBoost(InputAction.CallbackContext context)=> controller.EndBoost();

    //ジャンプボタンが押されたとき
    private void OnJump(InputAction.CallbackContext context)
    {
        controller.OnJump();
    }

    //ジャンプボタン長押し（上昇）がされたとき
    private void StartRise(InputAction.CallbackContext context)
    {
        controller.StartRise();
    }

    //ジャンプボタン長押しが終わったら
    private void EndRise(InputAction.CallbackContext context)
    {
        controller.EndRise();
    }

    private void CameraReset(InputAction.CallbackContext context)
    {
        CameraReset();
    }

    private void LookToCameraPoint()
    {
        //注視点を設定
        Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 5000);
        Vector3 lookPosi = Camera.main.ScreenToWorldPoint(screenPoint);

        // Y軸を無視するために注視点のY座標を現在のオブジェクトのY座標に固定
        lookPosi.y = transform.position.y;

        transform.LookAt(lookPosi);
    }

    //カメラ方向リセット
    public async void CameraReset()
    {
        if (!target)
        {
            isFindingTarget = true;

            camReseting = true;

            Vector3 forwardNormal = transform.forward.normalized;
            camFrontVector = new Vector3(forwardNormal.x * 5000, camFrontVector.y, forwardNormal.z * 5000);

            //lookObj.position = camFrontVector;
            //normalCam.LookAt = lookObj;

            Debug.DrawRay(transform.position, camFrontVector, Color.red);
            Debug.DrawRay(transform.position, transform.forward, Color.blue);

            await UniTask.Delay(Mathf.CeilToInt(camResetTime * 1000));

            camReseting = false;
        }
        else
        {
            print("カメラリセット");

            isFindingTarget = false;

            target = null;

            //ロックオン解除
            controller.statusControl.SetTarget(null);
            controller.SetTarget(null);

            uiController.SetRockOnUI(null);

            // 通常カメラにロックオンカメラと同じ位置と向きをセット
            CinemachinePOV pov = normalCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                // lockOnCameraの現在の向きをもとにPOVの値を設定
                pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
                pov.m_VerticalAxis.Value = 20;
            }

            TargetCam.LookAt = null;
            TargetCam.Priority = 9;

        }

    }


    //InputActionの有効化など
    private void OnDestroy()
    {
        moveAction.Dispose();

        foreach (var entry in actionMap)
        {
            entry.Key.Dispose();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Dispose();
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();

        foreach (var entry in actionMap)
        {
            entry.Key.Enable();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Enable();
        }
    }

    private void OnDisable()
    {
        moveAction.Disable();

        foreach (var entry in actionMap)
        {
            entry.Key.Disable();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Disable();
        }
    }

}
