using SAS.Pool;
using UnityEngine;

[CreateAssetMenu(menuName = "SAS/Pool/Factory/ParticleSystem")]
public class ParticleSystemFactorySO : FactorySO<ParticleSystem>
{
    [SerializeField] private ParticleSystem m_ParticleSystem;
    public override bool Create(string id, out ParticleSystem item)
    {
        item = Instantiate(m_ParticleSystem);
        return m_ParticleSystem.GetComponent<ParticleSystem>();
    }
}
