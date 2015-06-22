﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Collections;

namespace VirtualFileSystem.Core
{
    public class Directory: Entry
    {
        private String name;
        private long modifiedTime;

        private ArrayList directory = new ArrayList();

        public Directory(String name)
        {
            this.name = name;
            this.modifiedTime = Utils.getUnixTimeStamp();
        }

        public override String getName()
        {
            return name;
        }

        public override void setName(string name)
        {
            this.name = name;
        }

        public override String getModifiedTime()
        {
            DateTime dateTime = Utils.getDateTime(this.modifiedTime);
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public override String getType()
        {
            return "文件夹";
        }

        public override String getSize()
        {
            return "";
        }

        public override String getContent()
        {
            throw new NotImplementedException();
        }

        public override Entry add(Entry entry)
        {
            this.modifiedTime = Utils.getUnixTimeStamp();

            directory.Add(entry);
            return this;
        }

        public override TreeNode getTreeNode()
        {
            TreeNode node = new TreeNode(this.name);
            node.Tag = this;

            foreach (Entry entry in directory)
                if (entry.getType() == "文件夹")
                {
                    TreeNode newNode = entry.getTreeNode();
                    node.Nodes.Add(newNode);
                }

            return node;
        }

        public override ArrayList getEntries()
        {
            return directory;
        }

        public override ListViewItem getListViewItem()
        {
            ListViewItem item = new ListViewItem(name, 0);
            item.Tag = this;

            ListViewItem.ListViewSubItem[] subItems;

            subItems = new ListViewItem.ListViewSubItem[]
            {
                new ListViewItem.ListViewSubItem(item, getType()),
                new ListViewItem.ListViewSubItem(item, getModifiedTime()),
                new ListViewItem.ListViewSubItem(item, getSize())
            };

            item.SubItems.AddRange(subItems);

            return item;
        }

    }
}
