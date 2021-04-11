using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkylightApp
{
    public class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip ctxMenu;
        private ToolStripMenuItem ctxMenuExitItem;

        public MainForm()
        {
            Load += new EventHandler(MainForm_Load);
            Shown += new EventHandler(MainForm_Shown);

            ctxMenuExitItem = new ToolStripMenuItem("Exit");
            ctxMenuExitItem.Click += new EventHandler(ctxMenuExitItem_Click);

            ctxMenu = new ContextMenuStrip();
            ctxMenu.Items.Add(ctxMenuExitItem);

            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Skylight";
            notifyIcon.ContextMenuStrip = ctxMenu;
            notifyIcon.Icon = new Icon("TrayIcon.ico");
            notifyIcon.Visible = true;
        }

        private void ctxMenuExitItem_Click(Object sender, EventArgs e)
        {
            notifyIcon.Icon = null;
            Close();
        }

        private void MainForm_Load(Object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            Opacity = 0;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
