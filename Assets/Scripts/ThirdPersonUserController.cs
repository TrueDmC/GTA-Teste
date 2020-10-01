using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandartAssets.Character.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserController : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character;
        private Transform m_cam;
        private Vector3 m_CamForward;
        private Vector3 m_move;
        private bool m_jump;

        // Start is called before the first frame update
        void Start()
        {
            if (Camera.main != null)
            {
                m_cam = Camera.main.transform;
            }
            m_Character = GetComponent<ThirdPersonCharacter>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_jump)
            {
                m_jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }

        private void FixedUpdate()
        {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            if (m_cam != null)
            {
                m_CamForward = Vector3.Scale(m_cam.forward, new Vector3(1, 0, 1)).normalized;
                m_move = v * m_CamForward + h * m_cam.right;
            }
            else
            {
                m_move = v * Vector3.forward + h * Vector3.right;

            }
#if !MOBILE_INPUT
            if (Input.GetKey(KeyCode.LeftShift)) m_move *= 1.5f;
            m_Character.Move(m_move, crouch, m_jump);
            m_jump = false;
#endif
        }
    }
}
