using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine {
    public abstract class VoxelGenerator
    {
        public Vector3 UnitVector => new Vector3(  size.x / gridSize.x, size.y / gridSize.y,size.z / gridSize.z);
        public Density density;
        public float threshold;
        public Vector3 position;
        public Vector3 size;
        public Vector3Int gridSize;
        public bool smoothShade;

        private volatile bool processingNewMesh = false;

        protected ConcurrentQueue<MeshData> meshDataQueue = new ConcurrentQueue<MeshData>();

        public bool MeshDataAvailable => meshDataQueue.Count > 0;
        
        public static readonly Vector3[] CubeVertices =
        {
            new Vector3( 0, 0, 0 ),
            new Vector3( 0, 0, 1 ),
            new Vector3( 0, 1, 0 ),
            new Vector3( 0, 1, 1 ),
            new Vector3( 1, 0, 0 ),
            new Vector3( 1, 0, 1 ),
            new Vector3( 1, 1, 0 ),
            new Vector3( 1, 1, 1 ),
        };

        public abstract MeshData GenerateMeshData();

        public Task RequestMeshUpdate() {
            var task = new Task( () => {
                processingNewMesh = true;
                var meshData = GenerateMeshData(); // todo: problem, accessed objects from the voxel generator are not thread safe
                meshDataQueue.Enqueue(meshData);
                processingNewMesh = false;
            });
            task.Start();
            return task;
        }
        
        protected Vector3 GetDensityPosition(Vector3Int coordinates) {
            return GetDensityPosition(coordinates.x, coordinates.y, coordinates.z);
        }

        public bool TryGetMeshData(out MeshData meshdata) {
            // Flush the beginning of the queue
            while (meshDataQueue.Count > 1) {
                meshDataQueue.TryDequeue(out _);
            }

            // If queue is empty, mesh data is not ready yet.
            if (meshDataQueue.Count < 1) {
                if (!processingNewMesh) {
                    RequestMeshUpdate(); // Process a new mesh if not requested
                }
                meshdata = null;
                return false;
            }

            return meshDataQueue.TryDequeue(out meshdata);
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
