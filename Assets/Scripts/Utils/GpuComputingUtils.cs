using UnityEngine;

namespace VoxelEngine {
    public static class GpuComputingUtils {
        public static Vector3Int GetThreadGroupsCount(Vector3Int gridSize, int threadGroupSize) {
            return new Vector3Int(
                Mathf.CeilToInt(gridSize.x / (float) threadGroupSize),
                Mathf.CeilToInt(gridSize.y / (float) threadGroupSize),
                Mathf.CeilToInt(gridSize.z / (float) threadGroupSize)
            );
        }

        public static int GetSize(Vector3Int gridSize) {
            return gridSize.x * gridSize.y * gridSize.z;
        }
    }
}