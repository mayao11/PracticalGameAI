using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathData : MonoBehaviour {

    public float F
    {
        get
        {
            return G + H;
        }
    }
    public float G;
    public float H;
    public int dir_x;
    public int dir_y;

}
