using UnityEngine;

namespace VoxelEngine {
    public abstract class GpuComputedDensity : Density {
        public ComputeShader computeShader;
        protected virtual int ThreadGroupSize => 8;
    }
}