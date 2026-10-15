using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public class GameAudio : MonoBehaviour {
  AudioSource music,sfx; AudioClip[] themes; AudioClip[] cues; string current="";
  void Awake(){music=gameObject.AddComponent<AudioSource>();sfx=gameObject.AddComponent<AudioSource>();music.loop=true;themes=new[]{MakeTheme(0),MakeTheme(1),MakeTheme(2)};cues=new AudioClip[11];for(int i=0;i<cues.Length;i++)cues[i]=Tone(i);}
  public void Apply(Settings s,string scene){music.volume=s.master*s.music*.35f;sfx.volume=s.master*s.sfx*.45f;string key=scene=="标题"||scene=="存档"||scene=="模板"||scene=="定制"?"标题":scene=="航海"||scene=="战斗"?"航海":"城市";if(key!=current){current=key;music.clip=themes[key=="标题"?0:key=="城市"?1:2];music.Play();}}
  public void Cue(int i){sfx.PlayOneShot(cues[Mathf.Clamp(i,0,cues.Length-1)]);}
  AudioClip Tone(int kind){int rate=22050;float length=kind>=9?.8f:.12f;float[] data=new float[(int)(rate*length)];for(int i=0;i<data.Length;i++){double t=i/(double)rate,f=kind==2?330-180*t:300+kind*49+120*t;data[i]=(float)(Math.Sin(t*f*Math.PI*2)*Math.Exp(-t*7)*.25);}var c=AudioClip.Create("原创音效"+kind,data.Length,1,rate,false);c.SetData(data,0);return c;}
  AudioClip MakeTheme(int type){int rate=22050;double beat=type==2?.38:.5;int[] melody=type==0?new[]{0,7,12,10,7,5,3,7,0,3,5,7,10,7,5,3}:type==1?new[]{0,4,7,9,7,4,2,4,5,9,12,9,7,4,2,0}:new[]{0,3,7,10,7,3,5,7,0,3,7,12,10,7,5,3};float[] data=new float[(int)(rate*beat*64)];for(int i=0;i<data.Length;i++){double t=i/(double)rate;int step=(int)(t/beat);double phase=t%beat,frequency=220*Math.Pow(2,melody[step%16]/12.0);double m=(Math.Sin(2*Math.PI*frequency*t)+.2*Math.Sin(4*Math.PI*frequency*t))*Math.Exp(-phase*5);double bass=110*Math.Pow(2,new[]{0,5,3,7}[(step/16)%4]/12.0);data[i]=(float)(m*.12+Math.Sin(2*Math.PI*bass*t)*.07*(.8+.2*Math.Sin(t)));}var clip=AudioClip.Create("原创航海主题"+type,data.Length,1,rate,false);clip.SetData(data,0);return clip;}
 }
}
