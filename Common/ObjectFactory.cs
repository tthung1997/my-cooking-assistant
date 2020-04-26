using System;
using System.Collections.Generic;
using System.Data;
using System.Net.NetworkInformation;
using System.Text;

namespace Common
{
    public class ObjectFactory
    {
        private static IDictionary<Guid, IDictionary<Type, object>> objectDictionary = new Dictionary<Guid, IDictionary<Type, object>>();
        private static Guid? currentDomain = null;

        public static void Initialize(Guid domainId, bool force = false) 
        {
            currentDomain = domainId;
            if (!objectDictionary.ContainsKey(domainId) || force)
            {
                objectDictionary[domainId] = new Dictionary<Type, object>();
            }
        }

        public static void Reset()
        {
            currentDomain = null;
            objectDictionary = new Dictionary<Guid, IDictionary<Type, object>>();
        }

        public static void RegisterInstance<TInterface>(TInterface instance)
        {
            if (currentDomain == null || !objectDictionary.ContainsKey((Guid) currentDomain) || objectDictionary[(Guid) currentDomain] == null)
            {
                throw new InvalidOperationException("Please execute Initialize first");
            }
            objectDictionary[(Guid) currentDomain][typeof(TInterface)] = instance;
        }

        public static void RegisterInstance<TInterface>(Guid domainId, TInterface instance)
        {
            if (!objectDictionary.ContainsKey(domainId) || objectDictionary[domainId] == null)
            {
                throw new InvalidOperationException("Please execute Initialize with the specified domain first");
            }
            objectDictionary[domainId][typeof(TInterface)] = instance;
        }

        public static TInterface GetInstance<TInterface>()
        {
            if (currentDomain == null || !objectDictionary.ContainsKey((Guid)currentDomain) || objectDictionary[(Guid) currentDomain] == null)
            {
                throw new InvalidOperationException("Please execute Initialize first");
            }
            return (TInterface) objectDictionary[(Guid)currentDomain][typeof(TInterface)];
        }

        public static TInterface GetInstance<TInterface>(Guid domainId)
        {
            if (!objectDictionary.ContainsKey(domainId) || objectDictionary[domainId] == null)
            {
                throw new InvalidOperationException("Please execute Initialize with the specified domain first");
            }
            return (TInterface) objectDictionary[domainId][typeof(TInterface)];
        }
    }
}
