//Player:玩家1231243124214214214
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    [Header("Attack details")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = 0.2f;
    public bool isBusy { get; private set; }
    [HideInInspector] public Transform lastDamageSource;//上次受到伤害的来源
    
    [Header("Move Info")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float swordReturnImpact = 7f;
    private float defualtMoveSpeed;
    private float defualtJumpForce;


    [Header("Dash Info")]
    public float dashSpeed=25f;
    public float dashDuration=0.2f;
    private float defaultDashSpeed;
    public float dashDir {  get; private set; }

    public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }

    #region 状态
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCounterAttackState counterAttack { get; private set; }

    public PlayerAimSwordState aimSword { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }

    public PlayerBlackholeState blackhole { get; private set; }
    public PlayerDeadState deadState { get; private set; }

    public PlayerFX FX { get; private set; }

    #endregion

    //创建对象1231·24214214124
    protected override void Awake()
    {
        base.Awake();

        StateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, StateMachine, "Idle");
        moveState = new PlayerMoveState(this, StateMachine, "Move");
        jumpState = new PlayerJumpState(this, StateMachine, "Jump");
        airState = new PlayerAirState(this, StateMachine, "Jump");
        dashState = new PlayerDashState(this, StateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, StateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, StateMachine, "Jump");
        primaryAttack = new PlayerPrimaryAttackState(this, StateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, StateMachine,  "CounterAttack");

        aimSword = new PlayerAimSwordState(this, StateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, StateMachine, "CatchSword");

        blackhole = new PlayerBlackholeState(this, StateMachine, "Jump");
        deadState = new PlayerDeadState(this, StateMachine, "Die");

    }

    // 设置初始状态
    protected override void Start()
    {
        base.Start();
        FX = GetComponent<PlayerFX>();
        skill = SkillManager.instance;

        StateMachine.Initialize(idleState);

        defualtMoveSpeed = moveSpeed;
        defualtJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }

    // 更新
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;

        base.Update();

        StateMachine.currentState.Update();

        CheckForDashInput();

        if (Input.GetKeyDown(KeyCode.F) && skill.crystal.crystalUnlocked)
            skill.crystal.CanUseSkill();//肯定要改到其他地方

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Inventory.instance.UseFlask();
    }

    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);

        Invoke("ReturnDefaultSpeed", _slowDuration);
    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defualtMoveSpeed;
        jumpForce = defualtJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    public void CatchTheSword()
    {
        StateMachine.ChangeState(catchSword);

        Destroy(sword);
    }


    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }

    //设置触发器
    public void AnimationTrigger() => StateMachine.currentState.AnimationFinishTrigger();

    //检查冲刺输入
    public void CheckForDashInput()
    {
        if (skill.dash.dashUnlocked == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = Input.GetAxisRaw("Horizontal");

            if (dashDir == 0)
                dashDir = facingDir;

            StateMachine.ChangeState(dashState);
        }
    }



    public override void Die()
    {
        base.Die();
        // 在玩家死亡时立即保存游戏状态
        //SaveManager.instance.SaveGame();
        StateMachine.ChangeState(deadState);
    }

    protected override void SetupZeroKnockbackPower()
    {
        knockbackPower = new Vector2(0, 0);
        // 确保在击退结束后重置玩家速度
        if (!isKnocked)
            rb.linearVelocity = Vector2.zero;
    }

}