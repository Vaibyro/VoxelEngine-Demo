using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine {
    public class BlocksVoxelGenerator : VoxelGenerator
    {
        public override bool IsProcessing { get; protected set; }
        protected override Task<MeshData> GenerateMeshDataAsync() {
            throw new System.NotImplementedException();
        }
    }
}
