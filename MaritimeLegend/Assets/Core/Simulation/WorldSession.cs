using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
[DefaultExecutionOrder(-200)]
public sealed class WorldSession : MonoBehaviour {
 public Catalog Catalog {get;private set;}
 public GameState State;
 GameService service;
 public GameService Service {get=>service;set{if(ReferenceEquals(service,value))return;if(service!=null)service.Workflow.Stop();service=value;}}
 public GameWorkflow Workflow=>service==null?null:service.Workflow;
 public bool Paused;
 double last;
 void Awake(){DontDestroyOnLoad(gameObject);Catalog=JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("Data/catalog").text);Catalog.Validate();last=Time.realtimeSinceStartupAsDouble;}
 void Update(){double now=Time.realtimeSinceStartupAsDouble;double elapsed=System.Math.Max(0,now-last);last=now;if(Workflow!=null)Workflow.Advance(elapsed,Paused);}
 void OnApplicationQuit(){if(Workflow!=null)Workflow.Stop();}
}
}
