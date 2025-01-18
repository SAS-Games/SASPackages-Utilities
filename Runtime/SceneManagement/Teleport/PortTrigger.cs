using UnityEngine;

namespace SAS.Teleport.teleport
{
    public class PortTrigger : MonoBehaviour
    {
        [SerializeField] private Teleporter m_Teleporter;
        private void OnTriggerEnter(Collider collider)
        {
            var teleportable = collider.GetComponent<ITeleportable>();
            if (teleportable != null)
                m_Teleporter?.OnEnter(teleportable);
        }
    }
}