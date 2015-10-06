using UnityEngine;
using System.Collections;

namespace DeerCat
{

	[RequireComponent(typeof(ParticleSystem))]
	[ExecuteInEditMode]
	public class ParticleTurbulence : MonoBehaviour
	{
		public float frequency = 0.5f;
		public float amplitude = 10.0f;
		public float evolutionSpeed = 1.0f;

		private ParticleSystem system;
		private ParticleSystem.Particle[] particles;

		void Start()
		{
			system = GetComponent<ParticleSystem>();
			particles = new ParticleSystem.Particle[system.maxParticles];
		}

		void Update()
		{
			if (system == null)
				system = GetComponent<ParticleSystem>();
			if (particles == null || system.maxParticles != particles.Length)
				particles = new ParticleSystem.Particle[system.maxParticles];

			if (system.isPlaying)
			{
				int particleCount = system.GetParticles(particles);

				UpdateParticles(particles, particleCount);

				system.SetParticles(particles, particleCount);
			}
		}

		virtual protected void UpdateParticles(ParticleSystem.Particle[] particles, int particleCount)
		{
#if UNITY_EDITOR
			float dT = 1.0f / 60.0f;	// fake deltaTime for use in the scene view
			float time = Time.realtimeSinceStartup;

			if (!Application.isPlaying)
			{
				// check if this particle system is selected (or any parent/child particle system)
				// if it's not then we don't want to update the particles as this particle-system is not currently being previewed in the editor
				bool selected = false;
				ParticleSystem[] systems = GetComponentsInParent<ParticleSystem>();
				for (int i = 0; i < systems.Length && !selected; ++i)
				{
					if (UnityEditor.Selection.Contains(systems[i].gameObject))
						selected = true;
				}
				systems = GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < systems.Length && !selected; ++i)
				{
					if (UnityEditor.Selection.Contains(systems[i].gameObject))
						selected = true;
				}
				if (!selected)
					return;
			}
#else
		float dT = Time.deltaTime;
		float time = Time.time;
#endif

			Vector3 animOffset = evolutionSpeed * time * Vector3.one;
			float speed = amplitude * dT;
			for (int i = 0; i < particleCount; ++i)
			{
				ParticleSystem.Particle particle = particles[i];
				Vector3 pos = particle.position;

				Vector3 noiseP = (pos + animOffset) * frequency;

				pos.x += speed * (Mathf.PerlinNoise(noiseP.y, noiseP.z) - 0.5f);
				pos.y += speed * (Mathf.PerlinNoise(noiseP.z, noiseP.x) - 0.5f);
				pos.z += speed * (Mathf.PerlinNoise(noiseP.x, noiseP.y) - 0.5f);

				particles[i].position = pos;
			}
		}
	}

}
