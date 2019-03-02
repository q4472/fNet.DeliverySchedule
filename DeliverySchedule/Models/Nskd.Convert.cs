using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nskd
{
    public static class Convert
    {
        private static CultureInfo ic = CultureInfo.InvariantCulture;

        public static Object NullToDBNull(Object obj)
        {
            return (obj == null) ? DBNull.Value : obj;
        }
        public static Int32? ToInt32OrNull(Object obj)
        {
            Int32? result = null;
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "System.DBNull":
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        result = System.Convert.ToInt32(obj);
                        break;
                    case "System.Char":
                        break;
                    case "System.String":
                        String s = (String)obj;
                        if (!String.IsNullOrWhiteSpace(s))
                        {
                            s = (new Regex(@"\s")).Replace(s, "");
                            Int32 r;
                            if (Int32.TryParse(s, NumberStyles.Any, ic, out r))
                            {
                                result = r;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
        public static Decimal? ToDecimalOrNull(Object value)
        {
            Decimal? d = null;
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            if (value.GetType() == typeof(String))
            {
                String temp = value as String;
                if (!String.IsNullOrWhiteSpace(temp))
                {
                    // убрать всё лишнее
                    temp = (new Regex(@"[^\d\.\,\+\-eE]")).Replace(temp, "");
                    if (temp.IndexOf('.') < 0)
                    {
                        // точки нет - значит запятая, если она есть, это разделитель дробной части и её надо заменить на точку
                        value = temp.Replace(',', '.');
                    }
                    else
                    {
                        // точка уже есть - значит запятые это разделители груп и их надо убрать
                        value = temp.Replace(",", "");
                    }
                }
            }
            try
            {
                d = System.Convert.ToDecimal(value, ic);
            }
            catch (Exception) { }
            return d;
        }
        public static Double? ToDoubleOrNull(Object obj)
        {
            Double? result = null;
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "System.DBNull":
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        result = System.Convert.ToDouble(obj);
                        break;
                    case "System.Char":
                        break;
                    case "System.String":
                        String s = (String)obj;
                        if (!String.IsNullOrWhiteSpace(s))
                        {
                            s = (new Regex(@"\s")).Replace(s, "");
                            s = (new Regex(@",")).Replace(s, ".");
                            Double d;
                            if (Double.TryParse(s, NumberStyles.Any, ic, out d))
                            {
                                result = d;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
        public static String ToStringOrNull(Object obj)
        {
            String result = null;
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "System.DBNull":
                        break;
                    case "System.DateTime":
                        result = ((DateTime)obj).ToString("yyyy-MM-dd");
                        break;
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                        result = String.Format("{0:n0}", obj);
                        break;
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        result = String.Format("{0:n2}", obj);
                        break;
                    case "System.Char":
                        result = obj.ToString();
                        break;
                    case "System.String":
                        result = (String)obj;
                        break;
                    default:
                        result = obj.ToString();
                        break;
                }
            }
            return result;
        }
        public static DateTime? ToDateTimeOrNull(Object obj)
        {
            DateTime? result = null;
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "System.DBNull":
                        break;
                    case "System.DateTime":
                        result = (DateTime)obj;
                        break;
                    case "System.Byte":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        break;
                    case "System.Char":
                        break;
                    case "System.String":
                        DateTime r;
                        if (DateTime.TryParse((String)obj, out r))
                        {
                            result = r;
                        }
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// Add seconds to base date 
        /// </summary>
        /// <param name="seconds">seconds from base date</param>
        /// <param name="baseDate">base date</param>
        /// <returns></returns>
        public static DateTime? ToDateTimeOrNull(Object seconds, DateTime baseDate)
        {
            DateTime? result = null;
            Double? ss = ToDoubleOrNull(seconds);
            if (ss != null)
            {
                result = (baseDate).AddSeconds((Double)ss);
            }
            return result;
        }
        /*
        public static DataTemplate StringToDataTemplate(String str)
        {
            DataTemplate result = null;
            if (str != null)
            {
                string dataTemplateString = string.Format(@"
                    <DataTemplate 
                            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                        <Grid>
                            {0}
                        </Grid>
                    </DataTemplate>", str);
                result = (DataTemplate)XamlReader.Parse(dataTemplateString);
            }
            return result;
        }
        */
        /*
        public class ValueConverter : IValueConverter
        {
            public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                if (value != null)
                {
                    switch (value.GetType().ToString())
                    {
                        case "System.DBNull":
                            result = null;
                            break;
                        case "System.Byte[]":
                            result = ConvertFromBytes((Byte[])value, targetType, parameter, culture);
                            break;
                        case "System.DateTime":
                            result = ConvertFromDateTime((DateTime)value, targetType, parameter, culture);
                            break;
                        case "System.Decimal":
                            result = ConvertFromDecimal((Decimal)value, targetType, parameter, culture);
                            break;
                        case "System.Double":
                            result = ConvertFromDouble((Double)value, targetType, parameter, culture);
                            break;
                        case "System.Int32":
                            result = ConvertFromInt32((Int32)value, targetType, parameter, culture);
                            break;
                        case "System.Guid":
                            result = ConvertFromGuid((Guid)value, targetType, parameter, culture);
                            break;
                        case "System.String":
                            result = ConvertFromString((String)value, targetType, parameter, culture);
                            break;
                        default:
                            Console.WriteLine("Nskd.Convert.ValueConverter.Convert(): " + value.GetType().ToString());
                            break;
                    }
                }
                return result;
            }
            public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
            {
                return Convert(value, targetType, parameter, culture);
            }
            private Object ConvertFromString(String value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.Object":
                        result = value;
                        break;
                    case "System.String":
                        result = value;
                        break;
                    case "System.Nullable`1[System.DateTime]":
                    case "System.DateTime":
                        DateTime _dateTime;
                        if (DateTime.TryParse(value, out _dateTime))
                        {
                            result = _dateTime;
                        }
                        break;
                    case "System.Nullable`1[System.Double]":
                    case "System.Double":
                        Double _double;
                        if (Double.TryParse(value, out _double))
                        {
                            result = _double;
                        }
                        break;
                    case "System.Nullable`1[System.Int32]":
                    case "System.Int32":
                        Int32 _int32;
                        if (Int32.TryParse(value, out _int32))
                        {
                            result = _int32;
                        }
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromString(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromDateTime(DateTime value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = value.ToString("yyyy-MM-dd");
                        break;
                    case "System.DateTime":
                        result = value;
                        break;
                    case "System.Double":
                        break;
                    case "System.Int32":
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromDateTime(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromDecimal(Decimal value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = value.ToString("n2");
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Double":
                        result = value;
                        break;
                    case "System.Int32":
                        try
                        {
                            result = System.Convert.ToInt32(value);
                        }
                        catch (OverflowException) { }
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromDouble(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromDouble(Double value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = value.ToString("n2");
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Double":
                        result = value;
                        break;
                    case "System.Int32":
                        try
                        {
                            result = System.Convert.ToInt32(value);
                        }
                        catch (OverflowException) { }
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromDouble(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromInt32(Int32 value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = value.ToString();
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Double":
                        result = System.Convert.ToDouble(value);
                        break;
                    case "System.Int32":
                        result = value;
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromInt32(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromGuid(Guid value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = value.ToString();
                        break;
                    case "System.DateTime":
                        break;
                    case "System.Double":
                        break;
                    case "System.Int32":
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromGuid(): " + targetType.ToString());
                        break;
                }
                return result;
            }
            private Object ConvertFromBytes(Byte[] value, Type targetType, Object parameter, CultureInfo culture)
            {
                Object result = null;
                switch (targetType.ToString())
                {
                    case "System.String":
                        result = System.Convert.ToBase64String(value);
                        break;
                    default:
                        Console.WriteLine("Nskd.Convert.ValueConverter.ConvertFromBytes(): " + targetType.ToString());
                        break;
                }
                return result;
            }
        }
         */
        private static Object ToBytes(String value)
        {
            Object r = null;
            if (!String.IsNullOrWhiteSpace(value))
            {
                value = (new Regex(@"\s")).Replace(value, "");
                if (value.Length > 2 && value[0] == '[' && value[value.Length - 1] == ']')
                {
                    value = value.Substring(1, value.Length - 2);
                    String[] ss = value.Split(',');
                    List<Byte> bytes = new List<Byte>();
                    Byte b = 0;
                    for (int i = 0; i < ss.Length && Byte.TryParse(ss[i], out b); i++)
                    {
                        bytes.Add(b);
                    }
                    r = bytes.ToArray();
                }
            }
            return r;
        }
        private static DateTime ToDateTime(Object value)
        {
            DateTime r = DateTime.Now;
            if ((value != null) && (value != DBNull.Value))
            {
                String temp = value as String;
                if (!String.IsNullOrWhiteSpace(temp))
                {
                    // убрать все пробелы
                    temp = (new Regex(@"\s")).Replace(temp, "");

                    Int32 y = r.Year;
                    Int32 m = r.Month;
                    Int32 d = r.Day;

                    Regex re = new Regex(@"[\.,/]");
                    Boolean order = re.IsMatch(temp);
                    if (order)
                    {
                        temp = re.Replace(temp, "-");
                    }
                    temp = (new Regex(@"[^\d-]")).Replace(temp, "");
                    temp = (new Regex(@"^-+|-+$")).Replace(temp, "");
                    String[] ps = temp.Split('-');
                    if (ps.Length > 0)
                    {
                        Int32 p0 = 1;
                        Int32.TryParse(ps[0], out p0);
                        if (ps.Length == 1)
                        { // есть только одна часть
                            d = p0;
                        }
                        else
                        { // ps.Length > 1
                            Int32 p1 = 1;
                            Int32.TryParse(ps[1], out p1);
                            if (ps.Length == 2)
                            {  // есть только две части
                                d = p0;
                                m = p1;
                            }
                            else
                            {  // ps.Length > 2
                                Int32 p2 = 1;
                                Int32.TryParse(ps[2], out p2);
                                if (p0 > 31)
                                {
                                    y = p0;
                                    m = p1;
                                    d = p2;
                                }
                                else
                                {
                                    if (p2 > 31)
                                    {
                                        d = p0;
                                        m = p1;
                                        y = p2;
                                    }
                                    else
                                    {
                                        if (order)
                                        {
                                            d = p0;
                                            m = p1;
                                            y = p2;
                                        }
                                        else
                                        {
                                            y = p0;
                                            m = p1;
                                            d = p2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    y = ((y < 50) ? 2000 + y : ((y < 100) ? 1900 + y : y));
                    value = "" + y.ToString() + "-" + ((m < 10) ? "0" + m.ToString() : m.ToString()) + "-" + ((d < 10) ? "0" + d.ToString() : d.ToString());
                }
                try
                {
                    r = System.Convert.ToDateTime(value, ic);
                }
                catch (Exception) { }
            }
            return r;
        }
        private static String ToDecimalString(String s)
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                // убрать всё лишнее
                s = (new Regex(@"[^\d\.\,\+\-]")).Replace(s, "");
                if (s.IndexOf('.') < 0)
                {
                    // точки нет - значит запятая, если она есть, это разделитель дробной части и её надо заменить на точку
                    s = s.Replace(',', '.');
                }
                else
                {
                    // точка уже есть - значит запятые это разделители груп и их надо убрать
                    s = s.Replace(",", "");
                }
            }
            return s;
        }
        private static Object ToGuid(String s)
        {
            Object r = null;
            s = (new Regex(@"[\{\}]")).Replace(s, "");
            Guid t;
            if(Guid.TryParse(s,out t))
            {
                r = t;
            }
            return r;
        }
        private static String ToInt32String(String s)
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                // убрать всё лишнее
                s = (new Regex(@"[^\d\.\,\+\-eE]")).Replace(s, "");
            }
            return s;
        }
        public static Double ToDouble(Object value)
        {
            Double r = 0;
            if (value.GetType() == typeof(String))
            {
                String temp = value as String;
                if (!String.IsNullOrWhiteSpace(temp))
                {
                    // убрать всё лишнее
                    temp = (new Regex(@"[^\d\.\,\+\-eE]")).Replace(temp, "");
                    if (temp.IndexOf('.') < 0)
                    {
                        // точки нет - значит запятая, если она есть, это разделитель дробной части и её надо заменить на точку
                        value = temp.Replace(',', '.');
                    }
                    else
                    {
                        // точка уже есть - значит запятые это разделители груп и их надо убрать
                        value = temp.Replace(",", "");
                    }
                }
            }
            try
            {
                r = System.Convert.ToDouble(value, ic);
            }
            catch (Exception) { }
            return r;
        }
        /// <summary>
        /// Изменяет тип. 
        /// </summary>
        /// <param name="value">Значение тип которого надо изменить.</param>
        /// <param name="targetType">Тип к которому приводится или конвертируеся значение.</param>
        /// <param name="exactType">Необязательный параметер определяющий можно ли возвращать null для результата размерного типа (value types).</param>
        /// <param name="allowDBNull">Необязательный параметер определяющий нужно ли заменять null на DBNull.Value.</param>
        /// <returns></returns>
        public static Object ChangeType(Object value, Type targetType, Boolean exactType = false, Boolean allowDBNull = true)
        {
            Object r = null;
            Boolean needToChangeType = true;
            // предварительная обработка значения для преобразования если оно имеет тип String
            if (targetType != null && value.GetType() == typeof(String))
            {
                String s = (String)value;
                switch (targetType.ToString())
                {
                    case "System.Byte[]":
                        r = ToBytes(s);
                        needToChangeType = false;
                        break;
                    case "System.Decimal":
                        value = ToDecimalString(s);
                        break;
                    case "System.Guid":
                        r = ToGuid(s);
                        needToChangeType = false;
                        break;
                    case "System.Int32":
                        value = ToInt32String(s);
                        break;
                    case "System.String":
                        if (String.IsNullOrWhiteSpace(s))
                        {
                            r = null;
                            needToChangeType = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (needToChangeType)
            {
                try
                {
                    r = System.Convert.ChangeType(value, targetType, ic);
                }
                catch (Exception) { }
            }

            if (exactType && r == null)
            {
                switch (targetType.ToString())
                {
                    case "System.Byte[]":
                        break;
                    case "System.Decimal":
                        r = 0m;
                        break;
                    case "System.Guid":
                        r = new Guid();
                        break;
                    case "System.Int32":
                        r = 0;
                        break;
                    case "System.String":
                        break;
                    default:
                        break;
                }
            }
            if (allowDBNull && r == null) r = DBNull.Value;
            return r;
        }
        public static String ToString(Object value, String format = null)
        {
            String result = String.Empty;
            if (value != null && value != DBNull.Value)
            {
                try
                {
                    switch (value.GetType().Name)
                    {
                        case "DateTime":
                            result = ((DateTime)value).ToString(format ?? "dd.MM.yy");
                            break;
                        default:
                            result = System.Convert.ToString(value, ic);
                            break;
                    }
                }
                catch (Exception) { }
            }
            return result;
        }
    }
}
