using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class WithinSight : Conditional
{
    // How wide of an angle the object can see
    public float fieldOfViewAngle;
    // The tag of the targets
    public string targetTag;
    // Set the target variable when a target has been found so the subsequent tasks know which object is the target
    public SharedTransform target = new SharedTransform();
    public SharedVector3 targetPos = new SharedVector3();
    public SharedFloat invadeDegree;
    // A cache of all of the possible targets
    private Transform[] possibleTargets;

    CharacterData chaData;
    public override void OnAwake()
    {
        // Cache all of the transforms that have a tag of targetTag
        var targets = GameObject.FindGameObjectsWithTag(targetTag);
        possibleTargets = new Transform[targets.Length];
        for (int i = 0; i < targets.Length; ++i)
        {
            possibleTargets[i] = targets[i].transform;
        }
        chaData = gameObject.GetComponent<CharacterData>();
    }
    public override TaskStatus OnUpdate()
    {
        // Return success if a target is within sight
        for (int i = 0; i < possibleTargets.Length; ++i)
        {
            if (withinSight(possibleTargets[i], fieldOfViewAngle, chaData.viewDistance))
            {
                // Set the target so other tasks will know which transform is within sight
                target.Value = possibleTargets[i];
                targetPos.Value = target.Value.position;
                invadeDegree.Value = Vector3.Angle(transform.right, targetPos.Value - transform.position);
                Debug.Log("Invade Degree:" + invadeDegree.Value);
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Running;
    }
    // Returns true if targetTransform is within sight of current transform
    public bool withinSight(Transform targetTransform, float fieldOfViewAngle, float distance)
    {
        Vector3 direction = targetTransform.position - transform.position;
        if (direction.magnitude > distance)
        {
            return false;
        }
        // An object is within sight if the angle is less than field of view
        return Vector3.Angle(direction, transform.forward) < fieldOfViewAngle;
    }
}
