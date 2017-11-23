using System;
using Castle.DynamicProxy;
using Neurotoxin.Godspeed.Core.Attributes;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class ModelInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.IsAutoProperty())
            {
                var property = invocation.Method.GetProperty();
                var propertyType = property.PropertyType;
                var proxy = (BinaryModelBase)invocation.Proxy;

                if (invocation.Method.IsGetMethod())
                {
                    if (proxy.CacheEnabled && proxy.Cache.ContainsKey(property.Name))
                    {
                        invocation.ReturnValue = proxy.Cache[property.Name];
                        return;
                    }

                    var attribute = property.GetAttribute<BinaryDataAttribute>();
                    object value;

                    if (propertyType.IsArray && BinaryModelBase.BaseType.IsAssignableFrom(propertyType.GetElementType()))
                    {
                        var offset = proxy.GetPropertyOffset(property.Name);
                        var elementType = propertyType.GetElementType();
                        var n = attribute.Length.Value;
                        var array = Array.CreateInstance(elementType, n);
                        var length = 0;
                        for (var i = 0; i < n; i++)
                        {
                            var submodel = (BinaryModelBase)ModelFactory.GetModel(elementType, proxy.Binary, offset + length);
                            length += submodel.OffsetTableSize;
                            array.SetValue(submodel, i);
                        }
                        value = array;
                    }
                    else if (BinaryModelBase.BaseType.IsAssignableFrom(propertyType))
                    {
                        var offset = proxy.GetPropertyOffset(property.Name);
                        value = ModelFactory.GetModel(propertyType, proxy.Binary, offset);
                    }
                    else
                    {
                        value = proxy.ReadPropertyValue(property.Name);
                    }

                    invocation.ReturnValue = value;
                    if (proxy.CacheEnabled) proxy.Cache.Add(property.Name, value);
                } 
                else
                {
                    proxy.WritePropertyValue(property.Name, invocation.Arguments[0]);
                    if (proxy.CacheEnabled && proxy.Cache.ContainsKey(property.Name))
                    {
                        proxy.Cache.Remove(property.Name);
                    }
                }
            }
            else
            {
                invocation.Proceed();
            }

        }
    }
}