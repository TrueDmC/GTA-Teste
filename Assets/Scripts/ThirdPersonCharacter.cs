using UnityEngine;


namespace UnityStandartAssets.Character.ThirdPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]


    public class ThirdPersonCharacter : MonoBehaviour
    {

        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StaitonaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f;
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMutiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        bool m_IsGrounded;
        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_FordAmount;
        Vector3 m_GroundNormal;
        float m_CapsuleHeight;
        Vector3 m_CapsuleCenter;
        CapsuleCollider m_Capsule;
        bool m_Crouching;
        public bool isStrafe;
        public bool isWeapon;


        // Start is called before the first frame update
        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            m_CapsuleHeight = m_Capsule.height;
            m_CapsuleCenter = m_Capsule.center;

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;

        }

        public void Move(Vector3 move, bool crouch, bool jump)
        {
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);

            if (!isWeapon)
                m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_FordAmount = move.z;

            ApplyExtraTurnRotation();

            if (m_IsGrounded)
            {
                HandleGroundedMovement(crouch, jump);
            }
            else
            {
                HandleAirBorneMovement();
            }

            ScaleCapsuleForCrouching(crouch);

            PreventStandingLowHeadroom();

            UpdateAnimator(move);

        }

        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (m_IsGrounded && crouch)
            {
                if (m_Crouching) return;
                m_Capsule.height = m_Capsule.height / 2;
                m_Capsule.center = m_Capsule.center / 2;
                m_Crouching = true;
            }

            else
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLenght = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLenght, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;
                    return;
                }

                m_Capsule.height = m_CapsuleHeight;
                m_Capsule.center = m_CapsuleCenter;
                m_Crouching = false;
            }
        }

        void PreventStandingLowHeadroom()
        {
            if (!m_Crouching)
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLenght = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLenght, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_Crouching = true;

                }
            }
        }

        void UpdateAnimator(Vector3 move)
        {
            if (!isStrafe)
            {
                m_Animator.SetFloat("Forward", m_FordAmount, 0.1f, Time.deltaTime);
                m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            }

            else
            {
                //... Create Armed Player
            }

            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);

            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);

            }
            else
            {
                m_Animator.SetFloat("Jump", 0);
            }

            float runCycle = Mathf.Repeat(m_Animator.GetNextAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float JumpLeg = (runCycle < k_Half ? 1 : 1) * m_FordAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", JumpLeg);
            }

            if (m_IsGrounded && move.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMutiplier;
            }

            else
            {
                m_Animator.speed = 1;
            }
        }

        void HandleAirBorneMovement()
        {
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }

        void HandleGroundedMovement(bool crouch, bool jump)
        {
            // if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
           if (jump && !crouch)
            {
                //Jump
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            if (isWeapon)
            {
                float turnSpeed = Mathf.Lerp(m_StaitonaryTurnSpeed, m_MovingTurnSpeed, m_FordAmount);
                transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
            }
        }

        public void OnAnimatorMove()
        {
            if (m_IsGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }

        void CheckGroundStatus()
        {
            RaycastHit hitinfo;

#if UNITY_EDITOR
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_OrigGroundCheckDistance));
#endif
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitinfo, m_GroundCheckDistance))
            {
                m_GroundNormal = hitinfo.normal;
                m_IsGrounded = true;
                m_Animator.applyRootMotion = true;
            }
            else
            {
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
            }
        }

    }

}
