using System;
using System.Data;
using System.Globalization;

namespace DeliverySchedule.Models
{
    public class Lib
    {
        public class ТаблицаДанных
        {
            private DataTable dt;
            private DataView dv;
            public ТаблицаДанных(DataTable dt)
            {
                if (dt != null)
                {
                    this.dt = dt;
                    dv = dt.DefaultView;
                }
            }
            public Int32 RowsCount
            {
                get => (dv == null) ? 0 : dv.Count;
            }
            public СтрокаДанных this[Int32 index]
            {
                get
                {
                    СтрокаДанных row = null;
                    if (dv != null && index >= 0 && index < dv.Count)
                    {
                        row = new СтрокаДанных(dv[index]);
                    }
                    return row;
                }
            }
            public String Sort
            {
                set
                {
                    dv.Sort = value;
                }
            }
            public String RowFilter
            {
                set
                {
                    dv.RowFilter = value;
                }
            }
        }
        public class СтрокаДанных
        {
            private DataRowView drv;
            public СтрокаДанных(DataRowView drv) { this.drv = drv; }
            public String this[String fieldName, String format = null]
            {
                get
                {
                    String v = String.Empty;
                    if (!String.IsNullOrWhiteSpace(fieldName) 
                        && drv != null 
                        && drv.DataView.Table.Columns.Contains(fieldName))
                    {
                        v = ConvertToString(drv[fieldName], format);
                    }
                    return v;
                }
            }
        }
        public static String ConvertToString(Object v, String format = null)
        {
            CultureInfo ic = CultureInfo.InvariantCulture;
            String s = String.Empty;
            if (v != null && v != DBNull.Value)
            {
                String tfn = v.GetType().FullName;
                switch (tfn)
                {
                    case "System.Guid":
                        s = ((Guid)v).ToString();
                        break;
                    case "System.Int32":
                        s = ((Int32)v).ToString(ic);
                        break;
                    case "System.Boolean":
                        s = ((Boolean)v).ToString(ic);
                        break;
                    case "System.String":
                        s = (String)v;
                        break;
                    case "System.Decimal":
                        if (String.IsNullOrWhiteSpace(format)) format = "n2";
                        s = ((Decimal)v).ToString(format, ic);
                        break;
                    case "System.DateTime":
                        if (String.IsNullOrWhiteSpace(format)) format = "dd.MM.yy";
                        s = ((DateTime)v).ToString(format, ic);
                        break;
                    default:
                        s = "FNet.Supply.Models.Lib.ConvertToString() result: " + tfn;
                        break;
                }
            }
            return s;
        }
    }
}