using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using testing.models;
using testing.models.enums;

namespace testing.Interfaces
{
    internal interface IVisualizationAlgorithm
    {
        string Name { get; }
        StructureType _structureType { get; }
        AlgorithmResult Execute(List<SortingStep> _steps, AlgorithmConfig config);
    }
}
