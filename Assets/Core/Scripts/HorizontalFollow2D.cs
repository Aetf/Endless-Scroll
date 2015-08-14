using System;
using UnityEngine;

public class HorizontalFollow2D : MonoBehaviour {
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
    public ChangeStrategy yChangeStrategy = ChangeStrategy.ImmediateChange;
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
        float xMoveDelta = (target.position - m_LastTargetPosition).x;

        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget) {
            m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
        } else {
            m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        switch (yChangeStrategy) {
        case ChangeStrategy.IgnoreChange:
            newPos.y = transform.position.y;
            break;
        case ChangeStrategy.ImmediateChange:
            newPos.y = target.position.y;
            break;
        }
        transform.position = newPos;

        m_LastTargetPosition = target.position;
    }
}
