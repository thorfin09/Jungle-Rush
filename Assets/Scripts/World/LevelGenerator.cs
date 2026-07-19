using System.Collections.Generic;
using UnityEngine;
using EndlessRunner.Player;

namespace EndlessRunner.World
{
    /// <summary>
    /// Procedural runner track generator.
    /// Manages tile positioning, difficulty progression, biome shifting, and fog/skybox lerps.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private List<BiomeData> biomes;

        [Header("Track Spawning")]
        [SerializeField] private int initialSegments = 6;
        [SerializeField] private float safeDistance = 35f; // Distance behind player before recycling tile
        
        [Header("Biomes & Transition")]
        [SerializeField] private int segmentsPerBiome = 20;
        [SerializeField] private float environmentTransitionSpeed = 1.5f;

        [Header("Spawning Rates (0.0 to 1.0)")]
        [SerializeField] private float obstacleChance = 0.45f;
        [SerializeField] private float coinChance = 0.35f;
        [SerializeField] private float powerUpChance = 0.05f;

        private readonly Queue<TrackSegment> activeSegments = new Queue<TrackSegment>();
        private float spawnZ = 0f;
        private int currentBiomeIndex = 0;
        private int segmentsSpawnedInCurrentBiome = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (biomes == null || biomes.Count == 0)
            {
                Debug.LogWarning("LevelGenerator: No BiomeData configurations assigned.");
                return;
            }

            // Prime the initial environment visual setup
            ApplyBiomeEnvironment(biomes[currentBiomeIndex]);

            // Spawn the first few segments without obstacles so the player starts safely
            for (int i = 0; i < initialSegments; i++)
            {
                bool spawnHazards = i >= 2;
                SpawnSegment(spawnHazards);
            }
        }

        private void Update()
        {
            if (player == null || activeSegments.Count == 0) return;

            // Check if player has advanced past the oldest tile + safe spacing buffer
            TrackSegment oldest = activeSegments.Peek();
            if (player.transform.position.z - safeDistance > oldest.transform.position.z + oldest.Length)
            {
                RecycleOldestSegment();
                SpawnSegment(true);
            }

            // Interpolate fog parameters for smooth transitions
            LerpEnvironmentSettings();
        }

        /// <summary>
        /// Instantiates a new segment at the current Z marker and populates it.
        /// </summary>
        private void SpawnSegment(bool spawnHazards)
        {
            BiomeData currentBiome = biomes[currentBiomeIndex];
            
            if (currentBiome.trackPrefabs == null || currentBiome.trackPrefabs.Length == 0)
            {
                Debug.LogError($"Biome {currentBiome.biomeName} has no track prefabs assigned!");
                return;
            }

            GameObject trackPrefab = currentBiome.trackPrefabs[Random.Range(0, currentBiome.trackPrefabs.Length)];
            
            // Get track segment from our ObjectPooler
            GameObject segmentObj = ObjectPooler.Instance.Get(trackPrefab, new Vector3(0f, 0f, spawnZ), Quaternion.identity);
            TrackSegment segment = segmentObj.GetComponent<TrackSegment>();

            if (segment == null)
            {
                Debug.LogError($"Segment prefab '{trackPrefab.name}' is missing the TrackSegment component!");
                return;
            }

            if (spawnHazards)
            {
                PopulateSegment(segment, currentBiome);
            }

            activeSegments.Enqueue(segment);
            spawnZ += segment.Length;
            segmentsSpawnedInCurrentBiome++;

            // Handle Biome Cycling
            if (segmentsSpawnedInCurrentBiome >= segmentsPerBiome)
            {
                TransitionToNextBiome();
            }
        }

        /// <summary>
        /// Populates a segment with obstacles, coins, and power-ups based on spawn points and biome chances.
        /// </summary>
        private void PopulateSegment(TrackSegment segment, BiomeData biome)
        {
            // 1. Spawning Obstacles
            if (biome.obstaclePrefabs != null && biome.obstaclePrefabs.Length > 0 && segment.ObstacleSpawnPoints != null)
            {
                foreach (Transform pt in segment.ObstacleSpawnPoints)
                {
                    if (Random.value < obstacleChance)
                    {
                        GameObject prefab = biome.obstaclePrefabs[Random.Range(0, biome.obstaclePrefabs.Length)];
                        GameObject item = ObjectPooler.Instance.Get(prefab, pt.position, pt.rotation);
                        segment.RegisterSpawnedItem(item);
                    }
                }
            }

            // 2. Spawning Power-Ups
            if (biome.powerUpPrefabs != null && biome.powerUpPrefabs.Length > 0 && segment.PowerUpSpawnPoints != null)
            {
                foreach (Transform pt in segment.PowerUpSpawnPoints)
                {
                    if (Random.value < powerUpChance)
                    {
                        GameObject prefab = biome.powerUpPrefabs[Random.Range(0, biome.powerUpPrefabs.Length)];
                        GameObject item = ObjectPooler.Instance.Get(prefab, pt.position, pt.rotation);
                        segment.RegisterSpawnedItem(item);
                        // Prevent coin spawning on top of a powerup
                        continue;
                    }
                }
            }

            // 3. Spawning Coins
            if (biome.coinPrefabs != null && biome.coinPrefabs.Length > 0 && segment.CoinSpawnPoints != null)
            {
                foreach (Transform pt in segment.CoinSpawnPoints)
                {
                    if (Random.value < coinChance)
                    {
                        GameObject prefab = biome.coinPrefabs[Random.Range(0, biome.coinPrefabs.Length)];
                        GameObject item = ObjectPooler.Instance.Get(prefab, pt.position, pt.rotation);
                        segment.RegisterSpawnedItem(item);
                    }
                }
            }
        }

        private void RecycleOldestSegment()
        {
            TrackSegment oldest = activeSegments.Dequeue();
            oldest.Recycle();
        }

        private void TransitionToNextBiome()
        {
            segmentsSpawnedInCurrentBiome = 0;
            currentBiomeIndex = (currentBiomeIndex + 1) % biomes.Count;
            ApplyBiomeEnvironment(biomes[currentBiomeIndex]);
        }

        private void ApplyBiomeEnvironment(BiomeData biome)
        {
            if (biome.skyboxMaterial != null)
            {
                RenderSettings.skybox = biome.skyboxMaterial;
            }
            RenderSettings.fog = true;
        }

        private void LerpEnvironmentSettings()
        {
            if (biomes == null || biomes.Count == 0) return;
            
            BiomeData targetBiome = biomes[currentBiomeIndex];
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetBiome.fogColor, environmentTransitionSpeed * Time.deltaTime);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetBiome.fogDensity, environmentTransitionSpeed * Time.deltaTime);
        }
    }
}
