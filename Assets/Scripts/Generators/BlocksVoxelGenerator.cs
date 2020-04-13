using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine {
    public class BlocksVoxelGenerator : VoxelGenerator
    {
        public override bool IsProcessing { get; protected set; }
        public override IEnumerator GenerateMeshData() {
            throw new System.NotImplementedException();
        }
    }
}
