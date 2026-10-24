using UnityEngine;
namespace MaritimeLegend {
public static class SeaBobbing {
 // Five-second cycle; crest-to-trough travel is one fifth of the drawn hull height.
 public static float Offset(float height,float seconds,int ship)=>Mathf.Sin(seconds*(2*Mathf.PI/5)+ship*.8f)*height*.1f;
}
}
