﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Support
{
    public class TreeNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Value { get; set; }
        public TreeNode? Left { get; set; }
        public TreeNode? Right { get; set; }
        public TreeNode? Parent { get; set; }
    }
}
