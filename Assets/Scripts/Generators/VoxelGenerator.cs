using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine {
    public abstract class VoxelGenerator
    {
        protected Vector3 UnitVector => new Vector3(  size.x / gridSize.x, size.y / gridSize.y,size.z / gridSize.z);
        public Density density;
        public float threshold;
        public Vector3 position;
        public Vector3 size;
        public Vector3Int gridSize;
        public bool smoothShade;
        public ComputeShader computeShader;

        /// <summary>
        /// Queue used to store processed meshData.
        /// </summary>
        private MeshData _lastMeshData;

        public abstract bool IsProcessing { get; protected set; }
        
        public MeshData MeshData => _lastMeshData;
        
        public bool MeshDataAvailable { get; private set; } = false;

        public async Task RequestMeshDataAsync() {
            if (IsProcessing) {
                Debug.LogWarning("Chunk already processing... Request cancelled.");
                return;
            }
            Debug.Log("Chunk processing.");
            IsProcessing = true;
            var meshData = await GenerateMeshDataAsync();
            IsProcessing = false;
            
            _lastMeshData = meshData;
            MeshDataAvailable = true;
            Debug.Log("Chunk processed.");
        }

        public bool TryPopMeshData(out MeshData meshData) {
            if (!MeshDataAvailable) {
                meshData = null;
                return false;
            }

            meshData = _lastMeshData;
            MeshDataAvailable = false;
            return true;
        }

        public MeshData PopMeshData() {
            if (!MeshDataAvailable) {
                throw new Exception("Mesh data not available. Please request data mesh generation first.");
            }

            return _lastMeshData;
        }

        
        protected abstract Task<MeshData> GenerateMeshDataAsync();
        
        protected Vector3 GetDensityPosition(Vector3Int coordinates) {
            return GetDensityPosition(coordinates.x, coordinates.y, coordinates.z);
        }

        protected Vector3 GetDensityPosition(int x, int y, int z) {
            var posInGrid = new Vector3(
                (x / (gridSize.x - 1f)) * size.x,
                (y / (gridSize.y - 1f)) * size.y, 
                (z / (gridSize.z - 1f)) * size.z
            );
                        
            return posInGrid - (size / 2f);
        }

        protected static float Adapt(float v0, float v1) {
            return (0 - v0) / (v1 - v0);
        }
    }
}
