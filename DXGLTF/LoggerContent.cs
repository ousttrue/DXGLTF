using NLog.Windows.Forms;
using NLog;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace DXGLTF
{
    public partial class LoggerContent : DockContent
    {
        public LoggerContent(RichTextBox richTextBox)
        {
            InitializeComponent();

            Controls.Add(richTextBox);
            richTextBox.Dock = DockStyle.Fill;
        }

        private void LoggerContent_LinkClicked(RichTextBoxTarget sender, string linkText, NLog.LogEventInfo logEvent)
        {
            MessageBox.Show(logEvent.Exception.ToString(), 
                "Exception details", MessageBoxButtons.OK);
        }
    }
}
