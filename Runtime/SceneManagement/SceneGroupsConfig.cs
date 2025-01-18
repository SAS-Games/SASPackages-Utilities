using System;
using UnityEngine;

namespace SAS.SceneManagement
{
    [Serializable, CreateAssetMenu(menuName = "SAS/SceneGroupsConfig")]

    public class SceneGroupsConfig : ScriptableObject
    {
        [SerializeField] private SceneGroup[] m_SceneGroups;

        public SceneGroup[] SceneGroups { get => m_SceneGroups; }
    }
}
