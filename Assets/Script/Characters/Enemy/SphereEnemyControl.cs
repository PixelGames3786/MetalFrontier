using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using SphereEnemy;
using System;
using UnityEditor.ShaderGraph;

public class SphereEnemyControl : UnitBase,IDamageable, ITargetable
{
    public SphereEnemyState.StateEnum firstState;
    public float straightTime;

    public Transform[] patrolPoints;

    public Transform target,shotPosition;

    public float HP;

    public float moveSpeed,targetMinDistance,shotChargeTime,physicalRes,beamRes;
    public string enemyTag;

    private bool isWorking;
    public bool isDied;

    //デリゲート
    private DeathHandler deathHandler;

    //ステート管理類
    private List<SphereEnemyState> states = new List<SphereEnemyState>();

    public SphereEnemyState beforeState;
    private SphereEnemyState nowState;

    public GameObject bulletPrefab,deathParticlePrefab;
    public ParticleSystem chargeParticle;

    public Collider bodyCollider;

    //弾関連
    public WeaponPartsData.AttackType bulletAttackType;
    public float bulletDamage,bulletSpeed;

    [NonSerialized]
    public Rigidbody rb;

    //探査範囲内に入ったオブジェクトをリストに格納
    [NonSerialized]
    public List<GameObject> objInSearchRange = new List<GameObject>();

    private MeshRenderer[] allMeshes; //カメラ外に出ているか判定するため、全メッシュを保存しておく

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        deathHandler = GetComponent<DeathHandler>();

        InitializeStates();

        //全メッシュを取得
        allMeshes = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWorking) nowState.OnUpdate();
    }

    //ステートの切り替え
    public void StateTranstion(SphereEnemyState.StateEnum transitState)
    {
        nowState.OnExit();

        SphereEnemyState newState = states.First(state => state.stateEnum == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        beforeState = nowState;
        nowState = newState;

        nowState.OnEnter();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        for (int i=0;i<patrolPoints.Length-1;i++)
        {
            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i+1].position);
        }

        Gizmos.DrawLine(patrolPoints[patrolPoints.Length-1].position, patrolPoints[0].position);
    }

    private void OnTriggerEnter(Collider other)
    {
        objInSearchRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        objInSearchRange.Remove(other.gameObject);
    }

    void IDamageable.Damage(AttackData attack)
    {
        if (isDied) return;

        //攻撃タイプと耐性を考慮してダメージを決定
        float damage = attack.damage;

        switch (attack.type)
        {
            case WeaponPartsData.AttackType.Physicial:

                damage = damage / (1 + physicalRes);

                break;

            case WeaponPartsData.AttackType.Beam:

                damage = damage / (1 + beamRes);

                break;
        }

        HP-=damage;

        //死亡処理
        if (HP <= 0)
        {
            print("死んだぜ！");
            Die();
        }
    }

    private void Die()
    {
        isDied = true;
        isWorking = false;

        nowState.OnDie();

        //デリゲートの呼び出し
        deathHandler.DeathInvoke();

        //死亡時
        GameObject particle = Instantiate(deathParticlePrefab);
        particle.transform.position = transform.position;

        //数秒後に破壊
        Destroy(gameObject,5f);
    }

    //カメラ内にいるかどうか
    bool ITargetable.IsVisible()
    {
        foreach (MeshRenderer renderer in allMeshes)
        {
            if (renderer.gameObject.layer == LayerMask.NameToLayer("MiniMap")) continue;

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

    bool IDamageable.CanHit()
    {
        return !isDied;
    }

    private void InitializeStates()
    {
        //ステートの初期化
        states.Add(new PatrolState(this));
        states.Add(new ChaseState(this));
        states.Add(new ShotState(this));
        states.Add(new DeathState(this));
        states.Add(new StraightState(this));

        nowState = states.First(state => state.stateEnum == firstState);

        nowState.OnEnter();
    }

    public override void UnitSetUp()
    {
        
    }

    public override void UnitActivate()
    {
        isWorking = true;
    }

    public override void UnitDeactivate()
    {
        isWorking = false;
    }
}

namespace SphereEnemy
{
    //待機ステート　指定したポイントを順番に移動する
    public class PatrolState : SphereEnemyState
    {
        private Vector3 moveDir;

        private Transform targetTrans;
        private int nowPoint = 0;


        //コンストラクタ　初期化
        public PatrolState(SphereEnemyControl control)
        {
            stateEnum = StateEnum.Patrol;

            controller = control;
        }

        public override void OnEnter()
        {
            targetTrans = controller.patrolPoints[nowPoint];

            controller.transform.LookAt(targetTrans);

            moveDir = (targetTrans.position - controller.transform.position).normalized;

            controller.rb.velocity = moveDir * controller.moveSpeed;
        }

        public override void OnUpdate()
        {
            //目標位置にたどり着いたら次の位置に移動する
            if (Vector3.Distance(controller.transform.position,targetTrans.position)<1f)
            {
                ChangeToNextPoint();
            }

            //ターゲットが索敵範囲内に入れば、追いかけるステートに移動
            CheckEnemyInRange();
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public void CheckEnemyInRange()
        {
            controller.objInSearchRange.RemoveAll(x => x == null);
            GameObject target = controller.objInSearchRange.FirstOrDefault((obj) => obj.CompareTag(controller.enemyTag));

            if (target)
            {
                controller.target = target.transform;

                controller.StateTranstion(StateEnum.Chase);
            }
        }

        private void ChangeToNextPoint()
        {
            nowPoint++;

            if (nowPoint >= controller.patrolPoints.Length) nowPoint = 0;

            targetTrans = controller.patrolPoints[nowPoint];

            controller.transform.LookAt(targetTrans);

            moveDir = (targetTrans.position - controller.transform.position).normalized;

            controller.rb.velocity = moveDir * controller.moveSpeed;
        }
    }

    //追跡ステート　見つけた敵を追いかける
    public class ChaseState : SphereEnemyState
    {
        private Transform targetTrans;

        private bool isNear; //ターゲットとの距離が一定以上なら動かなくなる

        //コンストラクタ　初期化
        public ChaseState(SphereEnemyControl control)
        {
            stateEnum = StateEnum.Chase;

            controller = control;
        }

        public override void OnEnter()
        {
            targetTrans = controller.target;
        }

        public override void OnUpdate()
        {
            ChaseTarget();
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        private void ChaseTarget() //敵を追いかける処理
        {
            //敵との距離をチェック
            if (Vector3.Distance(controller.transform.position, targetTrans.position) < controller.targetMinDistance)
            {
                controller.StateTranstion(StateEnum.Shot);

                isNear= true;
            }
            else
            {
                isNear = false;
            }

            //一定距離以上離れていたら近づく
            if (!isNear)
            {
                controller.transform.LookAt(targetTrans);
                Vector3 moveDir = (targetTrans.position - controller.transform.position).normalized;

                controller.rb.velocity = moveDir * controller.moveSpeed;
            }
            else
            {
                controller.rb.velocity = Vector3.zero;
            }
        }
    }

    //直進ステート　ボス敵から射出されたときなど
    public class StraightState : SphereEnemyState
    {
        private float straightTime,elapsedTime = 0f;

        //コンストラクタ　初期化
        public StraightState(SphereEnemyControl control)
        {
            stateEnum = StateEnum.Straight;

            controller = control;
            straightTime = control.straightTime;
        }

        public override void OnEnter()
        {
        }

        public override void OnUpdate()
        {
            MoveStraight();
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        private void MoveStraight() //まっすぐ進む
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime>straightTime)
            {
                controller.StateTranstion(StateEnum.Chase); //追いかける処理に移動
                elapsedTime = 0f;
            }

            Vector3 moveDir = controller.transform.forward;

            controller.rb.velocity = moveDir * controller.moveSpeed;
        }
    }

    //射撃ステート　ターゲットに対して弾を発射する
    public class ShotState : SphereEnemyState
    {
        private float shotElapsedTime = 0f; //経過時間

        private Transform targetTrans,shotPosi;

        private Vector3 targetPosi;

        //コンストラクタ　初期化
        public ShotState(SphereEnemyControl control)
        {
            stateEnum = StateEnum.Shot;

            controller = control;
            shotPosi = control.shotPosition;
        }

        public override void OnEnter()
        {
            targetPosi = controller.target.position;

            controller.chargeParticle.Play();
        }

        public override void OnUpdate()
        {
            shotElapsedTime += Time.deltaTime;

            if (shotElapsedTime>controller.shotChargeTime)
            {
                Shot();
            }
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);

            controller.chargeParticle.Stop();
            shotElapsedTime = 0f;
        }

        private void Shot()
        {
            GameObject bulletObj = UnityEngine.Object.Instantiate(controller.bulletPrefab);
            bulletObj.transform.position = shotPosi.position;

            Bullet bullet = bulletObj.GetComponentInChildren<Bullet>();

            Collider bulletCollider = bullet.bulletCollider;

            //自分自身とぶつからないようにする
            Physics.IgnoreCollision(bulletCollider, controller.bodyCollider, true);

            //ダメージ登録
            bullet.attackData.type = controller.bulletAttackType;
            bullet.attackData.damage = controller.bulletDamage;

            Vector3 shotVector = Vector3.zero;

            shotVector = (targetPosi - shotPosi.position).normalized;

            shotVector = shotVector* controller.bulletSpeed;

            bullet.Shot(shotVector);

            bullet.transform.LookAt(targetTrans);

            controller.chargeParticle.Stop();
            shotElapsedTime = 0f;

            controller.StateTranstion(StateEnum.Chase);
        }
    }

    //死亡ステート　Rigidbodyに重力を効かせて落ちるように
    public class DeathState : SphereEnemyState
    {
        //コンストラクタ　初期化
        public DeathState(SphereEnemyControl control)
        {
            stateEnum = StateEnum.Death;

            controller = control;
        }

        public override void OnEnter()
        {
            controller.rb.constraints = RigidbodyConstraints.None;

            controller.rb.useGravity = true;
        }

        public override void OnUpdate()
        {

        }
    }


    //基底ステートの定義
    public abstract class SphereEnemyState : IState
    {
        public enum StateEnum
        {
            Patrol,
            Chase,
            Shot,
            Straight,
            AvoidObstacle,
            Death,
        }

        public SphereEnemyControl controller;
        public StateEnum stateEnum;

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnDie()
        {

        }
    }

}