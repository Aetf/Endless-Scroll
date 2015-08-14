using System;
using UnityEngine;

public class VerticalFollow2D : MonoBehaviour {
    public enum ChangeStrategy {
        IgnoreChange,
        ImmediateChange,
        None
    }

    public Transform target;
    public float damping = 1;
    public float lookAheadFactor = 3;
    public float lookAheadReturnSpeed = 0.5f;
    public float lookAheadMoveThreshold = 0.1f;
    public ChangeStrategy xChangeStrategy = ChangeStrategy.ImmediateChange;
    public bool keepVerticalOffset = true;
    public bool forceTopLevelObject = false;

    private Vector3 m_Offset;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    // Use this for initialization
    private void Start() {
        if (keepVerticalOffset) {
            m_Offset = transform.position - target.position;
            m_Offset.x = 0f;
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
            + (keepVerticalOffset ? m_Offset : Vector3.forward * m_Offset.z);

        // only update lookahead pos if accelerating or changed direction
        float yMoveDelta = (targetPos - m_LastTargetPosition).y;

        bool updateLookAheadTarget = Mathf.Abs(yMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget) {
            m_LookAheadPos = lookAheadFactor * Vector3.up * Mathf.Sign(yMoveDelta);
        } else {
            m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = targetPos + m_LookAheadPos;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        switch (xChangeStrategy) {
        case ChangeStrategy.IgnoreChange:
            newPos.x = transform.position.x;
            break;
        case ChangeStrategy.ImmediateChange:
            newPos.x = targetPos.x;
            break;
        }
        transform.position = newPos;

        m_LastTargetPosition = targetPos;
    }
}
