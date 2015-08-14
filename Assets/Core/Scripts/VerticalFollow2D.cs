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
    public bool forceTopLevelObject = false;

    private float m_OffsetZ;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    // Use this for initialization
    private void Start() {
        m_LastTargetPosition = target.position;
        m_OffsetZ = (transform.position - target.position).z;
        if (forceTopLevelObject)
            transform.parent = null;
    }


    // Update is called once per frame
    private void Update() {
        // only update lookahead pos if accelerating or changed direction
        float yMoveDelta = (target.position - m_LastTargetPosition).y;

        bool updateLookAheadTarget = Mathf.Abs(yMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget) {
            m_LookAheadPos = lookAheadFactor * Vector3.up * Mathf.Sign(yMoveDelta);
        } else {
            m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        switch (xChangeStrategy) {
        case ChangeStrategy.IgnoreChange:
            newPos.x = transform.position.x;
            break;
        case ChangeStrategy.ImmediateChange:
            newPos.x = target.position.x;
            break;
        }
        transform.position = newPos;

        m_LastTargetPosition = target.position;
    }
}
