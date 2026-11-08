using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRNTyper {
    public class Step {
        public string Kind, Text;
        public int Milliseconds;
        public Step(string kind, string text = "", int milliseconds = 0) { Kind = kind; Text = text; Milliseconds = milliseconds; }
    }
    public static class Sequence {
        public static List<Step> Build(string[] values, bool fast = false) {
            if (values.Length == 0 || values.Length > 6) throw new ArgumentException("Enter 1 to 6 CRNs.");

            if (values.Any(v => v.Length < 1 || v.Length > 5 || v.Any(c => c < '0' || c > '9'))) throw new ArgumentException("CRNs must contain 1 to 5 digits.");
            if (values.Distinct().Count() != values.Length) throw new ArgumentException("Remove duplicate CRNs from this option.");
            var result = new List<Step>();
            for (int i = 0; i < values.Length; i++) {
                result.Add(new Step("text", values[i]));
                if (i + 1 < values.Length) {
                    result.Add(new Step(fast ? "tab-enter" : "tab"));
                    result.Add(new Step("wait", "", fast ? 1 : 300));
                }
            }
            result.Add(new Step("tab-tab"));
            result.Add(new Step("wait", "", fast ? 1 : 80));
            result.Add(new Step("enter"));
            return result;
        }
    }
    public static class Native {
        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
        [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll", SetLastError=true)] static extern uint SendInput(uint count, INPUT[] inputs, int size);
        [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint type; public UNION data; }
        [StructLayout(LayoutKind.Explicit)] struct UNION {
            [FieldOffset(0)] public KEYBOARD keyboard;
            [FieldOffset(0)] public MOUSE mouse;
        }
        [StructLayout(LayoutKind.Sequential)] struct KEYBOARD { public ushort key, scan; public uint flags, time; public UIntPtr extra; }
        [StructLayout(LayoutKind.Sequential)] struct MOUSE { public int x,y; public uint mouseData,flags,time; public UIntPtr extra; }
        static INPUT Key(ushort value, bool up, bool unicode) {
            return new INPUT { type=1, data=new UNION {keyboard=new KEYBOARD {key=unicode?(ushort)0:value,scan=unicode?value:(ushort)0,flags=(up?2u:0u)|(unicode?4u:0u)}}};
        }
        public static bool Held(int key) { return (GetAsyncKeyState(key) & 0x8000) != 0; }
        public static void Send(string kind, string text) {
            var list=new List<INPUT>();
            if (kind=="text") foreach(char c in text) { list.Add(Key(c,false,true));list.Add(Key(c,true,true)); }
            else {
                ushort[] keys=kind=="tab-tab"?new ushort[]{9,9}:kind=="tab-enter"?new ushort[]{9,13}:kind=="tab"?new ushort[]{9}:new ushort[]{13};
                foreach(ushort k in keys) {list.Add(Key(k,false,false));list.Add(Key(k,true,false));}
            }
            var input=list.ToArray();
            if (SendInput((uint)input.Length,input,Marshal.SizeOf(typeof(INPUT))) != input.Length) throw new InvalidOperationException("Windows could not deliver the keystrokes. Typing stopped.");
        }
    }
    public class Typer : Form {
        readonly TextBox[][] boxes=new TextBox[4][];
        readonly Label state=new Label();
        readonly ComboBox speed=new ComboBox();
        readonly bool[] registered=new bool[4];
        CancellationTokenSource cancellation;
        bool running;
        string hotkeyWarning="";
        public Typer() {
            Text="CRN Typer"; ClientSize=new Size(428,428); FormBorderStyle=FormBorderStyle.FixedSingle;
            MaximizeBox=false; StartPosition=FormStartPosition.CenterScreen; AutoScaleMode=AutoScaleMode.Dpi;
            Font=new Font("Segoe UI",9); BackColor=SystemColors.Control;
            for(int col=0;col<4;col++) {
                var group=new GroupBox {Text="Option "+(col+1),Location=new Point(10+col*104,10),Size=new Size(98,222)};
                boxes[col]=new TextBox[6];
                for(int row=0;row<6;row++) {
                    var box=new TextBox {Location=new Point(10,25+row*30),Size=new Size(77,24),MaxLength=5,TabIndex=col*6+row};
                    box.AccessibleName="Option "+(col+1)+", CRN "+(row+1);
                    box.KeyPress+=(s,e)=>{if(!char.IsControl(e.KeyChar)&&(e.KeyChar<'0'||e.KeyChar>'9')) e.Handled=true;};
                    group.Controls.Add(box);boxes[col][row]=box;
                }
                Controls.Add(group);
            }
            for(int i=0;i<4;i++) Controls.Add(new Label {Text="Press F"+(i+1)+" for option "+(i+1),Location=new Point(15,242+i*23),AutoSize=true});
            speed.DropDownStyle=ComboBoxStyle.DropDownList;speed.Location=new Point(15,337);speed.Size=new Size(397,26);speed.Items.AddRange(new object[]{"Normal (300 ms)","Fast (1 ms requested)"});speed.SelectedIndex=0;speed.SelectedIndexChanged+=(s,e)=>SetState(speed.SelectedIndex==1?"fast is guaranteed dean’s warning 😈 (just kidding)":"Normal mode — automatic, 300 ms.");Controls.Add(speed);
            state.Location=new Point(15,375);state.Size=new Size(397,45);state.Text="Ready. Focus the first empty CRN box, then press F1–F4.\nEscape cancels. Ends at Add to Summary.";Controls.Add(state);
            FormClosing+=(s,e)=>{if(cancellation!=null)cancellation.Cancel();};
        }
        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            var failed=new List<string>();
            for(int i=0;i<4;i++){registered[i]=Native.RegisterHotKey(Handle,100+i,0x4000,(uint)(0x70+i));if(!registered[i])failed.Add("F"+(i+1));}
            if(failed.Count>0){hotkeyWarning="Unavailable shortcuts: "+string.Join(", ",failed)+". Close the other hotkey app and restart.";state.Text=hotkeyWarning;state.ForeColor=Color.Firebrick;}
        }
        protected override void OnHandleDestroyed(EventArgs e) {
            if(cancellation!=null)cancellation.Cancel();
            for(int i=0;i<4;i++)if(registered[i])Native.UnregisterHotKey(Handle,100+i);
            Native.UnregisterHotKey(Handle,200);base.OnHandleDestroyed(e);
        }
        protected override void WndProc(ref Message m) {
            if(m.Msg==0x0312) {
                int id=m.WParam.ToInt32();
                if(id==200){if(cancellation!=null)cancellation.Cancel();return;}
                if(id>=100 && id<104){if(!running) StartOption(id-100);return;}
            }
            base.WndProc(ref m);
        }
        void SetState(string text, bool error=false) {state.Text=text;state.ForeColor=error?Color.Firebrick:SystemColors.ControlText;}
        void UI(Action update) {if(!IsDisposed && IsHandleCreated)try{BeginInvoke(update);}catch(InvalidOperationException){}}
        void Guard(IntPtr target, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            if(Native.Held(27))throw new OperationCanceledException();
            if(Native.GetForegroundWindow()!=target)throw new InvalidOperationException("Stopped: the active window changed. Start again from a clean first CRN box.");
            if(Native.Held(16)||Native.Held(17)||Native.Held(18)||Native.Held(0x5b)||Native.Held(0x5c))throw new InvalidOperationException("Stopped: release modifier keys before starting.");
        }
        async void StartOption(int option) {
            IntPtr target=Native.GetForegroundWindow();
            if(target==Handle){SetState("Select the first CRN box in your browser, then press F"+(option+1)+".",true);return;}
            string[] values=boxes[option].Select(b=>b.Text.Trim()).Where(v=>v.Length>0).ToArray();
            bool fast=speed.SelectedIndex==1;
            List<Step> plan;
            try{plan=Sequence.Build(values,fast);}catch(ArgumentException ex){SetState("Option "+(option+1)+": "+ex.Message,true);return;}
            if(!Native.RegisterHotKey(Handle,200,0x4000,27)){SetState("Cannot reserve Escape for cancellation. Close other hotkey apps and retry.",true);return;}
            running=true;cancellation=new CancellationTokenSource();var token=cancellation.Token;
            foreach(var column in boxes)foreach(var box in column)box.Enabled=false;
            speed.Enabled=false;SetState("Option "+(option+1)+" running — Escape cancels.");
            try {
                await Task.Run(async ()=>{
                    int waited=0;
                    while(Native.Held(0x70+option)){Guard(target,token);if(waited>=3000)throw new InvalidOperationException("Stopped: release the function key after pressing it.");await Task.Delay(25,token);waited+=25;}
                    await Task.Delay(fast?1:60,token);int typed=0;
                    foreach(var step in plan){
                        Guard(target,token);
                        if(step.Kind=="wait"){
                            int left=step.Milliseconds;while(left>0){int amount=Math.Min(25,left);await Task.Delay(amount,token);Guard(target,token);left-=amount;}
                        }else{
                            if(step.Kind=="text"){typed++;int current=typed;UI(()=>SetState("Option "+(option+1)+": typing CRN "+current+" of "+values.Length+". Escape cancels."));}
                            Native.Send(step.Kind,step.Text);
                        }
                    }
                },token);
                SetState("Done — "+values.Length+" CRN(s) typed. Add to Summary activated.\nReview the browser's result.");
            }catch(OperationCanceledException){SetState("Cancelled. Some text may already be entered.\nClear the form before restarting.",true);}
            catch(Exception ex){SetState(ex.Message,true);}
            finally{
                if(IsHandleCreated)Native.UnregisterHotKey(Handle,200);running=false;
                foreach(var column in boxes)foreach(var box in column)box.Enabled=true;
                speed.Enabled=true;cancellation.Dispose();cancellation=null;
            }
        }
        [STAThread] public static void Main() {Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Typer());}
    }
}



