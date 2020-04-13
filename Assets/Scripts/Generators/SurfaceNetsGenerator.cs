using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace VoxelEngine {
    public class SurfaceNetsGenerator : VoxelGenerator {
        
        // Buffers
        private readonly List<Vector3> _verticesBuffer = new List<Vector3>();
        private readonly List<int> _trianglesBuffer = new List<int>();
        private readonly Dictionary<Vector3Int, int> _verticesIndicesBuffer = new Dictionary<Vector3Int, int>();

        public override Mesh UpdateMesh(Mesh oldMesh) {
            // PART 1 - Create density map
            var densityGrid = density.GenerateDensityGrid(gridSize, size, position);

            // PART 2 - Create vertices
            var (vertices, vertIndices) = GetVertices(densityGrid);
            
            // PART 3 - Create faces
            var triangles = GetTriangles(vertices, vertIndices, densityGrid);

            // PART 4 - Update
            oldMesh.Clear();
            oldMesh.SetVertices(vertices);
            oldMesh.SetTriangles(triangles, 0);
            oldMesh.RecalculateNormals();
            oldMesh.RecalculateBounds();

            return oldMesh;
        }

        struct Vertex {
            public uint x;
            public uint y;
            public uint z;
            public Vector3 pos;
        }
        
        private (List<Vector3>, Dictionary<Vector3Int, int>) GetVertices(DensityData densityData) {
             _verticesBuffer.Clear();
             _verticesIndicesBuffer.Clear();
             
             var ThreadGroupSize = 8;

             var s = gridSize.x * gridSize.y * gridSize.z;
             var threadsGroupsX = Mathf.CeilToInt (gridSize.x / (float) ThreadGroupSize);
             var threadGroupsY = Mathf.CeilToInt (gridSize.y / (float) ThreadGroupSize);
             var threadGroupsZ = Mathf.CeilToInt (gridSize.z / (float) ThreadGroupSize);
            
             var densityBuffer = new ComputeBuffer(s, sizeof(float));
             densityBuffer.SetData(densityData.Data);
             computeShader.SetBuffer(0, "densityData", densityBuffer);

             var verticesBuffer = new ComputeBuffer (s, sizeof (uint) * 3 + sizeof(float) * 3, ComputeBufferType.Append);
             computeShader.SetBuffer (0, "vertices", verticesBuffer);
             verticesBuffer.SetCounterValue (0);
             
             var vCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);
             
             // Standard variables
             computeShader.SetInt("sizeX", gridSize.x);
             computeShader.SetInt("sizeY", gridSize.y);
             computeShader.SetInt("sizeZ", gridSize.z);
             computeShader.SetVector("size", this.size);
             computeShader.SetVector("position", this.position);
             computeShader.SetVector("UnitVector", this.UnitVector);
             
             // Launch kernels
             computeShader.Dispatch(0, threadsGroupsX, threadGroupsY, threadGroupsZ);

             // Count of vertices
             ComputeBuffer.CopyCount (verticesBuffer, vCountBuffer, 0);
             int[] triCountArray = { 0 };
             vCountBuffer.GetData (triCountArray);
             var numV = triCountArray[0];
             
             // Get results
             var verticesA = new Vertex[numV];
             verticesBuffer.GetData (verticesA, 0, 0, numV);
             
             // Release buffers
             densityBuffer.Release();
             verticesBuffer.Release();
             vCountBuffer.Release();
             
             foreach (var vertexA in verticesA) {
                 _verticesIndicesBuffer.Add(new Vector3Int((int)vertexA.x, (int)vertexA.y, (int)vertexA.z), _verticesBuffer.Count);
                 _verticesBuffer.Add(vertexA.pos);
             }
             
             return (_verticesBuffer, _verticesIndicesBuffer);
        }
         
        private List<int> GetTriangles(IEnumerable<Vector3> vertices, Dictionary<Vector3Int, int> indexes, DensityData densityData) {
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