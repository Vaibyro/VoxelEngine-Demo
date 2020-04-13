using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelEngine {
    public enum VoxelMethod // your custom enumeration
    {
        Blocks,
        Shapes,
        Smooth,
        Sharp
    };

    public class VoxelRenderer : MonoBehaviour {
        public VoxelMethod method;
        public Density density;
        public float threshold = 1f;
        public Vector3Int gridSize = new Vector3Int(10, 10, 10);
        public Vector3 size = new Vector3(10f, 10f, 10f);
        public bool smoothShade = true;
        public bool showGizmos = true;
        public bool debugAllFrameUpdate;

        public ComputeShader algoShader;
        
        private VoxelGenerator _generator;

        private readonly IDictionary<VoxelMethod, VoxelGenerator> _generators =
            new Dictionary<VoxelMethod, VoxelGenerator>() {
                {VoxelMethod.Blocks, new BlocksVoxelGenerator()},
                {VoxelMethod.Shapes, new BlocksVoxelGenerator()},
                {VoxelMethod.Smooth, new SurfaceNetsGenerator()},
                {VoxelMethod.Sharp, new BlocksVoxelGenerator()},
            };

        private VoxelMethod _lastMethod;

        //todo temporary
        public MeshFilter meshFilter;

        private void Awake() {
            _generator = _generators[method];
            _generator.computeShader = algoShader;

            // Generate new mesh if it's null
            if (meshFilter.mesh == null) {
                meshFilter.mesh = new Mesh {name = "generatedMesh"};
            }
        }

        private void Start() {
            RequestUpdateGenerator();
            StartCoroutine(test());
        }

        IEnumerator test() {
            while (true) {
                RequestUpdateGenerator();
                yield return new WaitForSeconds(2);
            }
        }
        
        private void Reset() {
            //Debug.Log("test");
        }

        private void Update() {
            // Method change
            if (_lastMethod != method) {
                _generator = _generators[method];
                _lastMethod = method;

                Debug.Log($"Voxel method changed to {method.ToString()}.");
            }

            // Update the mesh
            if (_generator.MeshDataAvailable) {
                var mesh = meshFilter.mesh;
                if (_generator.TryGetMeshData(out var meshData)) {
                    mesh.Clear();
                    mesh.SetVertices(meshData.vertices);
                    mesh.SetTriangles(meshData.triangles, 0);
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                }
            }
        }

        /// <summary>
        /// Method to ask the generator to produce a new mesh.
        /// </summary>
        /// <param name="generator"></param>
        private void RequestUpdateGenerator() {
            if (_generator == null) {
                _generator = _generators[method];
            }

            _generator.density = density;
            _generator.size = size;
            _generator.gridSize = gridSize;
            _generator.threshold = threshold;
            _generator.position = transform.position;
            _generator.smoothShade = smoothShade;
            
            _generator.RequestMeshUpdate(this);
        }

        private void OnDrawGizmos() {
            if (showGizmos) {
                Gizmos.color = Color.blue;
                var trans = transform;
                Gizmos.DrawWireCube(trans.position, size);
            }
        }
    }
}