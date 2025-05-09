using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class OnTriggerHandler : MonoBehaviour
{
    [Obsolete] [SerializeField] private GameObject m_MessageListener;
    [Obsolete] [SerializeField] private string m_OnEnterMessage;
    [Obsolete] [SerializeField] private string m_OnExitMessage;
    [SerializeField] private string[] m_CollisionTags = { "Player" };
    [SerializeField] private UnityEvent<Object> m_OnTriggerEnterAction;
    [SerializeField] private UnityEvent<Object> m_OnTriggerExitAction;

    private void OnTriggerEnter(Collider other)
    {
        if (m_CollisionTags.Contains(other.tag))
        {
            if (!m_MessageListener)
                m_MessageListener = gameObject;
            if (!string.IsNullOrEmpty(m_OnEnterMessage))
                m_MessageListener?.SendMessage(m_OnEnterMessage, other, SendMessageOptions.DontRequireReceiver);
            m_OnTriggerEnterAction?.Invoke(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_CollisionTags.Contains(other.tag))
        {
            if (!m_MessageListener)
                m_MessageListener = gameObject;
            if (!string.IsNullOrEmpty(m_OnExitMessage))
                m_MessageListener?.SendMessage(m_OnExitMessage, other, SendMessageOptions.DontRequireReceiver);
            m_OnTriggerExitAction?.Invoke(other);
        }
    }
}