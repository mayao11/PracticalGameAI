using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour {

    public float speed = 3.0f;
    public float turnSpeed = 3.0f;          // 转身速度
    public float maxChaseDist = 11.0f;      // 最大追击距离
    public float maxLeaveDist = 2.0f;      // 最大离开原位距离

    public float viewDistance = 5.0f;

    public GameObject bullet;
    public float bulletSpeed = 30.0f;
    public float fireInterval = 0.2f;
    public float fireCd = 0;

    public Vector3 basePosition;
    public Vector3 baseForward;

    public GameObject beShotShooter = null;

    int hp = 10;

    public void Awake()
    {
        basePosition = transform.position;
        baseForward = transform.forward;
    }

    public void Fire()
    {
        if (fireCd > Time.time)
        {
            return;
        }
        var b = Instantiate(bullet, gameObject.transform.position, Quaternion.identity, transform);
        var rigid = b.GetComponent<Rigidbody>();
        rigid.velocity = transform.forward * bulletSpeed;
        fireCd = Time.time + fireInterval;

        var b_script = b.GetComponent<Bullet>();
        b_script.shooter = gameObject;
    }

    public void RotateTo(Vector3 pos)
    {
        Vector3 v1 = pos - transform.position;
        v1.y = 0;
        float angle = Vector3.Angle(transform.forward, v1);
        if (angle < 0.1)
        {
            return;
        }
        Vector3 cross = Vector3.Cross(transform.forward, v1);
        transform.Rotate(cross, Mathf.Min(turnSpeed, Mathf.Abs(angle)));
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject bullet = other.transform.gameObject;
        if (bullet.tag != "PlayerBullet")
        {
            return;
        }
        beShotShooter = bullet.GetComponent<Bullet>().shooter;
        Destroy(bullet);
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
        var com = gameObject.GetComponent("BehaviorTree");
        Destroy(com);
        // 如果不删除寻路组件，就不能旋转
        var com2 = gameObject.GetComponent("NavMeshAgent");
        Destroy(com2);
        transform.Rotate(transform.right, 90);
    }
}
