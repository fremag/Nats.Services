using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nats.Services.Core
{
    public static class TypeHelpers
    {
        public static EventInfo GetEventInfo(this Type type, string eventName)
        {
            return GetAllEventInfos(type).FirstOrDefault(evt => evt.AddMethod.Name == eventName);
        }

        public static IEnumerable<EventInfo> GetAllEventInfos(this Type type)
        {
            foreach (var evtInfo in type.GetEvents())
            {
                yield return evtInfo;
            }
            foreach (var interfaceType in type.GetInterfaces())
            {
                foreach (var evtInfo in interfaceType.GetEvents())
                {
                    yield return evtInfo;
                }
            }
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName)
        {
            return GetAllMethodInfos(type).FirstOrDefault(meth => meth.Name == methodName);
        }

        public static IEnumerable<MethodInfo> GetAllMethodInfos(this Type type)
        {
            foreach (var methInfo in type.GetMethods())
            {
                yield return methInfo;
            }
            foreach (var interfaceType in type.GetInterfaces())
            {
                foreach (var methInfo in interfaceType.GetMethods())
                {
                    yield return methInfo;
                }
            }
        }
    }
}
