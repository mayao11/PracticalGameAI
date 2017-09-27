using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float viewRadius = 8.0f;
    public float viewAngleStep = 40;

    public float moveSpeed = 1.0f;          // 移动速度
    public float turnSpeed = 3.0f;          // 转身速度
    public float maxChaseDist = 11.0f;      // 最大追击距离
    public float maxLeaveDist = 2.0f;      // 最大离开原位距离

    Vector3 basePosition;       // 原始位置
    Quaternion baseDirection;   // 原始方向

    /// <summary>
    /// ---------------战斗部分开始-------------------
    /// </summary>

    public GameObject bullet;
    public float bulletSpeed = 30.0f;
    public float fireInterval = 0.2f;
    public float fireCd = 0;

    int hp = 10;

    void Fire()
    {
        if (fireCd > Time.time)
        {
            return;
        }
        var b = Instantiate(bullet, transform.position, Quaternion.identity, transform);
        var rigid = b.GetComponent<Rigidbody>();
        rigid.velocity = transform.forward * bulletSpeed;
        fireCd = Time.time + fireInterval;
    }

    /// <summary>
    /// ===============战斗部分结束===================
    /// </summary>

    private void OnTriggerEnter(Collider other)
    {
        GameObject bullet = other.transform.gameObject;
        if (bullet.tag != "PlayerBullet")
        {
            return;
        }
        Destroy(bullet);
        if (state == State.Idle)
        {
            state = State.Attack;
            invader = bullet.transform.parent.gameObject;
        }
        if (hp > 0)
        {
            hp -= 1;
            if (hp == 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        transform.Rotate(transform.right, 90);
        state = State.Dead;
    }

    public enum State
    {
        Idle,   // 待命状态
        Attack, // 进攻敌方
        Back,   // 回归原位
        Dead,   // 死亡
    }
    public State state = State.Idle;    // AI当前状态
    GameObject invader = null;      // 入侵者GameObject

    void Start () {
        basePosition = transform.position;
        baseDirection = transform.rotation;
	}

    // 是否正在面对入侵者，即已经正确瞄准
    bool IsFacingInvader()
    {
        if (invader == null)
        {
            return false;
        }
        Vector3 v1 = invader.transform.position - transform.position;
        v1.y = 0;
        if (Vector3.Angle(transform.forward, v1) < 1)
        {
            return true;
        }
        return false;
    }

    // 转向入侵者方向，每次只转一点，速度受turnSpeed控制
    void RotateToInvader()
    {
        if (invader == null)
        {
            return;
        }
        Vector3 v1 = invader.transform.position - transform.position;
        v1.y = 0;
        Vector3 cross = Vector3.Cross(transform.forward, v1);
        float angle = Vector3.Angle(transform.forward, v1);
        transform.Rotate(cross, Mathf.Min(turnSpeed, Mathf.Abs(angle)));
    }

    // 转向参数指定的方向，每次只转一点，速度受turnSpeed控制
    void RotateToDirection(Quaternion rot)
    {
        Quaternion.RotateTowards(transform.rotation, rot, turnSpeed);
    }

    // 是否正位于某个点
    bool IsInPosition(Vector3 pos)
    {
        Vector3 v = pos - transform.position;
        v.y = 0;
        return v.magnitude < 0.05f;
    }

    // 移动到某个点，每次只移动一点
    void MoveToPosition(Vector3 pos)
    {
        Vector3 v = pos - transform.position;
        v.y = 0;
        transform.position += v.normalized * moveSpeed * Time.deltaTime;
    }

	void Update() {
        if (state == State.Dead)
        {
            return;
        }

        if (state == State.Idle)
        {
            // 方向不对的话，转一下
            transform.rotation = Quaternion.RotateTowards(transform.rotation, baseDirection, turnSpeed);
        }
        else if (state == State.Attack)
        {
            if (invader != null)
            {
                if (Vector3.Distance(invader.transform.position, transform.position) > maxChaseDist)
                {
                    // 与敌人距离过大，追丢的情况
                    state = State.Back;
                    return;
                }
                if (Vector3.Distance(basePosition, transform.position) > maxLeaveDist)
                {
                    // 离开原始位置过远的情况
                    state = State.Back;
                    return;
                }
                if (Vector3.Distance(invader.transform.position, transform.position) > maxChaseDist/2)
                {
                    // 追击敌人
                    MoveToPosition(invader.transform.position);
                }
                // 转向敌人
                if (!IsFacingInvader())
                {
                    RotateToInvader();
                }
                else
                {
                    Fire();
                }
            }
        }
        else if (state == State.Back)
        {
            if (IsInPosition(basePosition))
            {
                state = State.Idle;
                return;
            }
            MoveToPosition(basePosition);
        }
        DrawFieldOfView();
	}

    void DrawFieldOfView()
    {
        // 获得最左边那条射线的向量，相对正前方，角度是-45
        Vector3 forward_left = Quaternion.Euler(0, -45, 0) * transform.forward * viewRadius;
        // 依次处理每一条射线
        for (int i = 0; i <= viewAngleStep; i++)
        {
            // 每条射线都在forward_left的基础上偏转一点，最后一个正好偏转90度到视线最右侧
            Vector3 v = Quaternion.Euler(0, (90.0f / viewAngleStep) * i, 0) * forward_left; ;

            // 创建射线
            Ray ray = new Ray(transform.position, v);
            RaycastHit hitt = new RaycastHit();
            // 射线只与两种层碰撞，注意名字和你添加的layer一致，其他层忽略
            int mask = LayerMask.GetMask("Obstacle", "Player");
            Physics.Raycast(ray, out hitt, viewRadius, mask);

            // Player位置加v，就是射线终点pos
            Vector3 pos = transform.position + v;
            if (hitt.transform != null)
            {
                // 如果碰撞到什么东西，射线终点就变为碰撞的点了
                pos = hitt.point;
            }
            // 从玩家位置到pos画线段，只会在编辑器里看到
            Debug.DrawLine(transform.position, pos, Color.red); ;

            // 如果真的碰撞到敌人，进一步处理
            if (hitt.transform!=null && hitt.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                OnEnemySpotted(hitt.transform.gameObject);
            }
        }
    }

    void OnEnemySpotted(GameObject enemy)
    {
        invader = enemy;
        state = State.Attack;
    }

}

