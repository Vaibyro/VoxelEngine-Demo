using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace VoxelEngine {
    public class NoiseDensity : Density {
        public ComputeShader computeShader;
        
        private const int ThreadGroupSize = 8;

        [Range(1, 5)]
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
            var tgX = Mathf.CeilToInt (gridSize.x / (float) ThreadGroupSize);
            var tgY = Mathf.CeilToInt (gridSize.y / (float) ThreadGroupSize);
            var tgZ = Mathf.CeilToInt (gridSize.z / (float) ThreadGroupSize);
            
            var buffer = new ComputeBuffer(size, sizeof(float));
            buffer.SetData(densityData.Data);
            
            computeShader.SetBuffer(0, "points", buffer);
            
            computeShader.SetInt("sizeX", gridSize.x);
            computeShader.SetInt("sizeY", gridSize.y);
            computeShader.SetInt("sizeZ", gridSize.z);
            computeShader.SetVector("size", boxSize);
            computeShader.SetVector("position", rendererPos);

            computeShader.SetFloat("scale", noiseScale);
            computeShader.SetFloat("weight", noiseWeight);
            
            computeShader.Dispatch(0, tgX, tgY, tgZ);

            buffer.GetData(densityData.Data);
            buffer.Release();
            
            return densityData;
        }
    }
}
