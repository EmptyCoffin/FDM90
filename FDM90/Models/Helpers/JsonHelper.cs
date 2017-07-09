using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FDM90.Models.Helpers
{
    public static class JsonHelper
    {
        public static JObject AddWeekValue(JObject currentObject, string propertyName, DateTime date, int value)
        {
            DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dateInfo.Calendar;
            int weekNumber = calendar.GetWeekOfYear(date, dateInfo.CalendarWeekRule, dateInfo.FirstDayOfWeek);

            JObject week = new JObject();
            // add to object / update object 
            JToken weekExisting;

            if (!currentObject.TryGetValue("Week" + weekNumber.ToString(), out weekExisting))
            {
                currentObject.Add("Week" + weekNumber, week);
            }

            JToken existingValue;
            if (((JObject)currentObject.GetValue("Week" + weekNumber)).TryGetValue(propertyName, out existingValue))
            {
                ((JObject)currentObject.GetValue("Week" + weekNumber)).GetValue(propertyName).Replace(int.Parse(existingValue.ToString()) + value);
            }
            else
            {
                ((JObject)currentObject.GetValue("Week" + weekNumber)).Add(propertyName, value);
            }

            return currentObject;
        }

        public static T Parse<T>(dynamic dynamicData, T facebookData)
        {
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