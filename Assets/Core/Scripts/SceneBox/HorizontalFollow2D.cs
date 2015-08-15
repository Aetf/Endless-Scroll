using System;
using UnityEngine;
using UnlimitedCodeWorks;

public class HorizontalFollow2D : MonoBehaviour {
    public enum ChangeStrategy {
        IgnoreChange,
        ImmediateChange,
        None
    }

    public Transform target;
    public bool useMonoTarget = true;
    public float damping = 1;
    public float lookAheadFactor = 3;
    public float lookAheadReturnSpeed = 0.5f;
    public float lookAheadMoveThreshold = 0.1f;
    public ChangeStrategy yChangeStrategy = ChangeStrategy.ImmediateChange;
    public bool keepHorizontalOffset = true;
    public bool forceTopLevelObject = false;

    private Vector3 m_Offset;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    // Use this for initialization
    private void Start() {
        if (useMonoTarget) {
            target = MonoTargetHub.Player;
        }
        if (keepHorizontalOffset) {
            m_Offset = transform.position - target.position;
            m_Offset.y = 0f;
        } else {
            m_Offset = Vector3.zero;
        }
        m_LastTargetPosition = target.position + m_Offset;
        if (forceTopLevelObject)
            transform.parent = null;
    }


    // Update is called once per frame
    private void Update() {
        Vector3 targetPos = target.position
            + (keepHorizontalOffset ? m_Offset : Vector3.forward * m_Offset.z);

        // only update lookahead pos if accelerating or changed direction
        float xMoveDelta = (targetPos - m_LastTargetPosition).x;

        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget) {
            m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
        } else {
            m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = targetPos + m_LookAheadPos;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        switch (yChangeStrategy) {
        case ChangeStrategy.IgnoreChange:
            newPos.y = transform.position.y;
            break;
        case ChangeStrategy.ImmediateChange:
            newPos.y = targetPos.y;
            break;
        }
        transform.position = newPos;

        m_LastTargetPosition = targetPos;
    }
}
