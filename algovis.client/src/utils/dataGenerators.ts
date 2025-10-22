import { TreeNode } from '../components/TreeVisualization';
import { GraphNode, GraphEdge } from '../components/GraphVisualization';

export class DataGenerators {
  // Array generators
  static randomArray(size: number, min: number = 1, max: number = 100): number[] {
    return Array.from({ length: size }, () => 
      Math.floor(Math.random() * (max - min + 1)) + min
    );
  }

  static sortedArray(size: number): number[] {
    return Array.from({ length: size }, (_, i) => i + 1);
  }

  static reverseSortedArray(size: number): number[] {
    return Array.from({ length: size }, (_, i) => size - i);
  }

  static nearlySortedArray(size: number): number[] {
    const arr = this.sortedArray(size);
    // Swap a few random elements
    const swaps = Math.floor(size * 0.1);
    for (let i = 0; i < swaps; i++) {
      const idx1 = Math.floor(Math.random() * size);
      const idx2 = Math.floor(Math.random() * size);
      [arr[idx1], arr[idx2]] = [arr[idx2], arr[idx1]];
    }
    return arr;
  }

  // Tree generators
  static randomBST(nodeCount: number): TreeNode | null {
    const values = this.randomArray(nodeCount, 1, 100);
    const uniqueValues = [...new Set(values)]; // Remove duplicates
    
    let root: TreeNode | null = null;
    
    const insert = (node: TreeNode | null, value: number): TreeNode => {
      if (!node) {
        return { value };
      }
      if (value < node.value) {
        node.left = insert(node.left || null, value);
      } else if (value > node.value) {
        node.right = insert(node.right || null, value);
      }
      return node;
    };
    
    uniqueValues.forEach(value => {
      root = insert(root, value);
    });
    
    return root;
  }

  static balancedBST(values: number[]): TreeNode | null {
    if (values.length === 0) return null;
    
    const sortedValues = [...values].sort((a, b) => a - b);
    
    const buildBalanced = (start: number, end: number): TreeNode | null => {
      if (start > end) return null;
      
      const mid = Math.floor((start + end) / 2);
      const node: TreeNode = {
        value: sortedValues[mid],
      };
      
      node.left = buildBalanced(start, mid - 1) || undefined;
      node.right = buildBalanced(mid + 1, end) || undefined;
      
      return node;
    };
    
    return buildBalanced(0, sortedValues.length - 1);
  }

  // Graph generators
  static randomGraph(nodeCount: number, edgeProbability: number = 0.3): { nodes: GraphNode[], edges: GraphEdge[] } {
    const nodes: GraphNode[] = [];
    const edges: GraphEdge[] = [];
    
    // Create nodes in a circle
    const centerX = 400;
    const centerY = 250;
    const radius = 150;
    
    for (let i = 0; i < nodeCount; i++) {
      const angle = (i * 2 * Math.PI) / nodeCount;
      nodes.push({
        id: i,
        x: centerX + radius * Math.cos(angle),
        y: centerY + radius * Math.sin(angle),
      });
    }
    
    // Create random edges
    const addedEdges = new Set<string>();
    
    for (let i = 0; i < nodeCount; i++) {
      for (let j = i + 1; j < nodeCount; j++) {
        if (Math.random() < edgeProbability) {
          const edgeKey = `${i}-${j}`;
          if (!addedEdges.has(edgeKey)) {
            addedEdges.add(edgeKey);
            edges.push({
              from: i,
              to: j,
              weight: Math.floor(Math.random() * 10) + 1,
            });
          }
        }
      }
    }
    
    // Ensure the graph is connected
    for (let i = 0; i < nodeCount - 1; i++) {
      const edgeKey = `${i}-${i + 1}`;
      if (!addedEdges.has(edgeKey)) {
        const reverseKey = `${i + 1}-${i}`;
        if (!addedEdges.has(reverseKey)) {
          addedEdges.add(edgeKey);
          edges.push({
            from: i,
            to: i + 1,
            weight: Math.floor(Math.random() * 10) + 1,
          });
        }
      }
    }
    
    return { nodes, edges };
  }

  static completeGraph(nodeCount: number): { nodes: GraphNode[], edges: GraphEdge[] } {
    const nodes: GraphNode[] = [];
    const edges: GraphEdge[] = [];
    
    // Create nodes in a circle
    const centerX = 400;
    const centerY = 250;
    const radius = 150;
    
    for (let i = 0; i < nodeCount; i++) {
      const angle = (i * 2 * Math.PI) / nodeCount;
      nodes.push({
        id: i,
        x: centerX + radius * Math.cos(angle),
        y: centerY + radius * Math.sin(angle),
      });
    }
    
    // Create all possible edges
    for (let i = 0; i < nodeCount; i++) {
      for (let j = i + 1; j < nodeCount; j++) {
        edges.push({
          from: i,
          to: j,
          weight: Math.floor(Math.random() * 10) + 1,
        });
      }
    }
    
    return { nodes, edges };
  }

  static gridGraph(rows: number, cols: number): { nodes: GraphNode[], edges: GraphEdge[] } {
    const nodes: GraphNode[] = [];
    const edges: GraphEdge[] = [];
    
    const spacing = 80;
    const startX = 100;
    const startY = 100;
    
    // Create nodes in a grid
    for (let i = 0; i < rows; i++) {
      for (let j = 0; j < cols; j++) {
        const id = i * cols + j;
        nodes.push({
          id,
          x: startX + j * spacing,
          y: startY + i * spacing,
        });
      }
    }
    
    // Create edges between adjacent nodes
    for (let i = 0; i < rows; i++) {
      for (let j = 0; j < cols; j++) {
        const currentId = i * cols + j;
        
        // Right neighbor
        if (j < cols - 1) {
          edges.push({
            from: currentId,
            to: currentId + 1,
            weight: 1,
          });
        }
        
        // Bottom neighbor
        if (i < rows - 1) {
          edges.push({
            from: currentId,
            to: currentId + cols,
            weight: 1,
          });
        }
      }
    }
    
    return { nodes, edges };
  }
}
