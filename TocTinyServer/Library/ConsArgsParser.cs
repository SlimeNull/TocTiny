using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Null.Library.ConsArgsParser
{
    class ConsArgs
    {
        Dictionary<string, string> stringP = new Dictionary<string, string>();
        List<string> stringL = new List<string>();
        List<string> booleanP = new List<string>();

        public Dictionary<string, string> Properties
        {
            get
            {
                return stringP;
            }
        }
        public List<string> Content
        {
            get
            {
                return stringL;
            }
        }
        public List<string> Booleans
        {
            get
            {
                return booleanP;
            }
        }

        /// <summary>
        /// 分析命令行参数, 以供更简单的操作
        /// </summary>
        /// <param name="arguments">命令行参数源</param>
        /// <param name="stringPropertiySign">作为属性标识的符号</param>
        /// <param name="booleanPropertySign">作为布尔值标识的符号</param>
        /// <param name="autoToUpper">是否对属性的键进行ToUpper处理</param>
        public ConsArgs(string[] arguments, string stringPropertiySign = "-", string booleanPropertySign = "/", bool autoToUpper = true)
        {
            bool key = false;                // 状态: 是否识别到了键
            string tempkey = string.Empty;   // 临时存储的键
            foreach(string i in arguments)
            {
                if (key)    // 如果已经识别到了键, 则代表当前的内容是一个值, 存储它, 并将"key"状态改为false
                {
                    stringP[tempkey] = i;
                    key = false;
                }
                else
                {
                    if (i.StartsWith(booleanPropertySign))        // 以startwith判断当前是否是一个布尔值
                    {
                        booleanP.Add((autoToUpper ? i.ToUpper() : i).Substring(booleanPropertySign.Length));
                    }
                    else if (i.StartsWith(stringPropertiySign))   // 判断当前是否是一个键, 如果是, 则使用tempkey存储下这个键, 并将状态"key"改为true
                    {
                        tempkey = (autoToUpper ? i.ToUpper() : i).Substring(stringPropertiySign.Length);
                        key = true;
                    }
                    else                                          // 不是布尔, 也不是键, 那么就是普通内容
                    {
                        stringL.Add(i);
                    }
                }
            }
        }
        public T ToObject<T>()
        {
            Type resultT = typeof(T);
            T result = Activator.CreateInstance<T>();

            foreach (FieldInfo info in resultT.GetFields())
            {
                if (info.FieldType == typeof(string))
                {
                    if (stringP.ContainsKey(info.Name))
                    {
                        info.SetValue(result, stringP[info.Name]);
                    }
                }
                else if (info.FieldType == typeof(bool))
                {
                    if (booleanP.Contains(info.Name))
                    {
                        info.SetValue(result, true);
                    }
                }
                else
                {
                    if (info.Name == "Content")
                    {
                        info.SetValue(result, this.stringL);
                    }
                    else if (info.Name == "Properties")
                    {
                        info.SetValue(result, this.stringP);
                    }
                    else if (info.Name == "Booleans")
                    {
                        info.SetValue(result, this.booleanP);
                    }
                }
            }

            return result;
        }
    }
}
