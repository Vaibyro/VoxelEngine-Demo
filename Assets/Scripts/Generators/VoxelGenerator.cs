using System;
using System.Collections;
using System.Collections.Generic;
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

        public static float MaterialVoid = 0.0f;
        public static float MaterialSolid = 1.0f;

        public abstract Mesh UpdateMesh(Mesh oldMesh);
        
        protected Vector3 GetDensityPosition(Vector3Int coord) {
            return GetDensityPosition(coord.x, coord.y, coord.z);
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
