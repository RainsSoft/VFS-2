﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;

using System.Collections;

namespace VirtualFileSystem.Core
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    class BlockGroup
    {
        //块组

        public SuperBlock super_block; //超级块

        private int block_group_index; //块组序号

        private long g_free_blocks_count;
        private long g_free_inodes_count;

        private bool[] block_index;
        private bool[] inode_index;

        private INode[] inodes;
        private Block[] blocks;

        public BlockGroup() { }

        public BlockGroup(int block_group_index)
        {
            this.block_group_index = block_group_index;

            this.super_block = new SuperBlock();

            this.g_free_blocks_count = Config.BLOCKS_PER_GROUP;
            this.g_free_inodes_count = Config.INODES_PER_GROUP;

            this.block_index = new bool[Config.BLOCKS_PER_GROUP];
            this.inode_index = new bool[Config.INODES_PER_GROUP];

            this.blocks = new Block[Config.BLOCKS_PER_GROUP];
            this.inodes = new INode[Config.INODES_PER_GROUP];

            for (int i = 0; i < Config.BLOCKS_PER_GROUP; i++)
                blocks[i] = new Block(this.block_group_index, i);
            for (int i = 0; i < Config.INODES_PER_GROUP; i++)
                inodes[i] = new INode(this.block_group_index, i);
        }

        public bool hasFreeINode()
        {
            if (g_free_inodes_count > 0)
                return true;
            else
                return false;
        }

        public bool hasFreeBlock()
        {
            if (g_free_blocks_count > 0)
                return true;
            else
                return false;
        }

        public INode getFreeInode()
        {
            for (int i = 0; i < Config.INODES_PER_GROUP; i++)
            {
                if (!inode_index[i])
                {
                    //刷新Inode位图
                    updateINodeIndex(i, true);
                    return inodes[i];
                }
            }

            //不会到这一步
            return null;
        }

        //刷新block
        public void updateBlockIndex(int index, bool flag)
        {
            if (flag)
            {
                this.g_free_blocks_count--;
                VFS.BLOCK_GROUPS[0].super_block.s_free_blocks_count--;
            }
            else
            {
                this.g_free_blocks_count++;
                VFS.BLOCK_GROUPS[0].super_block.s_free_blocks_count++;
            }
            this.block_index[index] = flag;
        }

        //刷新Inode
        public void updateINodeIndex(int index, bool flag)
        {
            if (flag)
            {
                this.g_free_inodes_count--;
                VFS.BLOCK_GROUPS[0].super_block.s_free_inodes_count--;
            }
            else
            {
                this.g_free_inodes_count++;
                VFS.BLOCK_GROUPS[0].super_block.s_free_inodes_count++;
            }
            this.inode_index[index] = flag;
        }

        public ArrayList getFreeBlocks()
        {
            ArrayList free_blocks = new ArrayList();
            for (int i = 0; i < Config.BLOCKS_PER_GROUP; i++)
                if (!block_index[i])
                    free_blocks.Add(blocks[i]);

            return free_blocks;
        }

    }
}
