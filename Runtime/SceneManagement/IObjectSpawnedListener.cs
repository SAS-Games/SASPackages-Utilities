using UnityEngine;
public interface IObjectSpawnedListener
{
	void OnSpawn(GameObject gameObject);
	void OnDespawn(GameObject gameObject);
}
