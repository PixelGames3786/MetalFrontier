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

        nowState = states[0];

        //�S���b�V�����擾
        allMeshes = GetComponentsInChildren<MeshRenderer>();

        SaveDataManager.instance.OnLoadComplete += LegacySetUp;
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
            Die();
        }
    }

    public void Damage(AttackData attack)
    {
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

        nowState.OnDie();

        //�f���Q�[�g�̌Ăяo��
        OnDeathWithName?.Invoke(gameObject.name);

        //���b��ɔj��
        Destroy(gameObject,5f);
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
    //�R���X�g���N�^�@������
    public ChaseState(EnemyControlTest control)
    {
        stateEnum = StateEnum.Chase;

        enemyAI = control;
        controller = enemyAI.controller;
    }

    public override void OnUpdate()
    {
        //�^�[�Q�b�g�Ƃ̋������v�Z
        float distance = Vector3.Distance(enemyAI.transform.position,enemyAI.target.position);
        float elevation = enemyAI.target.position.y-enemyAI.transform.position.y;

        //��������苗���ȏ㗣��Ă�����x�N�g�������߂Â�
        if (distance>enemyAI.enemyMaxDis)
        {
            Vector3 toTargetVec = (enemyAI.target.position - enemyAI.transform.position).normalized;

            //XY�̓���
            controller.moveDirInput(new Vector2(toTargetVec.x,toTargetVec.z));
        }

        //��������苗���ȏ�߂Â�����x�N�g������藣���
        if (distance < enemyAI.enemyMinDis)
        {
            Vector3 toTargetVec = (enemyAI.transform.position- enemyAI.target.position).normalized;

            //XY�̓���
            controller.moveDirInput(new Vector2(toTargetVec.x, toTargetVec.z));
        }


        //���������ȏ㍂�፷��������������
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

