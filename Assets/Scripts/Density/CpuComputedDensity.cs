using UnityEngine;

namespace VoxelEngine {
    public abstract class CpuComputedDensity : Density {
        public override DensityData GenerateDensityGrid(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos) {
            var densityGrid = new DensityData(gridSize);
            
            for (var xi = 0; xi < gridSize.x; xi++) {
                for (var yi = 0; yi < gridSize.y; yi++) {
                    for (var zi = 0; zi < gridSize.z; zi++) {
                        var posInGrid = new Vector3(
                            (xi / (gridSize.x - 1f)) * boxSize.x,
                            (yi / (gridSize.y - 1f)) * boxSize.y, 
                            (zi / (gridSize.z - 1f)) * boxSize.z
                        );
                        var densityPosition = rendererPos + posInGrid - (boxSize / 2f);
                        var density = Get(densityPosition);
                        densityGrid[xi, yi, zi] = density;
                        
                    }
                }
            }
            
            return densityGrid;
        }
        
        /// <summary>
        /// Calculate surface normal at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 CalculateSurfaceNormal(Vector3 position)
        {
            var h = 0.001f;
            var dx = Get(position + new Vector3(h, 0.0f, 0.0f)) - Get(position - new Vector3(h, 0.0f, 0.0f));
            var dy = Get(position + new Vector3(0.0f, h, 0.0f)) - Get(position - new Vector3(0.0f, h, 0.0f));
            var dz = Get(position + new Vector3(0.0f, 0.0f, h)) - Get(position - new Vector3(0.0f, 0.0f, h));

            return new Vector3(dx, dy, dz).normalized;
        }
    }
}