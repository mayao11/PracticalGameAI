using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public GameObject box_prefeb;

    // Use this for initialization
    void Start() {
    }

    void OnClickGround()
    {
        Camera cam = Camera.main;       // 主摄像机，这样获取很方便

        // 老规矩，从鼠标点击的地方，向屏幕内打射线
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 处理这条射线打到的那个GameObject
        RaycastHit hitt = new RaycastHit();
        Physics.Raycast(ray, out hitt, 100);
        Debug.DrawLine(cam.transform.position, ray.direction, Color.red);

        // 如果打到地面，就生成box（也就是货物）
        if (hitt.transform!=null && hitt.transform.name=="Ground")
        {
            Vector3 p = new Vector3(hitt.point.x, 5, hitt.point.z);
            Instantiate(box_prefeb, p, Quaternion.Euler(0, 0, 0));
        }
    }

    // Update is called once per frame
    void Update() {
        // 每帧检测鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            OnClickGround();
        }
    }


}
