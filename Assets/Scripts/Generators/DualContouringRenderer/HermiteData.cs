using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine {
    
    public class HermiteData {
        public Vector3Int DensityIndex { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public float DensityValue { get; set; }
        
        public HermiteData(Vector3Int densityIndex, Vector3 position, float densityValue, Vector3 normal) {
            DensityIndex = densityIndex;
            Position = position;
            DensityValue = densityValue;
            Normal = normal;
        }
    }
}

