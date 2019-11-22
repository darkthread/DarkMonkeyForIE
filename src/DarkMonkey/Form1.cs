using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DarkMonkey
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            IEScriptInjector.MonitorIE();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            LoadScriptlets();
            IEScriptInjector.DisplayStatus = (m) =>
            {
                txtMessage.Text = $"{DateTime.Now:HH:mm:ss} {m}";
            };
            IEScriptInjector.OnIEActive = (active) =>
            {
                this.Opacity = active ? 1 : 0.75;
            };
            IEScriptInjector.OnScriptInject = (urlMatch) =>
            {
                var found = panScripts.Controls.OfType<FlowLayoutPanel>()
                    .SingleOrDefault(o => o.Tag.ToString() == urlMatch);
                if (found != null)
                {
                    found.BackColor = Color.Yellow;
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1500);
                        this.Invoke(new Action(() =>
                        {
                            found.BackColor = Color.Transparent;
                        }));
                    });
                }
            };
        }

        void LoadScriptlets()
        {

            IEScriptInjector.Scriptlets =
                Directory.GetFiles(".", "*.js")
                .Select(f => new Scriptlet(f)).ToList();
            panScripts.FlowDirection = FlowDirection.TopDown;
            panScripts.Padding = new Padding(0);
            panScripts.Controls.Clear();
            IEScriptInjector.Scriptlets.ToList()
                .ForEach(o =>
                {
                    var row = new FlowLayoutPanel() { 
                        Tag = o.UrlMatch, 
                        FlowDirection = FlowDirection.LeftToRight, 
                        WrapContents = false,
                        AutoSize = true,
                        Margin = new Padding(0)
                    };
                    var flag = new CheckBox {
                        Tag = o,
                        Checked = !o.Disabled,
                        AutoSize = true
                    };
                    Action<object, EventArgs> toggle = (sender, e) => { 
                        o.Toggle(o.Disabled ? true : false);
                        LoadScriptlets();
                    };
                    flag.Click += toggle.Invoke;
                    var lblName = new Label { Text = o.Name, AutoSize = true, Padding = new Padding(3) };
                    var lblUrlMatch = new Label { Text = o.UrlMatch, AutoSize = true, Padding = new Padding(3) };
                    if (o.Disabled)
                    {
                        lblName.ForeColor = lblUrlMatch.ForeColor = Color.Gray;
                    }
                    else
                    {
                        lblName.ForeColor = Color.Blue;
                        lblUrlMatch.ForeColor = Color.Purple;
                    }
                    row.Controls.Add(flag);
                    row.Controls.Add(lblName);
                    row.Controls.Add(lblUrlMatch);
                    panScripts.Controls.Add(row);
                });
        }


    }
}
