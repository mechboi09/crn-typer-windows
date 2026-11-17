using System;using System.Linq;using System.Reflection;using System.Windows.Forms;using CRNTyper;
class SelectionTests {
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Key(LinkedCrnBox b,Keys k){typeof(LinkedCrnBox).GetMethod("OnKeyDown",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(b,new object[]{new KeyEventArgs(k)});}
 static void Release(LinkedCrnBox b){typeof(LinkedCrnBox).GetMethod("OnKeyUp",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(b,new object[]{new KeyEventArgs(Keys.A)});}
 [STAThread]static void Main(){
 using(var form=new Typer()){
 Check(form.Text=="CRN typer","App name");Check(form.Controls.OfType<ModeToggle>().Single().Fast,"Fast default toggle retained");
 var groups=form.Controls.OfType<GroupBox>().ToArray();Check(groups.Length==4,"Four columns");var boxes=groups[0].Controls.OfType<Panel>().SelectMany(p=>p.Controls.OfType<LinkedCrnBox>()).ToArray();Check(boxes.Length==7,"Seven boxes");
 boxes[0].PasteValues("00123,90002\n90003");Key(boxes[0],Keys.Control|Keys.A);Check(boxes[0].SelectionLength==5,"First Ctrl+A selects current CRN");
 var selected=typeof(Typer).GetField("selectedColumn",BindingFlags.Instance|BindingFlags.NonPublic);Check((int)selected.GetValue(form)==-1,"One press doesn't select column");
 Release(boxes[0]);Key(boxes[0],Keys.Control|Keys.A);Check((int)selected.GetValue(form)==0,"Second Ctrl+A selects column");Check(boxes.Take(3).All(b=>b.SelectionLength==5),"All CRN text selected");
 Key(boxes[6],Keys.Delete);Check(boxes.All(b=>b.Text.Length==0),"Delete clears selected column");Check(form.ActiveControl==boxes[0],"Deletion returns to first box");
 Release(boxes[0]);Key(boxes[0],Keys.Control|Keys.A);Release(boxes[0]);Key(boxes[0],Keys.Control|Keys.A);boxes[4].PasteValues("00001 00002");Check(boxes[0].Text=="00001"&&boxes[1].Text=="00002"&&boxes.Skip(2).All(b=>b.Text==""),"Selected column paste replaces from top");
 boxes[0].Text="a00-123";Check(boxes[0].Text=="00123","Digits only");
 }
 Console.WriteLine("PASS: Instant 1-10 CRNs, column selection, Delete, direct column paste, digits only, app name and compact controls. No keys sent.");
 }
}