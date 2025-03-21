﻿using System.Collections.Generic;
using System.Xml;
using MVZ2.IO;

namespace MVZ2.Metas
{
    public class StatCategoryMeta
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public StatCategoryType Type { get; private set; }
        public StatOperation Operation { get; private set; }
        public static StatCategoryMeta FromXmlNode(XmlNode node, string defaultNsp)
        {
            var id = node.GetAttribute("id");
            var name = node.GetAttribute("name");
            var typeStr = node.GetAttribute("type");
            StatCategoryType type = StatCategoryType.Entity;
            if (!string.IsNullOrEmpty(typeStr) && typeDict.TryGetValue(typeStr, out var value))
            {
                type = value;
            }
            var operationStr = node.GetAttribute("operation");
            StatOperation operation = StatOperation.Sum;
            if (!string.IsNullOrEmpty(operationStr) && operationDict.TryGetValue(operationStr, out var opValue))
            {
                operation = opValue;
            }
            return new StatCategoryMeta()
            {
                ID = id,
                Name = name,
                Type = type,
                Operation = operation
            };
        }
        private static readonly Dictionary<string, StatCategoryType> typeDict = new Dictionary<string, StatCategoryType>()
        {
            { "entity", StatCategoryType.Entity },
            { "stage", StatCategoryType.Stage }
        };
        private static readonly Dictionary<string, StatOperation> operationDict = new Dictionary<string, StatOperation>()
        {
            { "add", StatOperation.Sum },
            { "max", StatOperation.Max }
        };
    }
    public enum StatCategoryType
    {
        Entity,
        Stage
    }
    public enum StatOperation
    {
        Sum,
        Max
    }
}
