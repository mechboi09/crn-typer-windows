using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRNTyper {

    public class LinkedCrnBox : TextBox {
        public Action<string> PasteValues;
        protected override void WndProc(ref Message m){
            if(m.Msg==0x0302 && PasteValues!=null){try{PasteValues(Clipboard.GetText());}catch(Exception ex){MessageBox.Show(ex.Message,"Check CRNs");}return;}
            base.WndProc(ref m);
        }
    }

    public static class CrnLists {


        public static string[] Parse(string text,int max) {
            var values=text.Split(new[]{' ', '\t','\r','\n',','},StringSplitOptions.RemoveEmptyEntries);
            if(values.Length>max||values.Any(v=>v.Length>5||v.Any(c=>c<'0'||c>'9'))||values.Distinct().Count()!=values.Length)throw new ArgumentException("Use up to "+max+" unique CRNs, 1-5 digits each, separated by spaces, commas or new lines.");
            return values;
        }
    }

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
                    result.Add(new Step("wait", "", fast ? 1 : 200));
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

        readonly TabControl tabs=new TabControl();
        readonly TextBox[] names=new TextBox[4];
        readonly Panel[][] rowPanels=new Panel[4][];
        readonly int[] visibleRows=new int[]{3,3,3,3};
        bool loading=true;
        const int Maximum=6;
        void UpdateRows(int option){for(int i=0;i<Maximum;i++)rowPanels[option][i].Visible=i<visibleRows[option];}
        void Paste(int option){
            using(var dialog=new Form{Text="Paste CRN list",ClientSize=new Size(370,245),StartPosition=FormStartPosition.CenterParent,MinimizeBox=false,MaximizeBox=false}){
                var input=new TextBox{Multiline=true,ScrollBars=ScrollBars.Vertical,Location=new Point(15,15),Size=new Size(340,170)};
                var ok=new Button{Text="Use this list",Location=new Point(230,200),Size=new Size(125,30)};
                dialog.Controls.Add(input);dialog.Controls.Add(ok);ok.Click+=(s,e)=>{try{var values=CrnLists.Parse(input.Text,Maximum);loading=true;for(int i=0;i<Maximum;i++)boxes[option][i].Text=i<values.Length?values[i]:"";visibleRows[option]=Maximum;UpdateRows(option);loading=false;dialog.Close();SetState("Ready. List loaded.");}catch(ArgumentException ex){MessageBox.Show(dialog,ex.Message,"Check CRNs");}};dialog.ShowDialog(this);
            }
        }
        void BuildEditor(){
            ClientSize=new Size(410,335);Text="CRN Typer - AUB";
            for(int col=0;col<4;col++){
                int option=col;var group=new GroupBox{Text="Option "+(col+1)+" (F"+(col+1)+")",Location=new Point(7+col*101,6),Size=new Size(98,203)};Controls.Add(group);
                names[col]=new TextBox{Text="Option "+(col+1)};
                boxes[col]=new TextBox[Maximum];rowPanels[col]=new Panel[Maximum];visibleRows[col]=Maximum;
                for(int row=0;row<Maximum;row++){
                    var panel=new Panel{Location=new Point(7,21+row*24),Size=new Size(84,23)};rowPanels[col][row]=panel;group.Controls.Add(panel);
                    int index=row;var box=new LinkedCrnBox{Location=new Point(0,0),Size=new Size(84,22),MaxLength=5,AccessibleName="Option "+(col+1)+", CRN "+(row+1)};boxes[col][row]=box;panel.Controls.Add(box);box.PasteValues=text=>{if(running)return;var values=CrnLists.Parse(text,Maximum-index);loading=true;try{for(int i=index;i<Maximum;i++)boxes[option][i].Text=i-index<values.Length?values[i-index]:"";}finally{loading=false;}};
                }
                var paste=new Button{Text="Paste",Location=new Point(7,24+Maximum*24),Size=new Size(84,24)};group.Controls.Add(paste);paste.Click+=(a,e)=>{if(!running)Paste(option);};
            }
            speed.Items.AddRange(new object[]{"Normal (200 ms)","Fast (1 ms requested)"});speed.SelectedIndex=0;
            speed.DropDownStyle=ComboBoxStyle.DropDownList;speed.Location=new Point(8,218);speed.Size=new Size(394,26);Controls.Add(speed);state.Location=new Point(8,251);state.Size=new Size(394,42);state.Text="Ready. Focus the first CRN box, then F1-F4. Escape cancels.";Controls.Add(state);
            var clear=new Button{Text="Clear all",Location=new Point(8,302),Size=new Size(394,27)};Controls.Add(clear);clear.Click+=(a,e)=>{if(running)return;loading=true;foreach(var column in boxes)foreach(var box in column)box.Text="";loading=false;};

            loading=false;for(int c=0;c<4;c++){visibleRows[c]=Maximum;UpdateRows(c);}
            
        }
        public Typer() {
            Text="CRN Typer"; ClientSize=new Size(428,428); FormBorderStyle=FormBorderStyle.FixedSingle;
            MaximizeBox=false; StartPosition=FormStartPosition.CenterScreen; AutoScaleMode=AutoScaleMode.Dpi;
            Font=new Font("Segoe UI",9); BackColor=SystemColors.Control;
            BuildEditor();
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
            speed.Enabled=false;tabs.Enabled=false;SetState("Option "+(option+1)+" running — Escape cancels.");
            try {
                await Task.Run(async ()=>{
                    int waited=0;
                    while(Native.Held(0x70+option)){Guard(target,token);if(waited>=2000)throw new InvalidOperationException("Stopped: release the function key after pressing it.");await Task.Delay(25,token);waited+=25;}
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
                SetState("Done — "+values.Length+" CRN(s) sent. Keys sent to Add to Summary.\nReview the browser's result.");
            }catch(OperationCanceledException){SetState("Cancelled. Some text may already be entered.\nClear the form before restarting.",true);}
            catch(Exception ex){SetState(ex.Message,true);}
            finally{
                if(IsHandleCreated)Native.UnregisterHotKey(Handle,200);running=false;
                foreach(var column in boxes)foreach(var box in column)box.Enabled=true;
                speed.Enabled=true;tabs.Enabled=true;cancellation.Dispose();cancellation=null;
            }
        }
        [STAThread] public static void Main() {Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Typer());}
    }
}