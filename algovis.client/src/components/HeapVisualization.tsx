// import React from 'react';
// import { motion } from 'framer-motion';

// export interface HeapNode {
//   value: number;
//   index: number;
//   x?: number;
//   y?: number;
// }

// interface HeapVisualizationProps {
//   nodes: HeapNode[];
//   currentIndex?: number;
//   highlightedIndices?: number[];
//   comparedIndices?: number[];
//   type?: 'max' | 'min';
// }

// export function HeapVisualization({
//   nodes = [],
//   currentIndex,
//   highlightedIndices = [],
//   comparedIndices = [],
//   type = 'max'
// }: HeapVisualizationProps) {
//   const nodeRadius = 25;
//   const levelHeight = 80;
//   const startY = 50;
//   const svgWidth = 800;
//   const svgHeight = 400;

//   // Calculate positions for heap nodes
//   const calculatePosition = (index: number): { x: number; y: number } => {
//     const level = Math.floor(Math.log2(index + 1));
//     const levelStart = Math.pow(2, level) - 1;
//     const indexInLevel = index - levelStart;
//     const nodesInLevel = Math.pow(2, level);
    
//     const levelWidth = svgWidth / (nodesInLevel + 1);
//     const x = levelWidth * (indexInLevel + 1);
//     const y = startY + level * levelHeight;
    
//     return { x, y };
//   };

//   const getNodeColor = (index: number) => {
//     if (currentIndex === index) return 'rgb(239, 68, 68)';
//     if (highlightedIndices.includes(index)) return 'rgb(34, 197, 94)';
//     if (comparedIndices.includes(index)) return 'rgb(59, 130, 246)';
//     return type === 'max' ? 'rgb(147, 51, 234)' : 'rgb(236, 72, 153)';
//   };

//   const getParentIndex = (index: number) => {
//     if (index === 0) return null;
//     return Math.floor((index - 1) / 2);
//   };

//   const renderEdge = (parentIndex: number, childIndex: number) => {
//     const parent = calculatePosition(parentIndex);
//     const child = calculatePosition(childIndex);
    
//     const angle = Math.atan2(child.y - parent.y, child.x - parent.x);
//     const parentX = parent.x + nodeRadius * Math.cos(angle);
//     const parentY = parent.y + nodeRadius * Math.sin(angle);
//     const childX = child.x - nodeRadius * Math.cos(angle);
//     const childY = child.y - nodeRadius * Math.sin(angle);

//     return (
//       <motion.line
//         key={`edge-${parentIndex}-${childIndex}`}
//         x1={parentX}
//         y1={parentY}
//         x2={childX}
//         y2={childY}
//         stroke="rgb(100, 116, 139)"
//         strokeWidth="2"
//         initial={{ pathLength: 0, opacity: 0 }}
//         animate={{ pathLength: 1, opacity: 0.6 }}
//         transition={{ duration: 0.3 }}
//       />
//     );
//   };

//   return (
//     <div className="w-full bg-card rounded-lg border p-6 overflow-hidden">
//       <div className="mb-4 flex items-center justify-between">
//         <div className="text-sm text-muted-foreground">
//           {type === 'max' ? 'Max Heap' : 'Min Heap'} - Array representation: [
//           {nodes.map((n, i) => (
//             <span key={i} className={currentIndex === i ? 'text-red-500 font-bold' : ''}>
//               {n.value}{i < nodes.length - 1 ? ', ' : ''}
//             </span>
//           ))}]
//         </div>
//       </div>
//       <svg width={svgWidth} height={svgHeight}>
//         {/* Draw edges */}
//         {nodes.map((node, index) => {
//           const parentIndex = getParentIndex(index);
//           if (parentIndex !== null) {
//             return renderEdge(parentIndex, index);
//           }
//           return null;
//         })}

//         {/* Draw nodes */}
//         {nodes.map((node, index) => {
//           const pos = calculatePosition(index);
          
//           return (
//             <motion.g
//               key={index}
//               initial={{ scale: 0, opacity: 0 }}
//               animate={{ scale: 1, opacity: 1 }}
//               transition={{ duration: 0.3, delay: index * 0.05 }}
//             >
//               <motion.circle
//                 cx={pos.x}
//                 cy={pos.y}
//                 r={nodeRadius}
//                 fill={getNodeColor(index)}
//                 stroke={currentIndex === index ? 'rgb(239, 68, 68)' : 'rgb(100, 116, 139)'}
//                 strokeWidth={currentIndex === index ? 3 : 2}
//                 animate={{
//                   scale: currentIndex === index ? [1, 1.2, 1] : 1,
//                 }}
//                 transition={{
//                   duration: 0.6,
//                   repeat: currentIndex === index ? Infinity : 0,
//                 }}
//               />
//               <text
//                 x={pos.x}
//                 y={pos.y + 5}
//                 textAnchor="middle"
//                 className="fill-white select-none"
//               >
//                 {node.value}
//               </text>
//               <text
//                 x={pos.x}
//                 y={pos.y - nodeRadius - 8}
//                 textAnchor="middle"
//                 className="fill-muted-foreground text-xs select-none"
//               >
//                 [{index}]
//               </text>
//             </motion.g>
//           );
//         })}

//         {/* Empty heap message */}
//         {nodes.length === 0 && (
//           <text
//             x={svgWidth / 2}
//             y={svgHeight / 2}
//             textAnchor="middle"
//             className="fill-muted-foreground select-none"
//           >
//             Empty Heap
//           </text>
//         )}
//       </svg>
//     </div>
//   );
// }
