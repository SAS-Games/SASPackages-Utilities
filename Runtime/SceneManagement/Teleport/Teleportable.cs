using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAS.Teleport
{
    public interface ITeleportable
    {
        void TeleportTo(Transform destination);
        bool CanTeleport { get; set; }
    }

    public class Teleportable : MonoBehaviour, ITeleportable
    {
        [SerializeField] private bool m_MoveGameObjectToScene = false;
        public bool CanTeleport { get; set; } = true;

        void ITeleportable.TeleportTo(Transform destination)
        {
            transform.position = destination.position;
            if (m_MoveGameObjectToScene)
                SceneManager.MoveGameObjectToScene(gameObject, destination.gameObject.scene);
        }
    }
}
