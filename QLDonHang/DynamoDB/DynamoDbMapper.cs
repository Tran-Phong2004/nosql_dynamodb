using Amazon.DynamoDBv2.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            // DateTime
            if (attrValue.S != null && prop.PropertyType == typeof(DateTime))
            {
                prop.SetValue(obj, DateTime.Parse(attrValue.S, null, System.Globalization.DateTimeStyles.RoundtripKind));
                continue;
            }

            // Nullable DateTime
            if (attrValue.S != null && prop.PropertyType == typeof(DateTime?))
            {
                prop.SetValue(obj, DateTime.Parse(attrValue.S, null, System.Globalization.DateTimeStyles.RoundtripKind));
                continue;
            }

            // DateTimeOffset
            if (attrValue.S != null && prop.PropertyType == typeof(DateTimeOffset))
            {
                prop.SetValue(obj, DateTimeOffset.Parse(attrValue.S, null, System.Globalization.DateTimeStyles.RoundtripKind));
                continue;
            }

            // Nullable DateTimeOffset
            if (attrValue.S != null && prop.PropertyType == typeof(DateTimeOffset?))
            {
                prop.SetValue(obj, DateTimeOffset.Parse(attrValue.S, null, System.Globalization.DateTimeStyles.RoundtripKind));
                continue;
            }

            // String
            if (attrValue.S != null && prop.PropertyType == typeof(string))
            {
                prop.SetValue(obj, attrValue.S);
                continue;
            }

            // Number types
            if (attrValue.N != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                prop.SetValue(obj, Convert.ChangeType(attrValue.N, underlyingType));
                continue;
            }

            // Boolean
            if (attrValue.BOOL != null && (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?)))
            {
                prop.SetValue(obj, attrValue.BOOL);
                continue;
            }

            // DynamoDB String Set (SS) -> string[]
            if (attrValue.SS != null && attrValue.SS.Count > 0)
            {
                if (prop.PropertyType == typeof(string[]))
                {
                    prop.SetValue(obj, attrValue.SS.ToArray());
                    continue;
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    prop.SetValue(obj, attrValue.SS.ToList());
                    continue;
                }
            }

            // DynamoDB List (L) -> Array or List
            if (attrValue.L != null)
            {
                // Xử lý string[]
                if (prop.PropertyType == typeof(string[]))
                {
                    var stringList = attrValue.L
                        .Where(av => av.S != null)
                        .Select(av => av.S)
                        .ToArray();
                    prop.SetValue(obj, stringList);
                    continue;
                }

                // Xử lý List<string>
                if (prop.PropertyType == typeof(List<string>))
                {
                    var stringList = attrValue.L
                        .Where(av => av.S != null)
                        .Select(av => av.S)
                        .ToList();
                    prop.SetValue(obj, stringList);
                    continue;
                }

                // Xử lý các collection khác
                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    var elemType = prop.PropertyType.IsArray
                        ? prop.PropertyType.GetElementType()!
                        : prop.PropertyType.IsGenericType
                            ? prop.PropertyType.GenericTypeArguments[0]
                            : typeof(object);

                    var listType = typeof(List<>).MakeGenericType(elemType);
                    var list = (IList)Activator.CreateInstance(listType)!;

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
                        else if (av.S != null)
                        {
                            if (elemType == typeof(string))
                                list.Add(av.S);
                            else
                                list.Add(Convert.ChangeType(av.S, elemType));
                        }
                        else if (av.N != null)
                        {
                            list.Add(Convert.ChangeType(av.N, elemType));
                        }
                        else if (av.BOOL != null)
                        {
                            list.Add(av.BOOL);
                        }
                    }

                    // Chuyển List sang Array nếu cần
                    if (prop.PropertyType.IsArray)
                    {
                        var array = Array.CreateInstance(elemType, list.Count);
                        list.CopyTo(array, 0);
                        prop.SetValue(obj, array);
                    }
                    else
                    {
                        prop.SetValue(obj, list);
                    }
                    continue;
                }
            }

            // Nested object (Map)
            if (attrValue.M != null)
            {
                var nestedObj = typeof(DynamoDbMapper)
                    .GetMethod("ToObject")!
                    .MakeGenericMethod(prop.PropertyType)
                    .Invoke(null, new object[] { attrValue.M });
                prop.SetValue(obj, nestedObj);
                continue;
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
            // BỎ QUA computed properties (read-only properties không có setter)
            if (!prop.CanWrite)
                continue;

            // BỎ QUA properties có attribute [DynamoDBIgnore]
            if (prop.GetCustomAttributes(typeof(Amazon.DynamoDBv2.DataModel.DynamoDBIgnoreAttribute), true).Length > 0)
                continue;

            var val = prop.GetValue(obj);
            if (val == null) continue;

            // String
            if (val is string s)
            {
                if (!string.IsNullOrEmpty(s))
                    map[prop.Name] = new AttributeValue { S = s };
                continue;
            }

            // DateTime (non-nullable)
            if (val is DateTime dt)
            {
                map[prop.Name] = new AttributeValue { S = dt.ToUniversalTime().ToString("o") };
                continue;
            }

            // DateTimeOffset (non-nullable)
            if (val is DateTimeOffset dto)
            {
                map[prop.Name] = new AttributeValue { S = dto.ToUniversalTime().ToString("o") };
                continue;
            }

            // Xử lý nullable DateTime và DateTimeOffset
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
            if (underlyingType == typeof(DateTime))
            {
                var nullableDateTime = (DateTime?)val;
                if (nullableDateTime.HasValue)
                {
                    map[prop.Name] = new AttributeValue { S = nullableDateTime.Value.ToUniversalTime().ToString("o") };
                }
                continue;
            }

            if (underlyingType == typeof(DateTimeOffset))
            {
                var nullableDateTimeOffset = (DateTimeOffset?)val;
                if (nullableDateTimeOffset.HasValue)
                {
                    map[prop.Name] = new AttributeValue { S = nullableDateTimeOffset.Value.ToUniversalTime().ToString("o") };
                }
                continue;
            }

            // Number types (bao gồm cả nullable numbers)
            if (underlyingType != null && (underlyingType.IsPrimitive || underlyingType == typeof(decimal)))
            {
                map[prop.Name] = new AttributeValue { N = Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture)! };
                continue;
            }

            if (val is int || val is long || val is double || val is float || val is decimal)
            {
                map[prop.Name] = new AttributeValue { N = Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture)! };
                continue;
            }

            // Boolean
            if (val is bool b)
            {
                map[prop.Name] = new AttributeValue { BOOL = b };
                continue;
            }

            // String Array -> DynamoDB String Set (SS)
            if (val is string[] stringArray)
            {
                if (stringArray.Length > 0)
                {
                    map[prop.Name] = new AttributeValue { SS = stringArray.ToList() };
                }
                continue;
            }

            // List<string> -> DynamoDB String Set (SS)
            if (val is List<string> stringList)
            {
                if (stringList.Count > 0)
                {
                    map[prop.Name] = new AttributeValue { SS = stringList };
                }
                continue;
            }

            // IEnumerable (List, Array, etc.)
            if (val is IEnumerable enumerable && !(val is string))
            {
                var attrList = new List<AttributeValue>();
                foreach (var item in enumerable)
                {
                    if (item == null) continue;

                    var itemType = item.GetType();

                    if (item is string str)
                    {
                        attrList.Add(new AttributeValue { S = str });
                    }
                    else if (itemType.IsPrimitive || itemType == typeof(decimal))
                    {
                        attrList.Add(new AttributeValue { N = item.ToString()! });
                    }
                    else if (item is bool boolItem)
                    {
                        attrList.Add(new AttributeValue { BOOL = boolItem });
                    }
                    else
                    {
                        // Nested object
                        var nestedMap = ToAttributeMap(item);
                        if (nestedMap.Count > 0)
                            attrList.Add(new AttributeValue { M = nestedMap });
                    }
                }

                if (attrList.Count > 0)
                    map[prop.Name] = new AttributeValue { L = attrList };
                continue;
            }

            // Nested object (Map)
            if (!val.GetType().IsPrimitive && val.GetType() != typeof(decimal))
            {
                var nested = ToAttributeMap(val);
                if (nested.Count > 0)
                    map[prop.Name] = new AttributeValue { M = nested };
            }
        }

        return map;
    }
}