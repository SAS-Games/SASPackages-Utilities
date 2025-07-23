using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerHandler : MonoBehaviour
{
    [SerializeField] private string[] m_CollisionTags = { "Player" };
    [SerializeField] private UnityEvent<GameObject> m_OnTriggerEnterAction;
    [SerializeField] private UnityEvent<GameObject> m_OnTriggerExitAction;

    private void OnTriggerEnter(Collider other)
    {
        if (m_CollisionTags.Contains(other.tag))
        {
            m_OnTriggerEnterAction?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_CollisionTags.Contains(other.tag))
        {
            m_OnTriggerExitAction?.Invoke(other.gameObject);
        }
    }
}