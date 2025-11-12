using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class CGMergeJsonResult
{
    public Dictionary<string, object> CommonJson { get; set; }
    public IList<Dictionary<string, object>> RecordArray { get; set; }

    public static CGMergeJsonResult ResultForCommonJson(Dictionary<string, object> commonJson, IList<Dictionary<string, object>> recordArray)
    {
        return new CGMergeJsonResult
        {
            CommonJson = commonJson,
            RecordArray = recordArray
        };
    }
}

public class CGNetworkJsonMerger
{
    public static CGMergeJsonResult MergeCommonFields(IList<Dictionary<string, object>> recordArray)
    {
        var commonJson = new Dictionary<string, object>();

        if (recordArray == null || recordArray.Count <= 1)
        {
            return CGMergeJsonResult.ResultForCommonJson(commonJson, recordArray);
        }

        // 收集所有 item.properties 中的公有 key
        HashSet<string> fieldNames = null;

        foreach (var item in recordArray)
        {
            if (item.TryGetValue("properties", out var properties) && properties is Dictionary<string, object> propertiesDict)
            {
                if (fieldNames == null)
                {
                    // 初始化 fieldNames 为第一个 item 的所有字段
                    fieldNames = new HashSet<string>(propertiesDict.Keys);
                }
                else
                {
                    // 求交集
                    fieldNames.IntersectWith(propertiesDict.Keys);
                }
            }
        }

        if (fieldNames == null || fieldNames.Count == 0)
        {
            // 无公共字段时直接返回
            return CGMergeJsonResult.ResultForCommonJson(commonJson, recordArray);
        }

        var resultRecordArray = new List<Dictionary<string, object>>();

        foreach (var item in recordArray)
        {
            if (item.TryGetValue("properties", out var properties) && properties is Dictionary<string, object> propertiesDict)
            {
                var resultItem = new Dictionary<string, object>(item);
                var resultProperties = new Dictionary<string, object>(propertiesDict);

                foreach (var key in fieldNames)
                {
                    if (resultProperties.TryGetValue(key, out var itemValue))
                    {
                        if (commonJson.TryGetValue(key, out var commonValue))
                        {
                            if (Equals(commonValue, itemValue))
                            {
                                // 若公共值和item值相等，则移除item的该字段
                                resultProperties.Remove(key);
                            }
                            // 若不同，则保留item自己的字段
                        }
                        else
                        {
                            // 将item的值保存到公共json
                            commonJson[key] = itemValue;
                            // 移除item的该字段
                            resultProperties.Remove(key);
                        }
                    }
                }
                resultItem["properties"] = resultProperties;
                resultRecordArray.Add(resultItem);
            }
            else
            {
                // 没有properties，直接加
                resultRecordArray.Add(item);
            }
        }

        return CGMergeJsonResult.ResultForCommonJson(commonJson, resultRecordArray);
    }
}

