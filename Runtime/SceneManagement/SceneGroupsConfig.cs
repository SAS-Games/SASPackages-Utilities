using System;
using System.Linq;
using UnityEngine;

namespace SAS.SceneManagement
{
    [Serializable, CreateAssetMenu(menuName = "SAS/SceneGroupsConfig")]
    public class SceneGroupsConfig : ScriptableObject
    {
        [SerializeField] private SceneGroup[] m_SceneGroups;

        public SceneGroup[] SceneGroups
        {
            get => m_SceneGroups;
        }

        public SceneGroup GetSceneGroup(string sceneGroupName)
        {
            return m_SceneGroups.FirstOrDefault(sceneGroup => sceneGroup.Name == sceneGroupName);
        }
    }
}