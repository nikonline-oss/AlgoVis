using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.Visualization;

namespace testing.Models.DataStructures
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
