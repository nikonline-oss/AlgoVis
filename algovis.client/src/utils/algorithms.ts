import { TreeNode } from '../components/TreeVisualization';
import { GraphNode, GraphEdge } from '../components/GraphVisualization';

// Binary Search Tree utilities
export class BSTUtils {
  static insert(root: TreeNode | null, value: number): TreeNode {
    if (!root) {
      return { value };
    }
    
    if (value < root.value) {
      root.left = this.insert(root.left || null, value);
    } else {
      root.right = this.insert(root.right || null, value);
    }
    
    return root;
  }

  static search(root: TreeNode | null, value: number): boolean {
    if (!root) return false;
    if (root.value === value) return true;
    
    if (value < root.value) {
      return this.search(root.left || null, value);
    } else {
      return this.search(root.right || null, value);
    }
  }

  static findMin(root: TreeNode | null): TreeNode | null {
    if (!root) return null;
    while (root.left) {
      root = root.left;
    }
    return root;
  }

  static delete(root: TreeNode | null, value: number): TreeNode | null {
    if (!root) return null;
    
    if (value < root.value) {
      root.left = this.delete(root.left || null, value);
    } else if (value > root.value) {
      root.right = this.delete(root.right || null, value);
    } else {
      // Node to delete found
      if (!root.left && !root.right) {
        return null;
      }
      
      if (!root.left) {
        return root.right || null;
      }
      
      if (!root.right) {
        return root.left || null;
      }
      
      // Node has two children
      const minRight = this.findMin(root.right);
      if (minRight) {
        root.value = minRight.value;
        root.right = this.delete(root.right, minRight.value);
      }
    }
    
    return root;
  }

  static inorderTraversal(root: TreeNode | null, callback: (value: number) => void): void {
    if (!root) return;
    
    this.inorderTraversal(root.left || null, callback);
    callback(root.value);
    this.inorderTraversal(root.right || null, callback);
  }

  static preorderTraversal(root: TreeNode | null, callback: (value: number) => void): void {
    if (!root) return;
    
    callback(root.value);
    this.preorderTraversal(root.left || null, callback);
    this.preorderTraversal(root.right || null, callback);
  }

  static postorderTraversal(root: TreeNode | null, callback: (value: number) => void): void {
    if (!root) return;
    
    this.postorderTraversal(root.left || null, callback);
    this.postorderTraversal(root.right || null, callback);
    callback(root.value);
  }
}

// Graph utilities
export class GraphUtils {
  static createAdjacencyList(nodes: GraphNode[], edges: GraphEdge[]): Map<number, number[]> {
    const graph = new Map<number, number[]>();
    
    nodes.forEach(node => {
      graph.set(node.id, []);
    });
    
    edges.forEach(edge => {
      graph.get(edge.from)?.push(edge.to);
      graph.get(edge.to)?.push(edge.from); // For undirected graphs
    });
    
    return graph;
  }

  static bfs(
    nodes: GraphNode[], 
    edges: GraphEdge[], 
    startNode: number,
    callback: (nodeId: number, neighbors: number[], visited: Set<number>) => void
  ): void {
    const graph = this.createAdjacencyList(nodes, edges);
    const visited = new Set<number>();
    const queue: number[] = [startNode];
    
    while (queue.length > 0) {
      const current = queue.shift()!;
      
      if (visited.has(current)) continue;
      visited.add(current);
      
      const neighbors = graph.get(current) || [];
      callback(current, neighbors, visited);
      
      neighbors.forEach(neighbor => {
        if (!visited.has(neighbor) && !queue.includes(neighbor)) {
          queue.push(neighbor);
        }
      });
    }
  }

  static dfs(
    nodes: GraphNode[], 
    edges: GraphEdge[], 
    startNode: number,
    callback: (nodeId: number, neighbors: number[], visited: Set<number>) => void
  ): void {
    const graph = this.createAdjacencyList(nodes, edges);
    const visited = new Set<number>();
    
    const dfsHelper = (current: number) => {
      visited.add(current);
      
      const neighbors = graph.get(current) || [];
      callback(current, neighbors, visited);
      
      neighbors.forEach(neighbor => {
        if (!visited.has(neighbor)) {
          dfsHelper(neighbor);
        }
      });
    };
    
    dfsHelper(startNode);
  }

  static dijkstra(
    nodes: GraphNode[], 
    edges: GraphEdge[], 
    startNode: number
  ): Map<number, number> {
    const distances = new Map<number, number>();
    const visited = new Set<number>();
    
    // Initialize distances
    nodes.forEach(node => {
      distances.set(node.id, node.id === startNode ? 0 : Infinity);
    });
    
    while (visited.size < nodes.length) {
      // Find unvisited node with minimum distance
      let minDistance = Infinity;
      let minNode = -1;
      
      nodes.forEach(node => {
        if (!visited.has(node.id)) {
          const dist = distances.get(node.id) || Infinity;
          if (dist < minDistance) {
            minDistance = dist;
            minNode = node.id;
          }
        }
      });
      
      if (minNode === -1) break;
      visited.add(minNode);
      
      // Update distances to neighbors
      const currentDistance = distances.get(minNode) || 0;
      edges.forEach(edge => {
        let neighbor = -1;
        let weight = edge.weight || 1;
        
        if (edge.from === minNode) {
          neighbor = edge.to;
        } else if (edge.to === minNode) {
          neighbor = edge.from;
        }
        
        if (neighbor !== -1 && !visited.has(neighbor)) {
          const newDistance = currentDistance + weight;
          const oldDistance = distances.get(neighbor) || Infinity;
          
          if (newDistance < oldDistance) {
            distances.set(neighbor, newDistance);
          }
        }
      });
    }
    
    return distances;
  }
}
