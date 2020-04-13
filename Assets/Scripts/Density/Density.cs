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

        /// <summary>
        /// Generate a density 3D grid.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="boxSize"></param>
        /// <param name="rendererPos"></param>
        /// <returns></returns>
        public abstract DensityData GenerateDensityGrid(Vector3Int gridSize, Vector3 boxSize, Vector3 rendererPos);
    }
}
