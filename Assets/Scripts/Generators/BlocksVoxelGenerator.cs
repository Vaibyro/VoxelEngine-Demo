using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelEngine {
    public class BlocksVoxelGenerator : VoxelGenerator
    {
        public override bool IsProcessing { get; protected set; }
        public override Task GenerateMeshDataAsync() {
            throw new System.NotImplementedException();
        }
    }
}
