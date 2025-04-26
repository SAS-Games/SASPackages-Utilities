using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class SceneUtility
{
    /// <summary>
    /// Finds a single component of a specified type in a given scene.
    /// </summary>
    /// <typeparam name="T">The type of component to search for.</typeparam>
    /// <param name="sceneName">The name of the scene to search in.</param>
    /// <param name="objectName">Optional: The name of the object to match.</param>
    /// <returns>The found component of type T, or null if not found.</returns>
    public static T FindComponentInScene<T>(string sceneName, string objectName = null) where T : Component
    {
        return FindComponentInScene<T>(SceneManager.GetSceneByName(sceneName), objectName);
    }

    public static T FindComponentInScene<T>(Scene targetScene, string objectName = null) where T : Component
    {
        // Ensure the scene is valid and loaded
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogWarning($"Scene {targetScene.name} is not loaded or is invalid.");
            return null;
        }

        // Get all root objects in the scene
        GameObject[] rootObjects = targetScene.GetRootGameObjects();

        // Search through all root objects
        foreach (GameObject rootObject in rootObjects)
        {
            T[] components = rootObject.GetComponentsInChildren<T>(true);

            // If no components are found, continue to the next root object
            if (components.Length == 0) continue;

            // If objectName is provided, find the first matching component
            if (!string.IsNullOrEmpty(objectName))
            {
                foreach (T component in components)
                {
                    if (component.gameObject.name == objectName)
                    {
                        Debug.Log(
                            $"Found {typeof(T).Name} in scene {targetScene.name} on object {component.gameObject.name}.");
                        return component;
                    }
                }

                // If no matching object name is found, log a warning and continue to next root object
                Debug.LogWarning(
                    $"No component of type {typeof(T).Name} with object name '{objectName}' found in scene {targetScene.name}.");
                continue;
            }

            // If objectName is not provided, return the first component
            Debug.Log(
                $"Found first {typeof(T).Name} in scene {targetScene.name} on object {components[0].gameObject.name}.");
            return components[0];
        }

        // If no component is found at all, return null
        Debug.LogWarning($"No component of type {typeof(T).Name} found in scene {targetScene.name}.");
        return null;
    }


    /// <summary>
    /// Finds all components of a specified type in a given scene.
    /// </summary>
    /// <typeparam name="T">The type of component to search for.</typeparam>
    /// <param name="sceneName">The name of the scene to search in.</param>
    /// <returns>A list of components of type T found in the scene.</returns>
    public static List<T> FindComponentsInScene<T>(string sceneName)
    {
        List<T> components = new List<T>();

        // Get the target scene
        Scene targetScene = SceneManager.GetSceneByName(sceneName);

        // Ensure the scene is valid and loaded
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogWarning($"Scene {sceneName} is not loaded or is invalid.");
            return components;
        }

        // Get all root objects in the scene
        GameObject[] rootObjects = targetScene.GetRootGameObjects();

        // Search through all root objects
        foreach (GameObject rootObject in rootObjects)
        {
            // Add all components of type T found in the hierarchy
            T[] foundComponents = rootObject.GetComponentsInChildren<T>(true);
            if (foundComponents.Length > 0)
            {
                components.AddRange(foundComponents);
            }
        }

        Debug.Log($"Found {components.Count} components of type {typeof(T).Name} in scene {sceneName}.");
        return components;
    }

    public static void SetActiveScene(Scene scene)
    {
        if (scene.IsValid())
            SceneManager.SetActiveScene(scene);
        else
            Debug.LogWarning($"Active scene {scene.name} not found or is not valid.");
    }

    public static void SetActiveScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        SetActiveScene(scene);
    }

    public static Scene GetScene(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName);
    }

    public static void MoveGameObjectToScene(GameObject objectToMove, Scene scene)
    {
        SceneManager.MoveGameObjectToScene(objectToMove, scene);
    }
}
