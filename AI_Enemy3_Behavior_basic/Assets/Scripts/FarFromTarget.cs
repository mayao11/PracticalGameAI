using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class FarFromTarget : Conditional
{
    public SharedTransform target;
    CharacterData chaData;

    public MovePositionType type;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }

    public override TaskStatus OnUpdate()
    {
        if (type == MovePositionType.Target)
        {
            if (target.Value == null)
            {
                return TaskStatus.Success;
            }
            if (Vector3.Distance(target.Value.position, transform.position) > chaData.maxChaseDist)
            {
                return TaskStatus.Success;
            }
        }
        else if (type == MovePositionType.Home)
        {
            if (Vector3.Distance(chaData.basePosition, transform.position) > chaData.maxLeaveDist)
            {
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Running;
    }
}