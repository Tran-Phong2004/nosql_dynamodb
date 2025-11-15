using Amazon.DynamoDBv2.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Dùng để ánh xạ dữ liệu giữa class trong c# sang định dạng của DynamoDB (Dictionary<string, AttributeValue>) và ngược lại
public static class DynamoDbMapper
{
    // ------------------------
    // Dictionary -> Object
    // ------------------------
    public static T ToObject<T>(Dictionary<string, AttributeValue> item) where T : new()
    {
        if (item == null || item.Count == 0)
            return default!;

        T obj = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!item.ContainsKey(prop.Name))
                continue;

            var attrValue = item[prop.Name];

            // Primitive types
            if (attrValue.S != null && prop.PropertyType == typeof(string))
                prop.SetValue(obj, attrValue.S);
            else if (attrValue.N != null)
                prop.SetValue(obj, Convert.ChangeType(attrValue.N, prop.PropertyType));
            else if (attrValue.BOOL != null && prop.PropertyType == typeof(bool))
                prop.SetValue(obj, attrValue.BOOL);

            // List<string> or List<object>
            else if (attrValue.L != null && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {
                var elemType = prop.PropertyType.IsGenericType
                    ? prop.PropertyType.GenericTypeArguments[0]
                    : typeof(object);

                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType))!;

                foreach (var av in attrValue.L)
                {
                    if (av.M != null)
                    {
                        var objItem = typeof(DynamoDbMapper)
                            .GetMethod("ToObject")!
                            .MakeGenericMethod(elemType)
                            .Invoke(null, new object[] { av.M });
                        list.Add(objItem);
                    }
                    else if (av.S != null) list.Add(av.S);
                    else if (av.N != null) list.Add(Convert.ChangeType(av.N, elemType));
                    else if (av.BOOL != null) list.Add(av.BOOL);
                }

                prop.SetValue(obj, list);
            }

            // Nested object (Map)
            else if (attrValue.M != null)
            {
                var nestedObj = typeof(DynamoDbMapper)
                    .GetMethod("ToObject")!
                    .MakeGenericMethod(prop.PropertyType)
                    .Invoke(null, new object[] { attrValue.M });
                prop.SetValue(obj, nestedObj);
            }
        }

        return obj;
    }

    // ------------------------
    // Object -> Dictionary
    // ------------------------
    public static Dictionary<string, AttributeValue> ToAttributeMap<T>(T obj)
    {
        if (obj == null) return new Dictionary<string, AttributeValue>();

        var map = new Dictionary<string, AttributeValue>();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var val = prop.GetValue(obj);
            if (val == null) continue;

            switch (val)
            {
                case string s:
                    map[prop.Name] = new AttributeValue { S = s };
                    break;
                case int i:
                case long l:
                case double d:
                case float f:
                    map[prop.Name] = new AttributeValue { N = val.ToString() };
                    break;
                case bool b:
                    map[prop.Name] = new AttributeValue { BOOL = b };
                    break;
                case IEnumerable list:
                    var attrList = new List<AttributeValue>();
                    foreach (var item in list)
                    {
                        if (item == null) continue;
                        var type = item.GetType();
                        if (type == typeof(string)) attrList.Add(new AttributeValue { S = (string)item });
                        else if (type.IsPrimitive) attrList.Add(new AttributeValue { N = item.ToString() });
                        else
                        {
                            var nestedMap = ToAttributeMap(item);
                            attrList.Add(new AttributeValue { M = nestedMap });
                        }
                    }
                    map[prop.Name] = new AttributeValue { L = attrList };
                    break;
                default:
                    // Nested object
                    var nested = ToAttributeMap(val);
                    map[prop.Name] = new AttributeValue { M = nested };
                    break;
            }
        }

        return map;
    }
}
