using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;

public class DensityVisualizer : MonoBehaviour {
    public Density density;

    public float dotSizeFactor = 1.0f;

    public bool sameDotSize = false;
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position,transform.localScale);
        
        var d = density.GenerateDensityGrid(new Vector3Int(10, 10, 10), transform.localScale, transform.position);
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                for (int z = 0; z < 10; z++) {

                    var de = d[x, y, z] * dotSizeFactor;
                    if (de >= 0f) {
                        Gizmos.color = Color.cyan;
                    } else {
                        Gizmos.color = Color.red;
                    }

                    if (sameDotSize) {
                        de = 0.5f * dotSizeFactor;
                    }
                    
                    var unitv = new Vector3(transform.localScale.x / 10f, transform.localScale.y / 10f, transform.localScale.z / 10f);
                    
                    Gizmos.DrawSphere(new Vector3(x * unitv.x, y * unitv.y, z * unitv.z) + transform.position - (transform.localScale / 2f), de);
                }
            }
        }
    }
}
