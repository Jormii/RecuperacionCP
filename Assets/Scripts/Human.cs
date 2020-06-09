using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Human : MonoBehaviour, IStateMachine
{
    public Navigation navigation;

    private void Start()
    {
        navigation = GetComponent<Navigation>();
    }

    public abstract void OnStoreSeen(Store store);

    // IStateMachine
    public abstract void Init();
    public abstract void DeInit();

}
