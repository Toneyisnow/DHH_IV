using System;
using System.Collections.Generic;
namespace MaritimeLegend.Domain {
// Pure C#: one authoritative serial executor; no Scene, GameObject or Unity lifecycle dependencies.
// Task progress resides in GameState (orders/routes/world clock), not in renderer instances.
public sealed class GameWorkflow {
 readonly GameService service;
 readonly Queue<Action<GameService>> commands=new Queue<Action<GameService>>();
 readonly object gate=new object();
 public long ExecutedSteps {get;private set;}
 public bool Active {get;private set;}=true;
 public readonly string[] Tasks={"Production","WorldSimulation","AutoFleet"};
 public GameWorkflow(GameService service){this.service=service;}
 public void Enqueue(Action<GameService> command){if(command==null)throw new ArgumentNullException(nameof(command));lock(gate){if(!Active)throw new InvalidOperationException("Workflow stopped");commands.Enqueue(command);}}
 public void Advance(double seconds,bool worldPaused){if(!Active)return;if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
  lock(gate){int count=Math.Min(service.C.balance.world.maxCommandsPerTick,commands.Count);while(count-->0)commands.Dequeue()(service);}
  // Production continues during a world pause; its explicit production pause setting is honored by Advance.
  service.Advance(seconds,false);
  if(!worldPaused&&!service.DialogueOpen){service.AdvanceWorld(seconds);service.AdvanceAutoFleet(seconds);}
  ExecutedSteps++;
 }
 public void RestoreOffline(DateTime utcNow){if(!Active)throw new InvalidOperationException("Workflow stopped");service.RecoverOffline(utcNow);}
 public void Stop(){lock(gate){Active=false;commands.Clear();}}
}
}
