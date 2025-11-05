using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using GiantEnemy;
using System;
using Cysharp.Threading.Tasks;

public class GiantEnemyControl : UnitBase
{
    public Transform[] patrolPoints;

    public Transform target,shotPosition;

    public Animator animator;
    
    public float HP;
    public float dis;

    public float moveSpeed,dashSpeed,targetMinDistance,shotChargeTime,physicalRes,beamRes;
    public string enemyTag;

    //[NonSerialized]
    public bool isDied,isAttack,isWorking,isMoving;

    public Transform leftMissileParent, RightMissileParent;
    public MissileBullet missilePrefab;

    public Transform leftReleaseParent, rightReleaseParent;
    public GameObject sphereEnemyPrefab;

    public SerializableDictionary<string, GameObject> attackColliderDic;
    public List<DamagePoint> damagePointList;

    private DeathHandler deathHandler;

    //ステート管理類
    private List<GiantEnemyState> states = new List<GiantEnemyState>();

    public GiantEnemyState beforeState;
    private GiantEnemyState nowState;

    public GameObject bulletPrefab,deathParticlePrefab;
    public ParticleSystem chargeParticle;

    [NonSerialized]
    public Rigidbody rb;

    private MeshRenderer[] allMeshes; //カメラ外に出ているか判定するため、全メッシュを保存しておく

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        deathHandler = GetComponent<DeathHandler>();

        //ステートの初期化
        states.Add(new PatrolState(this));
        states.Add(new ChaseState(this));
        states.Add(new StampState(this));
        states.Add(new DownSwordSlashState(this));
        states.Add(new MissileState(this));
        states.Add(new ReleaseSphereEnemyState(this));
        states.Add(new DashState(this));
        states.Add(new KneelState(this));
        states.Add(new DeathState(this));

        nowState = states[0];
        nowState.OnEnter();

        //全メッシュを取得
        allMeshes = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isWorking) nowState.OnUpdate();
    }

    //ステートの切り替え
    public void StateTranstion(GiantEnemyState.StateEnum transitState)
    {
        nowState.OnExit();

        GiantEnemyState newState = states.First(state => state.stateEnum == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");
        Debug.Log(transitState);

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
        nowState.TriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        nowState.TriggerExit(other);
    }

    public void Damage(AttackData attack)
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

        HP -= damage;

        //死亡処理
        if (HP <= 0)
        {
            print("死んだぜ！");
            Die();
            return;
        }

        //もしも巡回中にダメージを食らったらプレイヤーに向かって行動を開始する
        if (nowState.stateEnum==GiantEnemyState.StateEnum.Patrol)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
            StateTranstion(GiantEnemyState.StateEnum.Chase);
        }
    }

    private void Die()
    {
        isDied = true;

        nowState.OnDie();

        //デリゲートの呼び出し
        deathHandler.DeathInvoke();

        /*
        //死亡時
        GameObject particle = Instantiate(deathParticlePrefab);
        particle.transform.position = transform.position;

        //数秒後に破壊
        Destroy(gameObject,5f);
        */
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

    public void SetMoving(bool value)
    {
        isMoving= value;

        if(!isMoving) rb.velocity= Vector3.zero;
    }

    public void LeftKneel() //左ひざをつく
    {
        animator.SetTrigger("leftKneelTrigger");
        StateTranstion(GiantEnemyState.StateEnum.Kneel);
    }

    public void RightKneel() //右ひざをつく
    {
        animator.SetTrigger("rightKneelTrigger");
        StateTranstion(GiantEnemyState.StateEnum.Kneel);
    }

    public void CallStateFunc(string funcName)
    {
        nowState.CallFunc(funcName);
    }

    public void ShowAttackCollider(string colName)
    {
        attackColliderDic[colName]?.SetActive(true);
    }

    public void HideAttackCollider(string colName)
    {
        attackColliderDic[colName]?.SetActive(false);
    }

}

namespace GiantEnemy
{
    //待機ステート　指定したポイントを順番に移動する
    public class PatrolState : GiantEnemyState
    {
        private Vector3 moveDir;

        private Transform targetTrans;
        private int nowPoint = 0;


        //コンストラクタ　初期化
        public PatrolState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Patrol;

            controller = control;
        }

        public override void OnEnter()
        {
            targetTrans = controller.patrolPoints[nowPoint];

            controller.animator.SetBool("isFrontMove",true);

            moveDir = CheckVector(targetTrans.position, controller.transform.position);

            Vector3 lookDir = new Vector3(moveDir.x, 0, moveDir.z);
            controller.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        public override void OnUpdate()
        {
            if (!controller.isMoving) return;

            //目標位置にたどり着いたら次の位置に移動する
            if (CheckDistance(controller.transform.position,targetTrans.position)<1f)
            {
                controller.rb.velocity = Vector3.zero;
                ChangeToNextPoint();
            }
            else
            {
                //移動する
                moveDir = CheckVector(targetTrans.position,controller.transform.position)*controller.moveSpeed;
                controller.rb.velocity = new Vector3(moveDir.x,controller.rb.velocity.y,moveDir.z);
            }
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        private void ChangeToNextPoint()
        {
            nowPoint++;

            if (nowPoint >= controller.patrolPoints.Length) nowPoint = 0;

            targetTrans = controller.patrolPoints[nowPoint];

            moveDir = CheckVector(targetTrans.position, controller.transform.position);

            Vector3 lookDir = new Vector3(moveDir.x, 0, moveDir.z);
            controller.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        //高さを考慮しない方向ベクトル
        private Vector3 CheckVector(Vector3 one,Vector3 second)
        {
            one = new Vector3(one.x, 0, one.z);
            second = new Vector3(second.x, 0, second.z);

            return (one-second).normalized;
        }

        //高さを考慮しない距離
        private float CheckDistance(Vector3 one,Vector3 second)
        {
            one = new Vector3(one.x,0,one.z);
            second=new Vector3(second.x,0,second.z);

            return Vector3.Distance(one,second);
        }
    }

    //追跡ステート　見つけた敵を追いかける
    public class ChaseState : GiantEnemyState
    {
        private Transform thisTrans,targetTrans;
        private bool isNear; //ターゲットとの距離が一定以上なら動かなくなる

        private Vector3 moveDir;

        private float chaseTime,chaseElapsedTime=0f;

        //コンストラクタ　初期化
        public ChaseState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Chase;

            controller = control;
            thisTrans = controller.transform;
        }
        public override void OnEnter()
        {
            targetTrans = controller.target;

            chaseTime = UnityEngine.Random.Range(5f,10f);
            chaseElapsedTime = 0f;

            controller.animator.SetBool("isFrontMove", true);
        }

        public override void OnUpdate()
        {
            chaseElapsedTime += Time.deltaTime;

            if (chaseElapsedTime>chaseTime)
            {
                ActionCheck();
            }
            else
            {
                ChaseTarget();
            }
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public override void OnExit()
        {
            chaseElapsedTime = 0f;
        }

        private void ChaseTarget() //敵を追いかける処理
        {
            //敵との距離をチェック
            if (CheckDistance(thisTrans.position, targetTrans.position) < controller.targetMinDistance)
            {
                //近づいていたら行動に移る
                ActionCheck();
            }
            else
            {
                if (!controller.isMoving) return;

                //一定距離以上離れていたら近づく
                moveDir = CheckVector(targetTrans.position, thisTrans.position) * controller.moveSpeed;
                controller.rb.velocity = new Vector3(moveDir.x, controller.rb.velocity.y, moveDir.z);

                Vector3 lookDir = new Vector3(moveDir.x, 0, moveDir.z);
                thisTrans.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        private void ActionCheck() //距離によって行動を変える
        {
            float dis = CheckDistance(thisTrans.position, targetTrans.position);

            controller.animator.SetBool("isFrontMove", false);
            controller.rb.velocity = Vector3.zero;

            if (dis<30) //近接ならば
            {
                int random = UnityEngine.Random.Range(0, 2);

                switch (random)
                {
                    //スタンプ攻撃
                    case 0:
                        controller.StateTranstion(StateEnum.Stamp);
                        break;

                    //下段剣攻撃
                    case 1:
                        controller.StateTranstion(StateEnum.DownSwordSlash);
                        break;

                }
            }
            else if (dis<100)
            {
                int random = UnityEngine.Random.Range(0,3);

                switch (random)
                {
                    //ミサイル発射
                    case 0:
                        controller.StateTranstion(StateEnum.Missile);
                        break;

                    //ダッシュ
                    case 1:
                        controller.StateTranstion(StateEnum.Dash);
                        break;

                    //球体敵発射
                    case 2:
                        controller.StateTranstion(StateEnum.ReleaseSphere);
                        break;
                }
            }
            else
            {
                controller.animator.SetBool("isFrontMove", true);

                chaseTime = UnityEngine.Random.Range(5f, 10f);
                chaseElapsedTime = 0f;
            }
        }

        //高さを考慮しない方向ベクトル
        private Vector3 CheckVector(Vector3 one, Vector3 second)
        {
            one = new Vector3(one.x, 0, one.z);
            second = new Vector3(second.x, 0, second.z);

            return (one - second).normalized;
        }

        //高さを考慮しない距離
        private float CheckDistance(Vector3 one, Vector3 second)
        {
            one = new Vector3(one.x, 0, one.z);
            second = new Vector3(second.x, 0, second.z);

            controller.dis = Vector3.Distance(one, second);

            return Vector3.Distance(one, second);
        }
    }

    //スタンプステート
    public class StampState : GiantEnemyState
    {
        enum TargetPosition //自身から見た敵が左右どちらにいるのか
        {
            straight, //真正面or真後ろ
            left,
            right,
        }

        private Transform thisTrans, targetTrans;

        //コンストラクタ　初期化
        public StampState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Stamp;
            controller = control;

            thisTrans = controller.transform;

            actionDic = new Dictionary<string, Action>()
            {
                {"StampEnd",StampEnd },
            };
        }

        public override void OnEnter()
        {
            targetTrans = controller.target;

            TargetPosition tagPosi = GetTargetPosition();

            switch (tagPosi)
            {
                case TargetPosition.left:

                    controller.animator.SetTrigger("leftStampTrigger");
                    controller.isAttack = true;

                    break;

                case TargetPosition.right:

                    controller.animator.SetTrigger("rightStampTrigger");
                    controller.isAttack = true;

                    break;
            }
        }

        public override void OnUpdate()
        {
            if (!controller.isAttack) controller.StateTranstion(StateEnum.Chase);
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        private void StampEnd()
        {
            controller.StateTranstion(StateEnum.Chase);
        }

        private TargetPosition GetTargetPosition()
        {
            // ターゲットへの方向ベクトルを計算
            Vector3 directionToTarget = (targetTrans.position - thisTrans.position).normalized;

            // 自身のforwardとターゲット方向のベクトルを比較
            float dotProduct = Vector3.Dot(thisTrans.right, directionToTarget);

            // 結果を判定
            if (dotProduct > 0)
            {
                return TargetPosition.right;
            }
            else if (dotProduct < 0)
            {
                return TargetPosition.left;

            }
            else
            {
                return TargetPosition.straight;
            }
        }
    }

    //下段剣攻撃ステート
    public class DownSwordSlashState : GiantEnemyState
    {
        private Transform thisTrans, targetTrans;

        //コンストラクタ　初期化
        public DownSwordSlashState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.DownSwordSlash;
            controller = control;

            thisTrans = controller.transform;

            actionDic = new Dictionary<string, Action>()
            {
                {"AttackEnd",AttackEnd },
            };
        }

        public override void OnEnter()
        {
            targetTrans = controller.target;

            controller.animator.SetTrigger("downSlashTrigger");
            controller.isAttack = true;
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        private void AttackEnd()
        {
            controller.StateTranstion(StateEnum.Chase);
        }

    }

    //ミサイル発射ステート
    public class MissileState : GiantEnemyState
    {
        private Transform thisTrans, targetTrans;

        //コンストラクタ　初期化
        public MissileState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Missile;
            controller = control;

            thisTrans = controller.transform;

            actionDic = new Dictionary<string, Action>()
            {
                { "MissileShot",MissileShot},
                { "MissileEnd",MissileEnd},
            };
        }

        public override void OnEnter()
        {
            targetTrans = controller.target;

            controller.animator.SetTrigger("missileStandBy");
        }

        public override void OnUpdate()
        {
            
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public async void MissileShot()
        {
            for (int i=0;i<controller.leftMissileParent.childCount;i++)
            {
                if (controller.isDied) break; //発射中に死亡したら止める

                //左から出す
                Transform firePoint = controller.leftMissileParent.GetChild(i);
                MissileBullet missile = GameObject.Instantiate(controller.missilePrefab);

                missile.transform.position = firePoint.position;

                //自身のコライダーとぶつからないようにする
                //自身に当たらないように
                Physics.IgnoreCollision(missile.bulletCol, controller.GetComponent<Collider>(), true);

                missile.Shot(targetTrans, 50);


                //右から出す
                firePoint = controller.RightMissileParent.GetChild(i);
                missile = GameObject.Instantiate(controller.missilePrefab);

                missile.transform.position = firePoint.position;

                //自身のコライダーとぶつからないようにする
                //自身に当たらないように
                Physics.IgnoreCollision(missile.bulletCol, controller.GetComponent<Collider>(), true);

                missile.Shot(targetTrans, 50);

                await UniTask.Delay(200);
            }

            controller.animator.SetTrigger("missileEnd");
        }

        private void MissileEnd()
        {
            controller.StateTranstion(StateEnum.Chase);
        }
    }

    //球体敵放出ステート
    public class ReleaseSphereEnemyState : GiantEnemyState
    {
        private MissionManager missionManage;

        private Transform thisTrans, targetTrans;

        //コンストラクタ　初期化
        public ReleaseSphereEnemyState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.ReleaseSphere;
            controller = control;

            thisTrans = controller.transform;

            actionDic = new Dictionary<string, Action>()
            {
                { "ReleaseStart",ReleaseStart},
                { "ReleaseEnd",ReleaseEnd},
            };

            missionManage = GameObject.FindObjectOfType<MissionManager>();
        }

        public override void OnEnter()
        {
            targetTrans = controller.target;

            controller.animator.SetTrigger("releaseStandBy");
        }

        public override void OnUpdate()
        {

        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public async void ReleaseStart()
        {
            for (int i = 0; i < 3; i++)
            {
                if (controller.isDied) break; //発射中に死亡したら止める

                //左から出す
                Transform firePoint = controller.leftReleaseParent;
                SphereEnemyControl enemy = GameObject.Instantiate(controller.sphereEnemyPrefab).GetComponent<SphereEnemyControl>();

                enemy.transform.position= firePoint.position;
                enemy.transform.rotation = firePoint.rotation;

                enemy.firstState = SphereEnemy.SphereEnemyState.StateEnum.Straight;
                enemy.target = controller.target;

                //MissionManagerに登録
                missionManage.AddUnit(enemy);
                missionManage.AddDeathHandler(enemy.GetComponent<DeathHandler>());

                //右から出す
                firePoint = controller.rightReleaseParent;
                enemy = GameObject.Instantiate(controller.sphereEnemyPrefab).GetComponent<SphereEnemyControl>();

                enemy.transform.position = firePoint.position;
                enemy.transform.rotation = firePoint.rotation;

                enemy.firstState = SphereEnemy.SphereEnemyState.StateEnum.Straight;
                enemy.target = controller.target;

                //MissionManagerに登録
                missionManage.AddUnit(enemy);
                missionManage.AddDeathHandler(enemy.GetComponent<DeathHandler>());

                await UniTask.Delay(300);
            }

            controller.animator.SetTrigger("releaseEnd");
        }

        private void ReleaseEnd()
        {
            controller.StateTranstion(StateEnum.Chase);
        }
    }

    //ダッシュステート
    public class DashState : GiantEnemyState
    {
        private Transform thisTrans, targetTrans;

        private Vector3 moveDir;

        private float dashTime, dashElapsedTime = 0f;
        private bool isDashing;

        //コンストラクタ　初期化
        public DashState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Dash;
            controller = control;

            thisTrans = controller.transform;

            actionDic = new Dictionary<string, Action>()
            {
                { "DashStart",DashStart},
                { "DashEnd",DashEnd},
            };
        }

        public override void OnEnter()
        {
            isDashing = false;
            targetTrans = controller.target;

            controller.animator.SetBool("isFrontDash", true);

            moveDir = CheckVector(targetTrans.position, thisTrans.position) * controller.dashSpeed;

            Vector3 lookDir = new Vector3(moveDir.x, 0, moveDir.z);
            thisTrans.rotation = Quaternion.LookRotation(lookDir);

            dashTime = UnityEngine.Random.Range(2f, 5f);
            dashElapsedTime = 0f;
        }

        public override void OnUpdate()
        {
            if (!isDashing) return;

            dashElapsedTime += Time.deltaTime;

            if (dashElapsedTime > dashTime)
            {
                isDashing = false;
                controller.animator.SetBool("isFrontDash", false);
            }
            else
            {
                Dash();
            }
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public override void TriggerExit(Collider other)
        {
            //作戦エリア外に出たなら即ダッシュ終了する
            if (other.tag == "OperationArea")
            {
                isDashing = false;
                controller.animator.SetBool("isFrontDash", false);
            }
        }

        private void DashStart()
        {
            isDashing = true;
        }

        private void Dash() //ダッシュ処理
        {
            controller.rb.velocity = new Vector3(moveDir.x, controller.rb.velocity.y, moveDir.z);
        }

        private void DashEnd() //ダッシュ終了
        {
            controller.StateTranstion(StateEnum.Chase);
        }

        //高さを考慮しない方向ベクトル
        private Vector3 CheckVector(Vector3 one, Vector3 second)
        {
            one = new Vector3(one.x, 0, one.z);
            second = new Vector3(second.x, 0, second.z);

            return (one - second).normalized;
        }
    }

    //膝をつくステート
    public class KneelState : GiantEnemyState
    {
        private float kneelElapsedTime=0f;

        //コンストラクタ　初期化
        public KneelState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Kneel;

            controller = control;
        }

        public override void OnEnter()
        {
            kneelElapsedTime = 0f;
        }

        public override void OnDie()
        {
            controller.StateTranstion(StateEnum.Death);
        }

        public override void OnUpdate()
        {
            kneelElapsedTime += Time.deltaTime;

            if (kneelElapsedTime>5f)
            {
                kneelElapsedTime = 0f;

                controller.animator.SetTrigger("kneelEnd");
                controller.StateTranstion(StateEnum.Chase);
            }
        }
    }

    //死亡ステート
    public class DeathState : GiantEnemyState
    {
        //コンストラクタ　初期化
        public DeathState(GiantEnemyControl control)
        {
            stateEnum = StateEnum.Death;

            controller = control;
        }

        public override void OnEnter()
        {
            //DamagePointを無効に
            foreach (DamagePoint point in controller.damagePointList)
            {
                point.canDamage = false;
                point.canTarget = false;
            }

            controller.animator.SetTrigger("deadTrigger");
        }

        public override void OnUpdate()
        {

        }
    }


    //基底ステートの定義
    public abstract class GiantEnemyState : IState
    {
        public enum StateEnum
        {
            Patrol,
            Chase,
            DownSwordSlash,
            Stamp,
            Missile,
            ReleaseSphere,
            Dash,
            Kneel,
            Death,
        }

        public GiantEnemyControl controller;
        public StateEnum stateEnum;

        //呼べる関数をまとめるDictionary
        protected Dictionary<string, Action> actionDic;

        //引数ありの関数をまとめるDic
        protected Dictionary<string, Action<object[]>> actionDicWithArg;

        public void CallFunc(string FuncName)
        {
            if (!actionDic.ContainsKey(FuncName))
            {
                Debug.Log($"{stateEnum.ToString()}には{FuncName}関数がありません！");
                return;
            }

            Action action = actionDic[FuncName];

            action.Invoke();
        }

        public void CallFuncArg(string FuncName, object[] args)
        {
            Action<object[]> action = actionDicWithArg[FuncName];

            if (action != null)
            {
                action.Invoke(args);
            }
            else
            {
                throw new System.Exception("呼ぶ関数がないぜ！");
            }
        }

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

        public virtual void TriggerEnter(Collider other)
        {

        }

        public virtual void TriggerExit(Collider other)
        {

        }
    }
}