using System;using System.Linq;using System.IO;using CRNTyper;
class Tests {
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Invalid(Action f){try{f();}catch(ArgumentException){return;}throw new Exception("Invalid input accepted");}
 static void Main(){
#if LAU
 const int max=10;
#else
 const int max=7;
#endif
 for(int n=1;n<=max;n++)foreach(bool fast in new[]{false,true}){var values=Enumerable.Range(1,n).Select(i=>i.ToString("00000")).ToArray();var p=Sequence.Build(values,fast);Check(p.Where(s=>s.Kind=="text").Select(s=>s.Text).SequenceEqual(values),"order");Check(p.Last().Kind=="enter","final Enter");Check(p.Where(s=>s.Kind=="wait").Take(n-1).All(s=>s.Milliseconds==(fast?1:200)),"200ms normal");
#if LAU
 Check(p.Count(s=>s.Kind=="tab")==n-1&&!p.Any(s=>s.Kind=="tab-tab"||s.Kind=="tab-enter"),"LAU navigation");var instant=Sequence.Build(values,false,true);Check(!instant.Any(s=>s.Kind=="wait"),"Instant waits");
#else
 Check(p.Count(s=>s.Kind=="tab-tab")==1,"AUB final grouped Tabs");Check(p.Count(s=>s.Kind==(fast?"tab-enter":"tab"))==n-1,"AUB intermediate navigation");
#endif
 }
 Check(CrnLists.Parse("00123,90002\n90003\t90004",max).SequenceEqual(new[]{"00123","90002","90003","90004"}),"paste parsing");Invalid(()=>CrnLists.Parse("1,1",max));Invalid(()=>CrnLists.Parse("abc",max));Invalid(()=>CrnLists.Parse("123456",max));Invalid(()=>Sequence.Build(new string[0]));Invalid(()=>Sequence.Build(Enumerable.Range(0,max+1).Select(i=>i.ToString()).ToArray()));
 using(var form=new Typer()) {
 var keypress=typeof(LinkedCrnBox).GetMethod("OnKeyPress",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
 using(var digitBox=new LinkedCrnBox()){
 foreach(char c in new[]{'a','-',' ','\u0661'}){var e=new System.Windows.Forms.KeyPressEventArgs(c);keypress.Invoke(digitBox,new object[]{e});Check(e.Handled,"Reject non-digit");}
 foreach(char c in new[]{'0','9','\b'}){var e=new System.Windows.Forms.KeyPressEventArgs(c);keypress.Invoke(digitBox,new object[]{e});Check(!e.Handled,"Allow digits and editing");}
 digitBox.Text="a00-12z";Check(digitBox.Text=="0012","Non-typing input filtered");
 }
 var mode=form.Controls.OfType<ModeToggle>().Single();Check(mode.Fast,"Fast is default");Check(!form.Controls.OfType<System.Windows.Forms.ComboBox>().Any(),"No dropdown");var modeKey=typeof(ModeToggle).GetMethod("OnKeyDown",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);modeKey.Invoke(mode,new object[]{new System.Windows.Forms.KeyEventArgs(System.Windows.Forms.Keys.Left)});Check(!mode.Fast,"Left selects Normal");modeKey.Invoke(mode,new object[]{new System.Windows.Forms.KeyEventArgs(System.Windows.Forms.Keys.Right)});Check(mode.Fast,"Right selects Fast");
 var groups=form.Controls.OfType<System.Windows.Forms.GroupBox>().ToArray();Check(groups.Length==4&&groups[0].Text=="Option 1","compact columns");
 Check(form.Controls.OfType<System.Windows.Forms.Button>().Count()==1&&form.Controls.OfType<System.Windows.Forms.Button>().Single().Text=="Tips","Tip button only");Check(groups.All(g=>!g.Controls.OfType<System.Windows.Forms.Button>().Any()),"No Paste buttons");Check(groups.All(g=>g.Controls.OfType<System.Windows.Forms.Panel>().Count()==7),"Seven boxes per option");var boxes=groups[0].Controls.OfType<System.Windows.Forms.Panel>().SelectMany(panel=>panel.Controls.OfType<LinkedCrnBox>()).ToArray();
 boxes[0].PasteValues("00123,90002\n90003");Check(boxes[0].Text=="00123"&&boxes[1].Text=="90002"&&boxes[2].Text=="90003","linked paste");
#if LAU
 Check(!form.Controls.OfType<System.Windows.Forms.ComboBox>().Any()&&!form.Controls.OfType<System.Windows.Forms.Label>().Any(),"LAU has no speed selector or captions");
#endif
 }
 Console.WriteLine("PASS: sequence sizes and modes, 200ms Normal, paste parsing, validation and leading zeros. No keys sent.");
 }
}