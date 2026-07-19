using UnityEngine;

namespace EndlessRunner.World
{
    /// <summary>
    /// Configuration asset for a visual biome.
    /// Defines track layouts, local obstacles, collectibles, and environmental settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Biome", menuName = "Endless Runner/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        [Header("General Settings")]
        public string biomeName;
        
        [Header("Asset Pools")]
        public GameObject[] trackPrefabs;
        public GameObject[] obstaclePrefabs;
        public GameObject[] coinPrefabs;
        public GameObject[] powerUpPrefabs;
        
        [Header("Environment Styles")]
        public Material skyboxMaterial;
        public Color fogColor = Color.grey;
        public float fogDensity = 0.015f;
    }
}
