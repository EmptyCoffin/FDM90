﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FDM90.Models.Helpers
{
    public static class JsonHelper
    {
        public static T Parse<T>(dynamic dynamicData, T facebookData)
        {
            //if(facebookData.GetType().Namespace.Contains("Collection") && facebookData.GetType().Namespace.Contains("System"))
            //{
            //    IList listInstance = (IList)Activator.CreateInstance(facebookData.GetType());

            //    foreach (var item in (IList)facebookData)
            //    {
            //        listInstance.Add(Parse(dynamicData, item));
            //    }

            //    return (T)listInstance;
            //}
            //else
            //{
            foreach (PropertyInfo property in facebookData.GetType().GetProperties())
            {
                JsonPropertyAttribute jsonValue = (JsonPropertyAttribute)property.GetCustomAttribute(typeof(JsonPropertyAttribute));
                if (dynamicData.GetType().BaseType.FullName.Contains("Collection"))
                {
                    for (int i = 0; i < dynamicData.Count; i++)
                    {
                        if (dynamicData[i]["name"] == jsonValue.PropertyName)
                        {
                            property.SetValue(facebookData, Parse(dynamicData[i], Activator.CreateInstance(property.PropertyType)));
                        }
                    }
                }
                else if (dynamicData.ContainsKey(jsonValue.PropertyName))
                {
                    if (!property.PropertyType.Namespace.Contains("Collection") && property.PropertyType.Namespace.Contains("System"))
                    {
                        property.SetValue(facebookData, Convert.ChangeType(dynamicData[jsonValue.PropertyName], property.PropertyType));
                    }
                    else if (!property.PropertyType.Namespace.Contains("System"))
                    {
                        property.SetValue(facebookData, Parse(dynamicData[jsonValue.PropertyName], Activator.CreateInstance(property.PropertyType)));
                    }
                    else
                    {
                        property.SetValue(facebookData, ParseList(dynamicData, property, jsonValue));
                    }
                }
                //}
            }

            return facebookData;
        }

        private static IList ParseList(dynamic dynamicData, PropertyInfo property, JsonPropertyAttribute jsonValue)
        {
            IList listInstance = (IList)Activator.CreateInstance(property.PropertyType);

            if (dynamicData[jsonValue.PropertyName][0].GetType().BaseType.FullName.Contains("Collection"))
            {
                for (int i = 0; i < dynamicData[jsonValue.PropertyName].data.Count; i++)
                {
                    listInstance.Add(Parse(dynamicData[jsonValue.PropertyName].data[i], Activator.CreateInstance(property.PropertyType.GetGenericArguments().Single())));
                }
            }
            else
            {
                for (int i = 0; i < dynamicData[jsonValue.PropertyName].Count; i++)
                {
                    listInstance.Add(Parse(dynamicData[jsonValue.PropertyName][i], Activator.CreateInstance(property.PropertyType.GetGenericArguments().Single())));
                }
            }

            return listInstance;
        }
    }
}