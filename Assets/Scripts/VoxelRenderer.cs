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

            UpdateGenerator(_generator);

            /*
            // Add meshfilter + renderer
            if (GetComponent<MeshFilter>() == null) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            
            if (GetComponent<MeshRenderer>() == null) {
                gameObject.AddComponent<MeshRenderer>();
            }
            */

            // Generate new mesh if it's null
            if (meshFilter.mesh == null) {
                meshFilter.mesh = new Mesh {name = "generatedMesh"};
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

            if (debugAllFrameUpdate) {
                UpdateGenerator(_generator);
            }
        }

        /// <summary>
        /// Method to ask the generator to produce a new mesh.
        /// </summary>
        /// <param name="generator"></param>
        private void UpdateGenerator(VoxelGenerator generator) {
            if (_generator == null) {
                _generator = _generators[method];
            }

            _generator.density = density;
            _generator.size = size;
            _generator.gridSize = gridSize;
            _generator.threshold = threshold;
            _generator.position = transform.position;
            _generator.smoothShade = smoothShade;

            meshFilter.mesh = _generator.UpdateMesh(meshFilter.mesh);
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