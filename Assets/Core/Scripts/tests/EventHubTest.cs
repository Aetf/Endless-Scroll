using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnlimitedCodeWorks;

using Debug = UnityEngine.Debug;

public class MyBase : GameEvent {
}
public class MyDerived : MyBase {
}
public class MyMostDerived : MyDerived {
}

public class TestEventHub : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void ScopeTest() {
        Debug.Log("=== Scope Test ===");
        EventHub.Cleanup();

        EventHub.AddListener<GameEvent>(e => { Debug.Log("GameEvent listener, event is " + e.GetType().Name); });
        EventHub.AddListener<MyBase>(e => { Debug.Log("MyBase listener, event is " + e.GetType().Name); });
        EventHub.AddListener<MyDerived>(e => { Debug.Log("MyDerived listener, event is " + e.GetType().Name); e.StopPropagation = true; });
        EventHub.AddListener<MyMostDerived>(e => { Debug.Log("MyMostDerived listener, event is " + e.GetType().Name); });

        EventHub.RaiseEvent(new MyMostDerived());
    }

    void SpeedTest() {
        Debug.Log("=== Speed Test ===");
        EventHub.Cleanup();


        EventHub.AddListener<MyBase>(e => { } );

        int maxcnt = 10000000;
        int cnt = maxcnt;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (cnt-- > 0) {
            EventHub.RaiseEvent(new MyMostDerived());
        }
        sw.Stop();

        Debug.Log(string.Format("Result: {0} times, {1:F3}s, average {2:F3}ms", maxcnt, sw.ElapsedMilliseconds/1000f, sw.ElapsedMilliseconds/(float) maxcnt));
    }
}
