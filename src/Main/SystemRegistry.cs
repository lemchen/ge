using System;
using System.Collections.Generic;

namespace Ge
{
    public class SystemRegistry
    {
        private Dictionary<Type, GameSystem> _systems = new Dictionary<Type, GameSystem>();

        public T GetSystem<T>() where T : GameSystem
        {
            GameSystem gs;
            if (!_systems.TryGetValue(typeof(T), out gs))
            {
                return null;
            }
            
            return (T)gs;
        }
        
        public void Register<T>(T system) where T : GameSystem
        {
            _systems.Add(typeof(T), system);
        }
    }
}