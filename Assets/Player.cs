using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class Player: MonoBehaviour
{
    [Header("Move info")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Dash info")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashDir {get; private set; }
    [SerializeField]public float dashTimer;
    public float dashCooldown = 1f;

    [Header("Collision info")]
    [SerializeField]private Transform groundCheck;
    [SerializeField]private float groundCheckDistance;
    [SerializeField]private Transform wallCheck;
    [SerializeField]private float wallCheckDistance;
    [SerializeField]private LayerMask whatIsGround;
    [SerializeField]private LayerMask whatIsWall;

    public int facingDir {get; private set;} = 1;
    private bool facingRight = true;



    #region Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }

    #endregion


    #region States
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get;private set; }
    public PlayerDashState dashState { get; private set; }


    #endregion

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this,stateMachine,"Idle");
        moveState = new PlayerMoveState(this,stateMachine,"Move");
        jumpState = new PlayerJumpState(this,stateMachine,"Jump");
        airState = new PlayerAirState(this,stateMachine, "Jump");
        dashState = new PlayerDashState(this,stateMachine,"Dash");
    }

    public void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine.Initialize(idleState);// PlayerStateMachine.Initialize()
    }

    public void Update()
    {
        stateMachine.currentState.Update();// PlayerState.Update()
        checkForDashInput();
    }

//在任意状态下都能冲刺
    private void checkForDashInput(){
        dashTimer -= Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.LeftShift) && dashTimer <= 0){
            dashTimer = dashCooldown;
            dashDir = Input.GetAxisRaw("Horizontal");
            if(dashDir == 0){
                dashDir = facingDir;
            }
            stateMachine.ChangeState(dashState);
        }
    }

//设置速度
    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        rb.velocity = new Vector2(_xVelocity,_yVelocity);
        FlipController(_xVelocity);
    }

//地面检测
//Physics2D.RayCast(起始点，方向，距离，检测层)
    public bool IsGroundedDetected() => Physics2D.Raycast(groundCheck.position,Vector2.down,groundCheckDistance,whatIsGround);

//画碰撞检测射线
    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(起始点，终点（x,y-距离）)
        //右上+，左下-
        Gizmos.DrawLine(groundCheck.position,new Vector3(groundCheck.position.x,groundCheck.position.y-groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position,new Vector3(wallCheck.position.x+wallCheckDistance,wallCheck.position.y));
    }

    private void Flip()
    {
        facingRight = !facingRight;
        facingDir *= -1;
        transform.Rotate(0,180,0);//y轴180度翻转
    }

    private void FlipController(float _x)
    {
        if(_x > 0 && !facingRight)
        {
            Flip();
        }
        else if(_x < 0 && facingRight)
        {
            Flip();
        }
    }

}
