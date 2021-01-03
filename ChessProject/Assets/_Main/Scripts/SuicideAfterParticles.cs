using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideAfterParticles : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        Destroy(gameObject, _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax);
    }
}
