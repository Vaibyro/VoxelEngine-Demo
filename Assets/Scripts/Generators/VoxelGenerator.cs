using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine {
    public abstract class VoxelGenerator
    {
        public Density density;
        public float threshold;
        public Vector3 position;
        public Vector3 size;
        public Vector3Int gridSize;
        public bool smoothShade;
        public ComputeShader computeShader;

        protected Vector3 UnitVector => new Vector3(  size.x / gridSize.x, size.y / gridSize.y,size.z / gridSize.z);
        private MeshData _lastMeshData;

        /// <summary>
        /// Get if a mesh data request is operating.
        /// </summary>
        public bool IsProcessing { get; protected set; }
        
        /// <summary>
        /// Get the current mesh data. Caution: it can be null.
        /// </summary>
        public MeshData MeshData => _lastMeshData;
        
        /// <summary>
        /// Get if mesh data is available.
        /// </summary>
        public bool MeshDataAvailable { get; private set; } = false;

        /// <summary>
        /// Request an update of the mesh and make it available in this object when the processing is finished.
        /// </summary>
        /// <returns></returns>
        public async Task RequestMeshDataAsync() {
            if (IsProcessing) {
                Debug.LogWarning("Chunk already processing... Request cancelled.");
                return;
            }
            Debug.Log("Chunk processing.");
            IsProcessing = true;
            var meshData = await GenerateMeshDataAsync(); // generate a mesh
            IsProcessing = false;
            
            _lastMeshData = meshData;
            MeshDataAvailable = true;
            Debug.Log("Chunk processed.");
        }

        /// <summary>
        /// Try to pop the mesh data. If it is available, return true and the mesh in the out parameter, otherwise, return false and null as out parameter.
        /// </summary>
        /// <param name="meshData"></param>
        /// <returns></returns>
        public bool TryPopMeshData(out MeshData meshData) {
            if (!MeshDataAvailable) { // Return false and null as out parameter if not mesh data available.
                meshData = null;
                return false;
            }

            meshData = _lastMeshData;
            MeshDataAvailable = false;
            return true;
        }

        /// <summary>
        /// Pop the mesh data. If you are not sure it is null, please use TryPopMeshData instead.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MeshData PopMeshData() {
            if (!MeshDataAvailable) {
                throw new Exception("Mesh data not available. Please request data mesh generation first.");
            }

            return _lastMeshData;
        }

        /// <summary>
        /// Async method to generate mesh data.
        /// </summary>
        /// <returns></returns>
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
