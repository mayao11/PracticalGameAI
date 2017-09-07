using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RotateType
{
    直接旋转1,
    直接旋转2,
    插值,
    匀速,
}

public class RotateInput : MonoBehaviour
{

    Camera cam;
    GameObject cube;
    Vector3 target_dir;

    public RotateType rotate_type;

    // Use this for initialization
    void Start()
    {
        cam = this.GetComponent<Camera>();
        cube = GameObject.Find("Cube");
        target_dir = cube.transform.forward;
    }

    bool OnClickGround()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // We need to actually hit an object
        RaycastHit hitt = new RaycastHit();
        Physics.Raycast(ray, out hitt, 100);
        Debug.DrawLine(cam.transform.position, ray.direction, Color.red);
        if (hitt.transform != null && hitt.transform.name == "Ground")
        {
            Vector3 dir = hitt.point - cube.transform.position;
            dir.y = 0;
            target_dir = dir.normalized;
            return true;
        }
        return false;
    }

    ///////// 利用Quaternion的方法开始 ///////////////////////
    void SetRotation1()
    {
        Quaternion q = new Quaternion();
        q.SetLookRotation(target_dir);
        cube.transform.rotation = q;
    }

    ///////// 自己设定旋转的方法开始 ///////////////////////
    void SetRotation2()
    {
        Vector3 cube_forward = cube.transform.forward;

        float cos = Vector3.Dot(cube_forward.normalized, target_dir);
        cos = Mathf.Min(cos, 1);
        cos = Mathf.Max(cos, -1);

        float rot = Mathf.Acos(cos);

        if (Vector3.Cross(target_dir, cube_forward).y > 0)
        {
            Debug.Log("左转" + cube_forward + "  " + cos + "   " + rot + "    " + rot * Mathf.Rad2Deg);
            cube.transform.Rotate(-Vector3.up * rot * Mathf.Rad2Deg);
        }
        else
        {
            Debug.Log("右转" + cube_forward + "    " + cos + "    " + rot + "    " + rot * Mathf.Rad2Deg);
            cube.transform.Rotate(Vector3.up * rot * Mathf.Rad2Deg);
        }
    }

    // 旋转到目标方向
    void RotateTarget1()
    {
        Quaternion q = new Quaternion();
        q.SetLookRotation(target_dir);

        cube.transform.rotation = Quaternion.Lerp(cube.transform.rotation, q, 0.1f);
    }

    void RotateTarget2()
    {
        Vector3 cube_forward = cube.transform.forward;

        float cos = Vector3.Dot(cube_forward.normalized, target_dir);
        cos = Mathf.Max( Mathf.Min(cos, 1), -1 );

        float rot = Mathf.Acos(cos);
        if (rot > 0.1f)
        {
            rot = 0.1f;
        }

        if (Vector3.Cross(target_dir, cube_forward).y > 0)
        {
            cube.transform.Rotate(-Vector3.up * rot * Mathf.Rad2Deg);
        }
        else
        {
            cube.transform.Rotate(Vector3.up * rot * Mathf.Rad2Deg);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnClickGround();
        }

        Vector3 cube_forward = cube.transform.forward;
        if (Mathf.Abs(Vector3.Dot(cube_forward, target_dir)-1) > 0.001f)
        {
            switch (rotate_type)
            {
                case RotateType.直接旋转1:
                    SetRotation1();
                    break;
                case RotateType.直接旋转2:
                    SetRotation2();
                    break;
                case RotateType.插值:
                    RotateTarget1();
                    break;
                case RotateType.匀速:
                    RotateTarget2();
                    break;
            }
        }

    }


}
