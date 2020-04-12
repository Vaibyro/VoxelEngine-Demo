using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoxelEngine {
    public class SurfaceNetsGenerator : VoxelGenerator {
        // Buffers
        private readonly List<Vector3> _verticesBuffer = new List<Vector3>();
        private readonly List<int> _trianglesBuffer = new List<int>();
        private readonly Dictionary<Vector3Int, int> _verticesIndicesBuffer = new Dictionary<Vector3Int, int>();

        public override MeshData GenerateMeshData() {
            // PART 1 - Create density map
            var densityGrid = density.GenerateDensityGrid(gridSize, size, position);

            // PART 2 - Create vertices
            var (vertices, vertIndices) = GetVertices(densityGrid);

            // PART 3 - Create faces
            var triangles = GetTriangles(vertIndices, densityGrid);

            return new MeshData(vertices, triangles);
        }

        private (List<Vector3>, Dictionary<Vector3Int, int>) GetVertices(DensityData densityData) {
            var verticesDictionary = new ConcurrentDictionary<Vector3Int, Vector3>();

            Parallel.For(0, gridSize.x - 1, xi => {
                Parallel.For(0, gridSize.y - 1, yi => {
                    Parallel.For(0, gridSize.z - 1, zi => {
                        var vertexPos = Vector3.zero;
                        var counter = 0;

                        // Search sign changes
                        for (var dx = 0; dx <= 1; dx++) {
                            for (var dy = 0; dy <= 1; dy++) {
                                if ((densityData[xi + dx, yi + dy, zi + 0] > 0f) !=
                                    (densityData[xi + dx, yi + dy, zi + 1] > 0f)) {
                                    var change = new Vector3(
                                        dx,
                                        dy,
                                        Adapt(densityData[xi + dx, yi + dy, zi + 0],
                                            densityData[xi + dx, yi + dy, zi + 1])
                                    );

                                    change = Vector3.Scale(change, UnitVector);
                                    vertexPos += change;
                                    counter++;
                                }
                            }
                        }


                        for (var dx = 0; dx <= 1; dx++) {
                            for (var dz = 0; dz <= 1; dz++) {
                                if ((densityData[xi + dx, yi + 0, zi + dz] > 0f) !=
                                    (densityData[xi + dx, yi + 1, zi + dz] > 0f)) {
                                    var change = new Vector3(
                                        dx,
                                        Adapt(densityData[xi + dx, yi + 0, zi + dz],
                                            densityData[xi + dx, yi + 1, zi + dz]),
                                        dz
                                    );

                                    change = Vector3.Scale(change, UnitVector);
                                    vertexPos += change;
                                    counter++;
                                }
                            }
                        }

                        for (var dy = 0; dy <= 1; dy++) {
                            for (var dz = 0; dz <= 1; dz++) {
                                if ((densityData[xi + 0, yi + dy, zi + dz] > 0f) !=
                                    (densityData[xi + 1, yi + dy, zi + dz] > 0f)) {
                                    var change =
                                        new Vector3(
                                            Adapt(densityData[xi + 0, yi + dy, zi + dz],
                                                densityData[xi + 1, yi + dy, zi + dz]),
                                            dy,
                                            dz
                                        );

                                    change = Vector3.Scale(change, UnitVector);
                                    vertexPos += change;
                                    counter++;
                                }
                            }
                        }

                        if (counter != 0) {
                            vertexPos /= (float) counter;
                            verticesDictionary.TryAdd(new Vector3Int(xi, yi, zi),
                                vertexPos + GetDensityPosition(xi, yi, zi));
                        }
                    });
                });
            });

            _verticesBuffer.Clear();
            _verticesIndicesBuffer.Clear();

            foreach (var vertice in verticesDictionary) {
                _verticesIndicesBuffer.Add(vertice.Key, _verticesBuffer.Count);
                _verticesBuffer.Add(vertice.Value);
            }

            return (_verticesBuffer, _verticesIndicesBuffer);
        }

        private List<int> GetTriangles(Dictionary<Vector3Int, int> indexes, DensityData densityData) {
            _trianglesBuffer.Clear();

            for (var xi = 0; xi < gridSize.x - 1; xi++) {
                for (var yi = 0; yi < gridSize.y - 1; yi++) {
                    for (var zi = 0; zi < gridSize.z - 1; zi++) {
                        if (xi > 0 && yi > 0) {
                            var solid1 = densityData[xi, yi, zi + 0] > 0f;
                            var solid2 = densityData[xi, yi, zi + 1] > 0f;
                            if (solid1 != solid2) {
                                if (solid1) {
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 1, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 0, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 1, zi)]);

                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 0, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 0, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 1, zi)]);
                                } else {
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 1, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 0, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 1, zi)]);

                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 0, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi - 1, zi)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi - 0, zi)]);
                                }
                            }
                        }

                        if (xi > 0 && zi > 0) {
                            var solid1 = densityData[xi, yi + 0, zi] > 0f;
                            var solid2 = densityData[xi, yi + 1, zi] > 0f;
                            if (solid1 != solid2) {
                                if (solid1) {
                                    //t1
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 1)]);
                                    //t2
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 0)]);
                                } else {
                                    //t1
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 1)]);
                                    //t2
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 0, yi, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi - 1, yi, zi - 1)]);
                                }
                            }
                        }

                        if (yi > 0 && zi > 0) {
                            var solid1 = densityData[xi + 0, yi, zi] > 0f;
                            var solid2 = densityData[xi + 1, yi, zi] > 0f;

                            if (solid1 != solid2) {
                                if (solid2) {
                                    //t1
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 1)]);
                                    //t2
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 0)]);
                                } else {
                                    //t1
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 1)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 1)]);
                                    //t2
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 0, zi - 0)]);
                                    _trianglesBuffer.Add(indexes[new Vector3Int(xi, yi - 1, zi - 1)]);
                                }
                            }
                        }
                    }
                }
            }

            return _trianglesBuffer;
        }
    }
}