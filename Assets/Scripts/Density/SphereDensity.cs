using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine {
    public class SphereDensity : Density {
        public float radius;
        public ComputeShader computeShader;

        private const int ThreadGroupSize = 8;

        public override float Get(Vector3 position) {
            return Vector3.Magnitude(position - transform.position) - radius;
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
            
            computeShader.SetFloat ("radius", radius);
            computeShader.SetInt("sizeX", gridSize.x);
            computeShader.SetInt("sizeY", gridSize.y);
            computeShader.SetInt("sizeZ", gridSize.z);
            computeShader.SetVector("size", boxSize);
            computeShader.SetVector("position", rendererPos);
            computeShader.SetVector("sphereCenter", transform.position);
            
            computeShader.Dispatch(0, tgX, tgY, tgZ);

            buffer.GetData(densityData.Data);
            buffer.Release();

            return densityData;
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
