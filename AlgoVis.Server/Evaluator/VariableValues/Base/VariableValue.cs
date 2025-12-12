using AlgoVis.Evaluator.Evaluator.Interfaces;
using AlgoVis.Evaluator.Evaluator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Evaluator.Evaluator.VariableValues.Base
{
    public abstract class VariableValue : IVariableValue
    {
        public abstract VariableType Type {  get; }

        public abstract object RawValue {  get; }

        public virtual IVariableValue GetProperty(string name)
        {
            throw new InvalidOperationException($"Property '{name}' not supported for type {Type}");
        }

        public virtual void SetProperty(string name, IVariableValue value)
        {
            throw new InvalidOperationException($"Cannot set properties on type {Type}");
        }


        public virtual IVariableValue CallMethod(string methodName, IVariableValue[] args)
        {
            throw new InvalidOperationException($"Method '{methodName}' not supported for type {Type}");
        }

        public virtual bool HasProperty(string name) => false;
        public virtual bool HasMethod(string methodName) => false;

        public abstract bool ToBool();
        public abstract double ToDouble();
        public abstract int ToInt();
        public abstract string ToValueString();
    }
}
