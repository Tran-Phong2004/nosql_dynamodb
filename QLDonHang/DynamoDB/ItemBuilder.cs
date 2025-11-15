using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace QLDonHang.DynamoDB
{
    public class ItemBuilder
    {
        private readonly Dictionary<string, AttributeValue> _dict = new();

        private ItemBuilder() { }

        public static ItemBuilder Create() => new ItemBuilder();

        // Add simple value (string, int, double, bool)
        public ItemBuilder Add(string key, string value)
        {
            _dict[key] = new AttributeValue { S = value };
            return this;
        }

        public ItemBuilder Add(string key, int value)
        {
            _dict[key] = new AttributeValue { N = value.ToString() };
            return this;
        }

        public ItemBuilder Add(string key, double value)
        {
            _dict[key] = new AttributeValue { N = value.ToString(CultureInfo.InvariantCulture) };
            return this;
        }

        public ItemBuilder Add(string key, bool value)
        {
            _dict[key] = new AttributeValue { BOOL = value };
            return this;
        }

        // Add nested map
        public ItemBuilder AddMap(string key, Action<ItemBuilder> mapBuilderAction)
        {
            var nestedBuilder = Create();
            mapBuilderAction(nestedBuilder);
            _dict[key] = new AttributeValue { M = nestedBuilder.Build() };
            return this;
        }

        // Add list
        public ItemBuilder AddList(string key, params object[] items)
        {
            var list = items.Select(item =>
            {
                return item switch
                {
                    string s => new AttributeValue { S = s },
                    int i => new AttributeValue { N = i.ToString() },
                    double d => new AttributeValue { N = d.ToString(CultureInfo.InvariantCulture) },
                    bool b => new AttributeValue { BOOL = b },
                    Dictionary<string, AttributeValue> m => new AttributeValue { M = m },
                    AttributeValue av => av,
                    _ => throw new NotSupportedException($"Unsupported type {item.GetType()}")
                };
            }).ToList();

            _dict[key] = new AttributeValue { L = list };
            return this;
        }

        // Build final dictionary
        public Dictionary<string, AttributeValue> Build() => _dict;
    }
}
