using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine {
    public class DensityData {
        private float[] _data;

        public float[] Data {
            get => _data;
            set => _data = value;
        }
        
        public Vector3Int Size { get; }

        public DensityData(Vector3Int size) {
            Size = size;
            _data = new float[size.x * size.y * size.z];
        }

        public float this[int i] {
            get => _data[i];
            set => _data[i] = value;
        }
        
        public float this[int x, int y, int z] {
            get => _data[x + Size.x * (y + Size.y * z)];
            set => _data[x + Size.x * (y + Size.y * z)] = value;
        }
    }
}
