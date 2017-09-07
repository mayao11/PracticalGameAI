using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRobotController : MonoBehaviour
{
    Dictionary<GameObject, int> boxes = new Dictionary<GameObject, int>();
    int id_counter = 1;

    // 当前正在搬运的货物
    GameObject working_box;
    // going_back表示了机器人的两种状态：
    // true代表当前箱子已处理完毕，可以去取下一个箱子
    // false代表正在推当前的箱子
    bool going_back = true;

    // 保存货物到boxes容器中，会给货物分配ID
    void SaveNewBox(GameObject box)
    {
        if (box.transform.position.y > 0.251f)
        {
            return;
        }
        if (boxes.ContainsKey(box))
        {
            return;
        }
        boxes[box] = id_counter;
        id_counter++;
    }

    // 整理货物，即搬运货物到目标位置
    bool CleanBox(GameObject box)
    {
        Vector3 clean_pos = BoxCleanPos(boxes[box]);
        if (Vector3.Distance(box.transform.position, clean_pos) > 0.05f)
        {
            //MoveTowards(current: Vector3, target: Vector3, maxDistanceDelta: float) : Vector3
            Vector3 to = clean_pos - box.transform.position;
            box.transform.position += to.normalized * Mathf.Min(0.1f, Vector3.Distance(box.transform.position, clean_pos));

            transform.position = box.transform.position + to.normalized * -0.5f;
            return false;
        }

        return true;
    }

    // 计算货物对应的位置
    Vector3 BoxCleanPos(int id)
    {
        int n = (id - 1) % 5;
        int row = (id - 1) / 5;
        Vector3 v = new Vector3(-5f + n * 1.0f, 0.25f, -5f + row * 1.0f);
        return v;
    }


    // Update is called once per frame
    void Update()
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject box in all)
        {
            // 保存货物到boxes中，这里会给货物分配ID
            SaveNewBox(box);
        }

        // 如果当前没有正在搬运的货物，则从boxes中查找需要搬运的货物
        if (working_box == null)
        {
            foreach (var pair in boxes)
            {
                Vector3 clean_pos = BoxCleanPos(boxes[pair.Key]);
                if (Vector3.Distance(pair.Key.transform.position, clean_pos) > 0.05f)
                {
                    // 找到一个需要搬运的货物，设置为当前正在搬的
                    working_box = pair.Key;
                    break;
                }
            }
        }

        // 如果当前正在搬运货物
        if (working_box != null)
        {
            // 情况一：正在搬运的状态
            if (going_back == false)
            {
                if (CleanBox(working_box))
                {
                    working_box = null;
                    going_back = true;

                }
            }
            else
            {
            // 情况二：正在跑向货物的状态
                if (Vector3.Distance(working_box.transform.position, transform.position) > 0.05f)
                {
                    //MoveTowards(current: Vector3, target: Vector3, maxDistanceDelta: float) : Vector3
                    Vector3 to = working_box.transform.position - transform.position;
                    float f = to.magnitude / 0.1f;
                    to /= f;
                    transform.position += to;
                }
                else
                {
                    going_back = false;
                }
            }
        }
    }
}
