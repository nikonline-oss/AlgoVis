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
        void ApplyState(object state);
    }

    // Обобщенный интерфейс для типобезопасной работы
    public interface IDataStructure<T> : IDataStructure
    {
        new T GetState();
        void ApplyState(T state);
        object IDataStructure.GetState() => GetState()!;
        void IDataStructure.ApplyState(object state) => ApplyState((T)state);
    }
}
