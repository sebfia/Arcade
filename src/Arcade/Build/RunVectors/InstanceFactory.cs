using System;
using System.Collections.Generic;
using System.Linq;
using Arcade.Engine;

namespace Arcade.Build.RunVectors
{
    public class InstanceFactory
    {
        private readonly RuntimeConfiguration _runtimeConfiguration;
        private readonly Dictionary<Guid, Func<object>> _customCreators;
        private readonly Dictionary<Type, Func<object>> _typeCreators; 

        public InstanceFactory(RuntimeConfiguration runtimeConfiguration)
        {
            _runtimeConfiguration = runtimeConfiguration;
            _customCreators = new Dictionary<Guid, Func<object>>();
            _typeCreators = new Dictionary<Type, Func<object>>();
        }

        public void AddInstanceCreatorForCorrelationId(Guid correlationId, Func<object> creator)
        {
            if(!_customCreators.ContainsKey(correlationId))
                _customCreators.Add(correlationId, creator);
        }

        public object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();

            var constructor = constructors[0];

            var parameterTypes = constructor.GetParameters().Select(x => x.ParameterType).ToArray();

            var parameters = parameterTypes.Select(x => _runtimeConfiguration.TypeDefinitions[x].Value).ToArray();

            return Activator.CreateInstance(type, parameters);
        }

        public object CreateInstance(Type type, Guid correlationId)
        {
            if (_customCreators.ContainsKey(correlationId))
            {
                return _customCreators[correlationId]();
            }

            return CreateInstance(type);
        }

        public T CreateInstance<T>()
        {
            if (_typeCreators.ContainsKey(typeof (T)))
            {
                return (T) _typeCreators[typeof (T)]();
            }

            return (T) CreateInstance(typeof (T));
        }
    }
}