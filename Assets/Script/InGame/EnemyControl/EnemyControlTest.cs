using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyControlTest : MonoBehaviour,IDamageable,ITargetable
{
    //���K�V�[�R���g���[���[
    public RobotController controller { get; private set; }

    //�X�e�[�g�Ǘ���
    private List<EnemyTestState> states = new List<EnemyTestState>();

    public EnemyTestState beforeState;
    private EnemyTestState nowState;

    public List<BodyPartsData> bodyPartsDatas = new List<BodyPartsData>();

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponPartsDic = new SerializableDictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

    //�G�����m����͈́@�G�Ƃ̍ŏ��E�ő勗���@�ŏ����፷
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

        //�X�e�[�g�̏�����
        states.Add(new FindTargetState(this));
        states.Add(new ChaseState(this));
        states.Add(new ShotState(this));
        states.Add(new AvoidObstacleState(this));

        nowState = states[0];

        //�S���b�V�����擾
        allMeshes = GetComponentsInChildren<MeshRenderer>();

        SaveDataManager.instance.onLoadComplete += LegacySetUp;
    }

    // Update is called once per frame
    void Update()
    {
        if(isWorking) nowState.OnUpdate();
    }

    //���K�V�[�̃f�[�^�Ȃǂ��Z�b�g�A�b�v����
    public void LegacySetUp()
    {
        controller.setupControl.LegacySetUp(bodyPartsDatas, weaponPartsDic);
        controller.statusControl.StatusInitalize(bodyPartsDatas);
    }

    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(EnemyTestState.StateEnum transitState)
    {
        nowState.OnExit();

        EnemyTestState newState = states.First(state => state.stateEnum == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

        beforeState = nowState;
        nowState = newState;

        nowState.OnEnter();
    }

    public void SetWorking(bool val)
    {
        isWorking = val;
    }

    //���K�V�[�̑���\�t���O��ύX
    public void SetOparete(bool val)
    {
        controller.canOperate = val;
    }

    //�_���[�W���󂯂�
    public void Damage(float damageVal)
    {
        controller.statusControl.HPChange(-damageVal);

        //���S����
        if (controller.statusControl.hp <= 0)
        {
            //Die();
        }
    }

    public void Damage(AttackData attack)
    {
        if (isDied) return;

        //�U���^�C�v�Ƒϐ����l�����ă_���[�W������
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

        //���S����
        if (controller.statusControl.hp <= 0)
        {
            print("���񂾂��I");
            Die();
        }
    }

    //���S����
    private void Die()
    {
        isDied = true;

        SetWorking(false);
        SetOparete(false);
        controller.SetArmLookAt(false);

        nowState.OnDie();

        //�f���Q�[�g�̌Ăяo��
        OnDeathWithName?.Invoke(gameObject.name);

        controller.Die();

        //���b��ɔj��
        //Destroy(gameObject,5f);

        Invoke("ChangeHackSlashSource", 10f);
    }

    //���S�������o��static��ݒ肵�����Ȃ��Ȃ�
    private void ChangeHackSlashSource()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().isTrigger = true;

        GameObject sourcePrefab= (GameObject)Resources.Load("Prefabs/HackSlashSource");

        HackSlashSource source= Instantiate(sourcePrefab,transform).GetComponent<HackSlashSource>();

        source.SourceSetUp(EquipedItemList());
    }


    //���ݑ������Ă���A�C�e����̕����킸�Ԃ�
    public List<ItemData> EquipedItemList()
    {
        List<ItemData> returnList = new List<ItemData>();

        returnList.AddRange(bodyPartsDatas);

        foreach (var keyValue in weaponPartsDic)
        {
            returnList.Add(keyValue.Value);
        }

        return returnList;
    }


    void OnDrawGizmosSelected()
    {
        // �V�[���r���[�Ŕ͈͂�����
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
    }

    //�J�������ɂ��邩�ǂ���
    bool ITargetable.IsVisible()
    {
        foreach (MeshRenderer renderer in allMeshes)
        {
            if(renderer.gameObject.layer==LayerMask.NameToLayer("MiniMap")) continue;

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

//���X�e�[�g�̒�`
public abstract class EnemyTestState : IState
{
    public enum StateEnum
    {
        FindTarget,
        Chase,
        Shot,
        AvoidObstacle,
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

    //���S������
    public virtual void OnDie()
    {
        controller.moveDirInput(Vector2.zero);
        controller.EndRise();
    }
}


//�ҋ@�X�e�[�g�@���͈͓��Ƀv���C���[������ƈړ��X�e�[�g�ɑJ��
public class FindTargetState : EnemyTestState
{
    //�R���X�g���N�^�@������
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

    //���͈͓��ɓG�����Ȃ����`�F�b�N���ă��b�N�I������
    public void CheckEnemyInRange()
    {
        //�������^�[�Q�b�e�B���O���Ă�����s��Ȃ�
        if (controller.statusControl.isRockOn) return;

        Transform myTrans=enemyAI.transform;

        RaycastHit[] hits = Physics.SphereCastAll(myTrans.position, enemyAI.enemyRadius, Vector3.up, 0);
        List<GameObject> enemiesInRange = new List<GameObject>();

        foreach (var hit in hits)
        {
            //�G�^�O�����Ă��邩�m�F
            if (hit.collider.CompareTag(enemyAI.enemyTag))
            {
                enemiesInRange.Add(hit.collider.gameObject);
            }
        }

        if (enemiesInRange.Count == 0) return;

        //�͈͓��ōł��߂��G�����
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

        //���̓G���^�[�Q�b�e�B���O
        enemyAI.target = mostNearEnemy.transform;

        controller.statusControl.SetTarget(mostNearEnemy.transform);
        controller.SetTarget(mostNearEnemy.transform);

        //�ǐՃX�e�[�g�Ɉړ�
        enemyAI.StateTranstion(StateEnum.Chase);
    }

}

//�v���C���[�ƈ�苗����ۂX�e�[�g
public class ChaseState : EnemyTestState
{
    private float leftArmWaitTime = 1f;
    private float leftArmElapsedTime = 0f;

    //�����Ƃ���ɂ͂܂��Ă��Ȃ����ǂ����m�F���邽�߂̕ϐ�
    private Vector3 nowPosition;
    private float stuckTime; //�����Ă��Ȃ�����

    //�R���X�g���N�^�@������
    public ChaseState(EnemyControlTest control)
    {
        stateEnum = StateEnum.Chase;

        enemyAI = control;
        controller = enemyAI.controller;
    }

    public override void OnEnter()
    {
        nowPosition= enemyAI.transform.position;
    }

    public override void OnUpdate()
    {
        KeepDistance();
        UseWeapon();
        CheckStuck();
    }

    private void UseWeapon()
    {
        leftArmElapsedTime+= Time.deltaTime;

        if (leftArmElapsedTime>=leftArmWaitTime)
        {
            enemyAI.controller.LeftArmShot();
            leftArmElapsedTime = 0f;
        }
    }

    private void KeepDistance()
    {
        //�^�[�Q�b�g�Ƃ̋������v�Z
        float distance = Vector3.Distance(enemyAI.transform.position, enemyAI.target.position);
        float elevation = enemyAI.target.position.y - enemyAI.transform.position.y;

        Vector3 toTargetDir = (enemyAI.target.position - enemyAI.transform.position).normalized;
        Vector3 localDir = enemyAI.transform.InverseTransformDirection(toTargetDir);

        //Debug.Log(distance);

        //��������苗���ȏ㗣��Ă�����x�N�g�������߂Â�
        if (distance > enemyAI.enemyMaxDis)
        {
            //XY�̓���
            controller.moveDirInput(new Vector2(localDir.x, localDir.z));
        }
        //��������苗���ȏ�߂Â�����x�N�g������藣���
        else if (distance < enemyAI.enemyMinDis)
        {
            //XY�̓���
            controller.moveDirInput(new Vector2(localDir.x*-1, localDir.z * -1));
        }
        else
        {
            //XY�̓���
            controller.moveDirInput(Vector2.zero);
        }


        //���������ȏ㍂�፷��������������
        if (elevation > enemyAI.enemyMinElevation)
        {
            controller.StartRise();
        }
        else
        {
            controller.EndRise();
        }
    }

    private void CheckStuck()
    {
        Vector3 newPosition = enemyAI.transform.position;

        if (Vector3DiffersByOne(nowPosition,newPosition))
        {
            nowPosition = newPosition;

            stuckTime = 0;
        }
        else
        {
            stuckTime += Time.deltaTime;

            if (stuckTime>=5f)
            {
                stuckTime = 0;
                enemyAI.StateTranstion(StateEnum.AvoidObstacle);
            }
        }
    }
    bool Vector3DiffersByOne(Vector3 a, Vector3 b) //Vector3�̂ǂꂩ�̒l��1f�ȏ�قȂ��Ă�����true��Ԃ�
    {
        return Mathf.Abs(a.x - b.x) >= 1.0f ||
               Mathf.Abs(a.y - b.y) >= 1.0f ||
               Mathf.Abs(a.z - b.z) >= 1.0f;
    }

}

//��Q�����������X�e�[�g�@�����_���̕����Ɉ�莞�ԓ������̃X�e�[�g�ɖ߂�
public class AvoidObstacleState : EnemyTestState
{
    private float maxMoveTime=3f, minMoveTime=10f;
    private float moveTime = 0f;
    private float moveElapsedTime = 0f;

    private float moveXDir, moveYDir;

    //�R���X�g���N�^�@������
    public AvoidObstacleState(EnemyControlTest control)
    {
        stateEnum = StateEnum.AvoidObstacle;

        enemyAI = control;
        controller = enemyAI.controller;
    }

    public override void OnEnter()
    {
        moveTime = UnityEngine.Random.Range(minMoveTime, maxMoveTime);
        moveXDir = UnityEngine.Random.Range(-1f,1f);
        moveYDir = UnityEngine.Random.Range(-1f,1f);
    }

    public override void OnUpdate()
    {
        Move();
    }

    private void Move()
    {
        //XY�̓���
        controller.moveDirInput(new Vector2(moveXDir, moveYDir));

        moveElapsedTime += Time.deltaTime;

        if (moveElapsedTime>moveTime)
        {
            moveElapsedTime = 0;

            enemyAI.StateTranstion(enemyAI.beforeState.stateEnum);
        }
    }

}

//�ҋ@�X�e�[�g
public class ShotState : EnemyTestState
{
    //�R���X�g���N�^�@������
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

