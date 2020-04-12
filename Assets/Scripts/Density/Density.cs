using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VoxelEngine {
    public abstract class Density : MonoBehaviour {
        /// <summary>
        /// Get density value at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public abstract float Get(Vector3 position);

        public bool gpuComputation = false;

        public DensityData GenerateDensityGridCpu(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos) {
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

        public abstract DensityData GenerateDensityGridGpu(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos);
        
        public DensityData GenerateDensityGrid(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos) {
            if (gpuComputation) {
                return GenerateDensityGridGpu(gridSize, boxSize, rendererPos);
            } else {
                return GenerateDensityGridCpu(gridSize, boxSize, rendererPos);
            }
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
