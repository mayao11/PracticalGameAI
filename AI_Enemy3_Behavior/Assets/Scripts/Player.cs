using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float moveSpeed = 6;

    Rigidbody myRigidbody;

    int hp = 100;

    public GameObject bullet;       // 子弹Prefab，用它来生成更多子弹
    public float bulletSpeed = 30.0f;       // 子弹速度
    public float fireInterval = 0.3f;       // 射击最小间隔
    float fireCd = 0;                  // 记录CD时间，用来控制子弹射击频率

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Fire()
    {
        if (fireCd > Time.time)
        {
            return;
        }
        var b = Instantiate(bullet, transform.position, Quaternion.identity, null);
        var rigid = b.GetComponent<Rigidbody>();
        rigid.velocity = transform.forward * bulletSpeed;
        fireCd = Time.time + fireInterval;

        var b_script = b.GetComponent<Bullet>();
        b_script.shooter = gameObject;
    }

    void Update()
    {
        if (hp > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitt = new RaycastHit();
            Physics.Raycast(ray, out hitt, 100, LayerMask.GetMask("Ground"));
            if (hitt.transform != null)
            {
                transform.LookAt(new Vector3(hitt.point.x, transform.position.y, hitt.point.z));
            }
            myRigidbody.velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

            if (Input.GetMouseButtonDown(0))
            {
                Fire();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject bullet = other.transform.gameObject;
        if (bullet.tag != "EnemyBullet")
        {
            return;
        }
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
        transform.Rotate(transform.right, 90);
        myRigidbody.velocity = Vector3.zero;
    }

}


