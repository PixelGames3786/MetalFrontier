using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyControlTest : MonoBehaviour,IDamageable,ITargetable
{
    //レガシーコントローラー
    public RobotController controller { get; private set; }

    //ステート管理類
    private List<EnemyTestState> states = new List<EnemyTestState>();

    private EnemyTestState nowState;

    public List<BodyPartsData> bodyPartsDatas = new List<BodyPartsData>();

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponPartsDic = new SerializableDictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

    //敵を感知する範囲　敵との最小・最大距離　最小高低差
    public float enemyRadius,enemyMinDis,enemyMaxDis,enemyMinElevation;
    public string enemyTag;

    public Transform target { get; set; }

    public bool isWorking = false;
    private bool isDied = false;

    public Action<string> OnDeathWithName;

    private MeshRenderer[] allMeshes;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<RobotController>();

        //ステートの初期化
        states.Add(new FindTargetState(this));
        states.Add(new ChaseState(this));
        states.Add(new ShotState(this));

        nowState = states[0];

        //全メッシュを取得
        allMeshes = GetComponentsInChildren<MeshRenderer>();

        SaveDataManager.instance.OnLoadComplete += LegacySetUp;
    }

    // Update is called once per frame
    void Update()
    {
        if(isWorking) nowState.OnUpdate();
    }

    //レガシーのデータなどをセットアップする
    public void LegacySetUp()
    {
        controller.setupControl.LegacySetUp(bodyPartsDatas, weaponPartsDic);
        controller.statusControl.StatusInitalize(bodyPartsDatas);
    }

    //ステートの切り替え
    public void StateTranstion(EnemyTestState.StateEnum transitState)
    {
        nowState.OnExit();

        EnemyTestState newState = states.First(state => state.stateEnum == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        nowState = newState;

        nowState.OnEnter();
    }

    public void SetWorking(bool val)
    {
        isWorking = val;
    }

    //レガシーの操作可能フラグを変更
    public void SetOparete(bool val)
    {
        controller.canOperate = val;
    }

    //ダメージを受ける
    public void Damage(float damageVal)
    {
        controller.statusControl.HPChange(-damageVal);

        //死亡処理
        if (controller.statusControl.hp <= 0)
        {
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

                damage = damage / (1 + controller.statusControl.nowStatus.physicalRes);

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
        isDied = true;

        SetWorking(false);
        SetOparete(false);

        nowState.OnDie();

        //デリゲートの呼び出し
        OnDeathWithName?.Invoke(gameObject.name);

        //数秒後に破壊
        Destroy(gameObject,5f);
    }


    void OnDrawGizmosSelected()
    {
        // シーンビューで範囲を可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
    }

    //カメラ内にいるかどうか
    bool ITargetable.IsVisible()
    {
        foreach (MeshRenderer renderer in allMeshes)
        {
            if (renderer.isVisible)
            {
                return true;
            }
        }

        return false;
    }

    bool ITargetable.CanTarget()
    {
        return !isDied;
    }
}

//基底ステートの定義
public abstract class EnemyTestState : IState
{
    public enum StateEnum
    {
        FindTarget,
        Chase,
        Shot
    }

    public StateEnum stateEnum;

    public EnemyControlTest enemyAI;
    protected RobotController controller;

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {

    }

    public virtual void OnUpdate()
    {

    }

    //死亡時処理
    public virtual void OnDie()
    {
        controller.moveDirInput(Vector2.zero);
        controller.EndRise();
    }
}


//待機ステート　一定範囲内にプレイヤーが入ると移動ステートに遷移
public class FindTargetState : EnemyTestState
{
    //コンストラクタ　初期化
    public FindTargetState(EnemyControlTest control)
    {
        stateEnum = StateEnum.FindTarget;

        enemyAI = control;
        controller = enemyAI.controller;
    }

    public override void OnUpdate()
    {
        CheckEnemyInRange();

    }

    //一定範囲内に敵がいないかチェックしてロックオンする
    public void CheckEnemyInRange()
    {
        //もしもターゲッティングしていたら行わない
        if (controller.statusControl.isRockOn) return;

        Transform myTrans=enemyAI.transform;

        RaycastHit[] hits = Physics.SphereCastAll(myTrans.position, enemyAI.enemyRadius, Vector3.up, 0);
        List<GameObject> enemiesInRange = new List<GameObject>();

        foreach (var hit in hits)
        {
            //敵タグがついているか確認
            if (hit.collider.CompareTag(enemyAI.enemyTag))
            {
                enemiesInRange.Add(hit.collider.gameObject);
            }
        }

        if (enemiesInRange.Count == 0) return;

        //範囲内で最も近い敵を取る
        GameObject mostNearEnemy = enemiesInRange[0];
        float distance = Vector3.Distance(myTrans.position, mostNearEnemy.transform.position);

        foreach (GameObject enemy in enemiesInRange)
        {
            float dis = Vector3.Distance(myTrans.position, enemy.transform.position);

            if (dis < distance)
            {
                mostNearEnemy = enemy;
                distance = dis;
            }
        }

        //その敵をターゲッティング
        enemyAI.target = mostNearEnemy.transform;

        controller.statusControl.SetTarget(mostNearEnemy.transform);
        controller.SetTarget(mostNearEnemy.transform);

        //追跡ステートに移動
        enemyAI.StateTranstion(StateEnum.Chase);
    }

}

//プレイヤーと一定距離を保つステート
public class ChaseState : EnemyTestState
{
    //コンストラクタ　初期化
    public ChaseState(EnemyControlTest control)
    {
        stateEnum = StateEnum.Chase;

        enemyAI = control;
        controller = enemyAI.controller;
    }

    public override void OnUpdate()
    {
        //ターゲットとの距離を計算
        float distance = Vector3.Distance(enemyAI.transform.position,enemyAI.target.position);
        float elevation = enemyAI.target.position.y-enemyAI.transform.position.y;

        //もしも一定距離以上離れていたらベクトルを取り近づく
        if (distance>enemyAI.enemyMaxDis)
        {
            Vector3 toTargetVec = (enemyAI.target.position - enemyAI.transform.position).normalized;

            //XYの入力
            controller.moveDirInput(new Vector2(toTargetVec.x,toTargetVec.z));
        }

        //もしも一定距離以上近づいたらベクトルを取り離れる
        if (distance < enemyAI.enemyMinDis)
        {
            Vector3 toTargetVec = (enemyAI.transform.position- enemyAI.target.position).normalized;

            //XYの入力
            controller.moveDirInput(new Vector2(toTargetVec.x, toTargetVec.z));
        }


        //もしも一定以上高低差があったら空を飛ぶ
        if (elevation>enemyAI.enemyMinElevation)
        {
            controller.StartRise();
        }
        else
        {
            controller.EndRise();
        }
    }

}

//待機ステート
public class ShotState : EnemyTestState
{
    //コンストラクタ　初期化
    public ShotState(EnemyControlTest controller)
    {
        stateEnum = StateEnum.Shot;

        enemyAI = controller;
    }

    public override void OnUpdate()
    {
        enemyAI.controller.LeftArmShot();
    }

}

