// import React from 'react';
// import { motion } from 'framer-motion';

// export interface HashBucket {
//   key: string;
//   value: number;
//   hash?: number;
// }

// interface HashTableVisualizationProps {
//   buckets: (HashBucket | null)[];
//   size: number;
//   currentBucket?: number;
//   highlightedBuckets?: number[];
//   collisionBuckets?: number[];
// }

// export function HashTableVisualization({
//   buckets = [],
//   size = 10,
//   currentBucket,
//   highlightedBuckets = [],
//   collisionBuckets = []
// }: HashTableVisualizationProps) {
//   const bucketWidth = 100;
//   const bucketHeight = 60;
//   const startX = 150;
//   const startY = 50;
//   const spacing = 10;

//   const getBucketColor = (index: number, bucket: HashBucket | null) => {
//     if (!bucket) return 'rgb(31, 41, 55)';
//     if (currentBucket === index) return 'rgb(239, 68, 68)';
//     if (highlightedBuckets.includes(index)) return 'rgb(34, 197, 94)';
//     if (collisionBuckets.includes(index)) return 'rgb(251, 146, 60)';
//     return 'rgb(59, 130, 246)';
//   };

//   return (
//     <div className="w-full bg-card rounded-lg border p-6 overflow-y-auto max-h-[600px]">
//       <div className="mb-4 text-sm text-muted-foreground">
//         Hash Table (Size: {size}) - Simple Modulo Hash Function
//       </div>
//       <svg width="500" height={size * (bucketHeight + spacing) + 50}>
//         {buckets.slice(0, size).map((bucket, index) => {
//           const yPos = startY + index * (bucketHeight + spacing);
//           const isEmpty = !bucket;
          
//           return (
//             <motion.g
//               key={index}
//               initial={{ x: -50, opacity: 0 }}
//               animate={{ x: 0, opacity: 1 }}
//               transition={{ duration: 0.3, delay: index * 0.02 }}
//             >
//               {/* Index label */}
//               <text
//                 x={startX - 30}
//                 y={yPos + bucketHeight / 2 + 5}
//                 textAnchor="end"
//                 className="fill-muted-foreground select-none"
//               >
//                 [{index}]
//               </text>

//               {/* Bucket */}
//               <motion.rect
//                 x={startX}
//                 y={yPos}
//                 width={bucketWidth}
//                 height={bucketHeight}
//                 fill={getBucketColor(index, bucket)}
//                 stroke="rgb(100, 116, 139)"
//                 strokeWidth="2"
//                 rx="6"
//                 animate={{
//                   scale: currentBucket === index ? [1, 1.05, 1] : 1,
//                 }}
//                 transition={{
//                   duration: 0.6,
//                   repeat: currentBucket === index ? Infinity : 0,
//                 }}
//               />

//               {/* Content */}
//               {!isEmpty ? (
//                 <>
//                   <text
//                     x={startX + bucketWidth / 2}
//                     y={yPos + bucketHeight / 2 - 5}
//                     textAnchor="middle"
//                     className="fill-white text-sm select-none"
//                   >
//                     Key: {bucket.key}
//                   </text>
//                   <text
//                     x={startX + bucketWidth / 2}
//                     y={yPos + bucketHeight / 2 + 15}
//                     textAnchor="middle"
//                     className="fill-white select-none"
//                   >
//                     {bucket.value}
//                   </text>
//                 </>
//               ) : (
//                 <text
//                   x={startX + bucketWidth / 2}
//                   y={yPos + bucketHeight / 2 + 5}
//                   textAnchor="middle"
//                   className="fill-muted-foreground select-none"
//                 >
//                   Empty
//                 </text>
//               )}

//               {/* Hash indicator for current bucket */}
//               {bucket && typeof bucket.hash === 'number' && (
//                 <text
//                   x={startX + bucketWidth + 15}
//                   y={yPos + bucketHeight / 2 + 5}
//                   className="fill-muted-foreground text-xs select-none"
//                 >
//                   hash: {bucket.hash}
//                 </text>
//               )}

//               {/* Collision indicator */}
//               {collisionBuckets.includes(index) && (
//                 <motion.text
//                   x={startX + bucketWidth + 15}
//                   y={yPos + bucketHeight / 2 + 20}
//                   className="fill-orange-500 text-xs select-none"
//                   animate={{ opacity: [1, 0.5, 1] }}
//                   transition={{ duration: 1, repeat: Infinity }}
//                 >
//                   Collision!
//                 </motion.text>
//               )}
//             </motion.g>
//           );
//         })}
//       </svg>
//     </div>
//   );
// }
