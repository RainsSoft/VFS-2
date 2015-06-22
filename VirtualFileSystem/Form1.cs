﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.WindowsAPICodePack.Shell;
using System.Runtime.InteropServices;
using System.Collections;

using VirtualFileSystem.Core;

namespace VirtualFileSystem
{

    public partial class Form1 : Form
    {
        private Directory currentDir;

        //发送消息依赖-------------------------------------------------------------
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);
        uint MSG_SHOW = RegisterWindowMessage("TextEditor Closed");
        //发送消息依赖-------------------------------------------------------------

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MSG_SHOW)
            {
                enterDirectory(currentDir);
                //MessageBox.Show("收到！");
            }
            base.WndProc(ref m);
        }


        private void InitialVFS()
        {
            //初始化全局块组
            for (int i = 0; i < Config.GROUPS; i++)
                VFS.BLOCK_GROUPS[i] = new BlockGroup(i);

            //初始化目录树
            VFS.rootDir = new Directory("/");

            Directory bootDir = new Directory("boot");
            Directory etcDir = new Directory("etc");
            Directory libDir = new Directory("lib");
            Directory homeDir = new Directory("home");
            Directory rootDir = new Directory("root");
            Directory tempDir = new Directory("temp");
            VFS.rootDir.add(bootDir);
            VFS.rootDir.add(etcDir);
            VFS.rootDir.add(libDir);
            VFS.rootDir.add(homeDir);
            VFS.rootDir.add(rootDir);
            VFS.rootDir.add(tempDir);

            File file1 = new File("bashrc");
            File file2 = new File("shadowsocks");
            etcDir.add(file1);
            etcDir.add(file2);
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode = VFS.rootDir.getTreeNode();

            treeView1.Nodes.Add(rootNode);
        }

        private void enterDirectory(Directory dir)
        {
            currentDir = dir;
            listView1.Items.Clear();

            ArrayList entries = dir.getEntries();

            foreach (Entry entry in entries)
            {
                listView1.Items.Add(entry.getListViewItem());
            }

            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); //自动修改宽度
        }

        public Form1()
        {
            InitializeComponent();

            InitialVFS();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.currentDir = VFS.rootDir;
            PopulateTreeView();

            enterDirectory(currentDir);
        }

        private void treeView1_NodeMouseClick_1(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            listView1.Items.Clear();

            Directory selectedDir = (Directory)selectedNode.Tag;

            enterDirectory(selectedDir);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            TextEditor textEditor = new TextEditor();
            textEditor.Show();
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {

            if (e.Label == null)
                return;

            if (e.Label == "")
            {
                e.CancelEdit = true;
                MessageBox.Show("必须键入文件名", "重命名");
                return;
            }

            //检查文件重名
            ArrayList entries = currentDir.getEntries();

            foreach (Entry entry in entries)
                if (entry.getName() == e.Label)
                {
                    e.CancelEdit = true;
                    MessageBox.Show("文件名重复", "重命名");
                    return;
                }

            ListViewItem selectedItem = listView1.Items[e.Item];
            Entry selectedEntry = (Entry)selectedItem.Tag;
            selectedEntry.setName(e.Label);

            //刷新listview
            enterDirectory(currentDir);

            //刷新treeview
            if (selectedEntry.getType() == "文件夹")
            {
                Directory selectedDir = (Directory)selectedEntry;
                selectedDir.getLinkedTreeNode().Text = e.Label;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Entry selectedEntry = (Entry)listView1.SelectedItems[0].Tag;

            if (selectedEntry.getType() == "文本文件")
            {
                //打开编辑器
                File file = (File)selectedEntry;
                TextEditor textEditor = new TextEditor(file);
                textEditor.Show();
            }
            else
            {
                //进入目录
                Directory directory = (Directory)selectedEntry;
                enterDirectory(directory);

            }

        }

        //设置快捷键
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
                enterDirectory(currentDir);
            else if (keyData == Keys.F2)
                OnRename();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //刷新
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enterDirectory(currentDir);
        }


        //右键菜单
        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            bool match = false;

            if (e.Button == MouseButtons.Right)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Bounds.Contains(e.Location))
                    {
                        match = true;
                        break;
                    }
                }
                if (match)
                {
                    contextMenuStrip2.Show(Cursor.Position);
                }
                else
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        //新建文件夹
        private void 文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Directory newDir = new Directory("新建文件夹");
            currentDir.add(newDir);

            //刷新listview
            enterDirectory(currentDir);

            //刷新treeview
            currentDir.getLinkedTreeNode().Nodes.Add(newDir.getTreeNode());

            ListViewItem newItem = null;
            foreach (ListViewItem item in listView1.Items)
                if (item.Text == newDir.getName())
                {
                    newItem = item;
                    break;
                }

            if (newItem != null)
                newItem.BeginEdit();
        }

        private void OnNewFile()
        {
            File newFile = new File("新建文件");
            currentDir.add(newFile);

            //刷新listview
            enterDirectory(currentDir);
        }

        //新建文本文件
        private void 文本文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnNewFile();
        }

        private void OnRename()
        {
            ListViewItem selectedItem = (ListViewItem)listView1.SelectedItems[0];
            selectedItem.BeginEdit();
        }

        //重命名
        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnRename();
        }



    }
}
