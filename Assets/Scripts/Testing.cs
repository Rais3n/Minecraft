using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public delegate void Function();

    private Function function;
    
    private void Start()
    {
        Test(T2);
    }

    private void Test(Function function)
    {
        function();
    }
    private void T2()
    {
        Debug.Log("JEST");
    }
}
