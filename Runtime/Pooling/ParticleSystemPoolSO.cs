using SAS.Pool;
using UnityEngine;

[CreateAssetMenu(menuName = "SAS/Pool/ParticleSystem")]
public class ParticleSystemPoolSO : ComponentPoolSO<ParticleSystem>
{
	[SerializeField] private ParticleSystemFactorySO _factory;
	protected override IFactory<ParticleSystem> Factory
	{
		get => _factory;
	}
}
