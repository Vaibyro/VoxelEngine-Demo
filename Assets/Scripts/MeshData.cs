using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine {
    public class MeshData {
        public List<Vector3> vertices;
        public List<int> triangles;

        public MeshData(List<Vector3> vertices, List<int> triangles) {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }
}
