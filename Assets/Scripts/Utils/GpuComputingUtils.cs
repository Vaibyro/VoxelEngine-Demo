using UnityEngine;

namespace VoxelEngine {
    public static class GpuComputingUtils {
        /// <summary>
        /// Method to get GPU Compute Shader thread groups count as a vector of integers from a 3D grid and a default thread group size.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="threadGroupSize"></param>
        /// <returns></returns>
        public static Vector3Int GetThreadGroupsCount(Vector3Int gridSize, int threadGroupSize) {
            return new Vector3Int(
                Mathf.CeilToInt(gridSize.x / (float) threadGroupSize),
                Mathf.CeilToInt(gridSize.y / (float) threadGroupSize),
                Mathf.CeilToInt(gridSize.z / (float) threadGroupSize)
            );
        }

        /// <summary>
        /// Get the total count of elements calculated from a vector of integers.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static int GetSize(Vector3Int gridSize) {
            return gridSize.x * gridSize.y * gridSize.z;
        }
    }
}