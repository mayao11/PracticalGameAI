using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class WithinSight : Conditional
{
    // 视野角度
    public float fieldOfViewAngle;
    // 目标物体的Tag
    public string targetTag;
    // 发现目标时，将目标对象设置到BahaviorTree共享变量里面去
    public SharedTransform target;
    public SharedVector3 targetPos;
    // 所有指定Tag的物体的数组
    private Transform[] possibleTargets;

    // 重载函数，Behavior Designer专用的Awake
    public override void OnAwake()
    {
        // 根据Tag查找到所有物体，全部加入数组
        var targets = GameObject.FindGameObjectsWithTag(targetTag);
        possibleTargets = new Transform[targets.Length];
        for (int i = 0; i < targets.Length; ++i)
        {
            possibleTargets[i] = targets[i].transform;
        }
    }
    // 重载函数，Behavior Designer专用的Update
    public override TaskStatus OnUpdate()
    {
        // 判断目标是否在视野内，这个返回值TaskStatus很关键，会影响树的执行流程
        for (int i = 0; i < possibleTargets.Length; ++i)
        {
            if (withinSight(possibleTargets[i], fieldOfViewAngle, 10))
            {
                // 将目标信息填写到共享变量里面，这样其它Action就可以访问它们了
                target.Value = possibleTargets[i];
                targetPos.Value = target.Value.position;
                Debug.Log("Find Target" + targetPos.Value);
                // 成功则返回 TaskStatus.Success
                return TaskStatus.Success;
            }
        }
        // 没找到目标就在下一帧继续执行此任务
        return TaskStatus.Running;
    }

    // 判断物体是否在视野范围内的方法
    public bool withinSight(Transform targetTransform, float fieldOfViewAngle, float distance)
    {
        Vector3 direction = targetTransform.position - transform.position;
        if (direction.magnitude > distance)
        {
            return false;
        }
        return Vector3.Angle(direction, transform.forward) < fieldOfViewAngle;
    }
}
