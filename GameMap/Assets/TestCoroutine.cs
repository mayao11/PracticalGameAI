using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoroutine : MonoBehaviour {

    bool f2_end = false;
    bool f4_start = false;

    void f1()
    {
        Debug.Log("f1");
    }
    IEnumerator f2()
    {
        Debug.Log("f2  111111111111111");
        yield return 0;
        Debug.Log("f2  222222222222222");
        yield return 0;


        f2_end = true;
        yield return null;
    }


    void f3()
    {
        Debug.Log("f3");
    }

    IEnumerator f4()
    {
        Debug.Log("f4 111111111");
        yield return 0;
        Debug.Log("f4 2222222222");
        yield return 0;
        Debug.Log("f4 3333333333");
        yield return null;
    }

	// Use this for initialization
	void Start () {
        f1();
        StartCoroutine(f2());
        f3();
	}

    private void Update()
    {
        if (f2_end && !f4_start)
        {
            StartCoroutine(f4());
            f4_start = true;
        }

        Debug.Log("-------------------");
    }

}
