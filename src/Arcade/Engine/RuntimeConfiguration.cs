using System;
using System.Collections.Generic;
using Arcade.Run.Aspects;

namespace Arcade.Engine
{
    public class RuntimeConfiguration
    {
        private readonly Dictionary<Type, Lazy<object>> _typeDefinitions;
        private readonly List<IAspect> _aspects; 

        public RuntimeConfiguration()
        {
            _typeDefinitions = new Dictionary<Type, Lazy<object>>();
            _aspects = new List<IAspect>();
        }

        public InterfaceToTypeMapper<T> For<T>()
        {
            return new InterfaceToTypeMapper<T>(this);
        }

        public void AddTypeDefinition(Type type, Func<object> create)
        {
            if(!_typeDefinitions.ContainsKey(type))
                _typeDefinitions.Add(type, new Lazy<object>(create));
        }

        public void AddAspect(IAspect aspect)
        {
            if(!_aspects.Contains(aspect))
                _aspects.Add(aspect);
        }

        internal Dictionary<Type, Lazy<object>> TypeDefinitions
        {
            get { return _typeDefinitions; }
        }

        internal IEnumerable<IAspect> GetAspects()
        {
            return _aspects;
        }
    }

    public class InterfaceToTypeMapper<T>
    {
        private readonly RuntimeConfiguration _runtimeConfiguration;

        public InterfaceToTypeMapper(RuntimeConfiguration runtimeConfiguration)
        {
            _runtimeConfiguration = runtimeConfiguration;
        }

        public RuntimeConfiguration Use(Func<object> create)
        {
            _runtimeConfiguration.AddTypeDefinition(typeof(T), create);
            return _runtimeConfiguration;
        }
    }
}