using System;using System.Linq;using System.IO;using CRNTyper;
class Tests {
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Invalid(Action f){try{f();}catch(ArgumentException){return;}throw new Exception("Invalid input accepted");}
 static void Main(){
#if LAU
 const int max=10;
#else
 const int max=6;
#endif
 for(int n=1;n<=max;n++)foreach(bool fast in new[]{false,true}){var values=Enumerable.Range(1,n).Select(i=>i.ToString("00000")).ToArray();var p=Sequence.Build(values,fast);Check(p.Where(s=>s.Kind=="text").Select(s=>s.Text).SequenceEqual(values),"order");Check(p.Last().Kind=="enter","final Enter");Check(p.Where(s=>s.Kind=="wait").Take(n-1).All(s=>s.Milliseconds==(fast?1:200)),"200ms normal");
#if LAU
 Check(p.Count(s=>s.Kind=="tab")==n-1&&!p.Any(s=>s.Kind=="tab-tab"||s.Kind=="tab-enter"),"LAU navigation");var instant=Sequence.Build(values,false,true);Check(!instant.Any(s=>s.Kind=="wait"),"Instant waits");
#else
 Check(p.Count(s=>s.Kind=="tab-tab")==1,"AUB final grouped Tabs");Check(p.Count(s=>s.Kind==(fast?"tab-enter":"tab"))==n-1,"AUB intermediate navigation");
#endif
 }
 Check(SettingsStore.Parse("00123,90002\n90003\t90004",max).SequenceEqual(new[]{"00123","90002","90003","90004"}),"paste parsing");Invalid(()=>SettingsStore.Parse("1,1",max));Invalid(()=>SettingsStore.Parse("abc",max));Invalid(()=>SettingsStore.Parse("123456",max));Invalid(()=>Sequence.Build(new string[0]));Invalid(()=>Sequence.Build(Enumerable.Range(0,max+1).Select(i=>i.ToString()).ToArray()));
 var directory=Path.Combine(Path.GetTempPath(),"CRN-test-"+Guid.NewGuid());var path=Path.Combine(directory,"settings.xml");var saved=new Saved{Values=new[]{new[]{"00123",""},new[]{"90001"},new[]{""},new[]{"90003"}},Names=new[]{"Main & Backup","Two","Three","Four"},Visible=new[]{3,3,3,3},Speed=1};SettingsStore.Write(path,saved);var result=SettingsStore.Read(path);Check(result.Values[0][0]=="00123"&&result.Names[0]=="Main & Backup"&&result.Speed==1,"save/reload");saved.Values[0][0]="99999";SettingsStore.Write(path,saved);Check(SettingsStore.Read(path).Values[0][0]=="99999"&&SettingsStore.Read(path+".bak").Values[0][0]=="00123","atomic replace and backup");Console.WriteLine("PASS: all sequence sizes/modes, 200ms, paste validation, persistence roundtrip, atomic overwrite, backup, names and leading zeros. No keys sent.");
 }
}