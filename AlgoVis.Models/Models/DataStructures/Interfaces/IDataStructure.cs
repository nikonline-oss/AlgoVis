using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoVis.Models.Models.Visualization;

namespace AlgoVis.Models.Models.DataStructures.Interfaces
{
    // Базовый необобщенный интерфейс
    public interface IDataStructure
    {
        string Type { get; }
        string Id { get; }
        VisualizationData ToVisualizationData();
        object GetState();
        object GetOriginState();
        void ApplyState(object state);
    }

    public interface IDataStructure<T> : IDataStructure
    {
        new T GetState();
        new T GetOriginState();
        void ApplyState(T state);
        object IDataStructure.GetState() => GetState()!;
        object IDataStructure.GetOriginState() => GetOriginState()!;
        void IDataStructure.ApplyState(object state) => ApplyState((T)state);
    }
}
