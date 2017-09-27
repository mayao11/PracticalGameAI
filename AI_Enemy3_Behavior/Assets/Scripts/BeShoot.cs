using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BeShoot : Conditional
{
    public SharedTransform target;
    public SharedVector3 targetPos = new SharedVector3();
    public SharedBool beSnipped;
    CharacterData chaData;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }

    public override TaskStatus OnUpdate()
    {
        if (chaData.beShotShooter == null)
        {
            return TaskStatus.Running;
        }

        target.Value = chaData.beShotShooter.transform;
        targetPos.Value = target.Value.position;
        beSnipped.Value = true;

        return TaskStatus.Success;
    }
}