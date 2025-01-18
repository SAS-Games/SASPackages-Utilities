using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAS.SceneManagement
{
    [Serializable]
    public class SceneGroup
    {
        [SerializeField] private string m_Name = "New Scene Group";
        public string Name => m_Name;
        public List<SceneData> Scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Reference.Name;
        }
    }

    [Serializable]
    public class SceneData
    {
        public SceneReference Reference;
        public string Name => Reference.Name;
        public SceneType SceneType;
        public bool IsOptinal;
    }

    public enum SceneType { ActiveScene = 1, MainMenu, UserInterface, HUD, Cinematic, Environment, Tooling }
}