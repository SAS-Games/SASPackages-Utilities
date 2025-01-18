using System.Linq;
using UnityEngine;

public class OnTriggerHandler : MonoBehaviour
{
    [SerializeField] private GameObject m_MessageListener;
    [SerializeField] private string m_OnEnterMessage;
    [SerializeField] private string m_OnExitMessage;
    [SerializeField] private string[] m_CollisionTags = { "Player" };

    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(m_OnEnterMessage))
            return;

        if (m_CollisionTags.Contains(other.tag))
        {
            if (!m_MessageListener)
                m_MessageListener = gameObject;
            m_MessageListener?.SendMessage(m_OnEnterMessage, other, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(m_OnExitMessage))
            return;

        if (m_CollisionTags.Contains(other.tag))
        {
            if (!m_MessageListener)
                m_MessageListener = gameObject;
            m_MessageListener?.SendMessage(m_OnExitMessage, other, SendMessageOptions.DontRequireReceiver);
        }
    }
}
