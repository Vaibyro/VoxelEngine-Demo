using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace VoxelEngine {
    public class NoiseDensity : Density {
        public ComputeShader computeShader;
        
        private const int ThreadGroupSize = 8;

        [Range(1, 12)]
        public int octaves;

        public float lacunarity;
        public float persistence;
        public float noiseScale;
        public float noiseWeight;
        public float floorOffset;
        public float weightMultiplier;
        public float hardFloor;
        public float hardFloorWeight;
        public Vector3 offset;
        public Vector4 shaderParams;
        
        public int seed;
        
        public override float Get(Vector3 position) {
            FastNoise f = new FastNoise();
            var p2 = position * (1f / noiseScale);
            return f.GetSimplex(p2.x, p2.y, p2.z) * noiseWeight;
        }
        
        public override DensityData GenerateDensityGridGpu(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos) {
            var densityData = new DensityData(gridSize);

            var size = gridSize.x * gridSize.y * gridSize.z;
            var threadsGroupsX = Mathf.CeilToInt (gridSize.x / (float) ThreadGroupSize);
            var threadGroupsY = Mathf.CeilToInt (gridSize.y / (float) ThreadGroupSize);
            var threadGroupsZ = Mathf.CeilToInt (gridSize.z / (float) ThreadGroupSize);
            
            var buffer = new ComputeBuffer(size, sizeof(float));
            buffer.SetData(densityData.Data);
            
            computeShader.SetBuffer(0, "points", buffer);

            // Standard variables
            computeShader.SetInt("sizeX", gridSize.x);
            computeShader.SetInt("sizeY", gridSize.y);
            computeShader.SetInt("sizeZ", gridSize.z);
            computeShader.SetVector("size", boxSize);
            computeShader.SetVector("position", rendererPos);

            // Specific variables
            var offsets = CreateOffsets();
            var offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
            offsetsBuffer.SetData (offsets);
            
            computeShader.SetBuffer (0, "offsets", offsetsBuffer);
            computeShader.SetInt("octaves", octaves);
            computeShader.SetFloat("noiseScale", noiseScale);
            computeShader.SetFloat("noiseWeight", noiseWeight);
            computeShader.SetFloat("persistence", persistence);
            computeShader.SetFloat("weightMultiplier", weightMultiplier);
            computeShader.SetFloat("lacunarity", lacunarity);
            
            // Launch kernels
            computeShader.Dispatch(0, threadsGroupsX, threadGroupsY, threadGroupsZ);

            // Get results
            buffer.GetData(densityData.Data);
            buffer.Release();
            offsetsBuffer.Release();
            
            return densityData;
        }
        
        private Vector3[] CreateOffsets() {
            var prng = new System.Random (seed);
            var offsets = new Vector3[octaves];
            float offsetRange = 1000;
            for (var i = 0; i < octaves; i++) {
                offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
            }

            return offsets;
        }

    }
}
