using Nskd;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DeliverySchedule.Models
{
    public class F3Model
    {
        public Guid SessionId;
        public Int32 SpecId;
        public DataTable Head;
        public DataTable Table;
        public DataTable Shedule;
        public String sSiop;
        public String sDpo;
        public String sSiso;
        public String sDps;
        public String sSizs;
        public String sDozp;
        public СпецификацииТаблицаГрафик График;
        public DataTable Заявки;

        public F3Model(RequestPackage rqp)
        {
            SessionId = rqp.SessionId;
            if (Int32.TryParse(rqp["id"] as String, out SpecId)) { }
            else { Int32.TryParse(rqp["код_спецификации"] as String, out SpecId); }
        }
        public void Load(RequestPackage rqp)
        {
            SpecGet();
        }
        public void Update(RequestPackage rqp)
        {
            rqp.Command = "[dbo].[спецификации_шапка_update_from_not_null_values]";
            rqp.AddSessionIdToParameters();
            rqp.GetResponse("http://127.0.0.1:11012/");

            SpecGet();
        }
        public void Update2(RequestPackage rqp)
        {
            // исправляем сроки исполнения
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (name.Length == 40 && value != null && (new Regex(@"_[0-9a-f-]{36}_s\d")).IsMatch(name))
                {
                    Guid.TryParse(name.Substring(1, 36), out Guid tpId);
                    String fieldName = name.Substring(38, 2);
                    RequestPackage rqp1 = new RequestPackage
                    {
                        SessionId = rqp.SessionId,
                        Command = "dbo.[спецификации_график_параметры_тп_изменить]",
                        Parameters = new RequestParameter[]
                        {
                            new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                            new RequestParameter { Name = "tp_uid", Value = tpId },
                            null // Parameters[2]
                        }
                    };
                    switch (fieldName)
                    {
                        case "s1":
                            rqp1.Parameters[2] = new RequestParameter { Name = "срок_исполнения_заявка_склад", Value = value };
                            break;
                        case "s2":
                            rqp1.Parameters[2] = new RequestParameter { Name = "срок_исполнения_склад_отгрузка", Value = value };
                            break;
                        case "s3":
                            rqp1.Parameters[2] = new RequestParameter { Name = "срок_исполнения_отгрузка_покупатель", Value = value };
                            break;
                        default:
                            break;
                    }
                    rqp1.GetResponse("http://127.0.0.1:11012/");
                }
            }
            // исправляем количество со старой датой
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                if (!String.IsNullOrWhiteSpace(name)
                    && name.Length == 50
                    && (new Regex(@"_[0-9a-f-]{36}_\d\d\.\d\d\.\d\d \d [qe]")).IsMatch(name))
                {
                    Guid.TryParse(name.Substring(1, 36), out Guid tpId);
                    DateTime.TryParse(name.Substring(38, 8), out DateTime date);
                    Int32.TryParse(name.Substring(47, 1), out Int32 ftype);
                    String field = name.Substring(49, 1);
                    String value = p.Value as String;
                    RequestPackage rqp1;
                    if (field == "q")
                    {
                        rqp1 = new RequestPackage
                        {
                            SessionId = rqp.SessionId,
                            Command = "[Pharm-Sib].[dbo].[спецификации_график_изменить_количество]",
                            Parameters = new RequestParameter[]
                            {
                                new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                                new RequestParameter { Name = "код_спецификации", Value = SpecId },
                                new RequestParameter { Name = "дата", Value = date },
                                new RequestParameter { Name = "количество", Value = Nskd.Convert.ToDecimalOrNull(value) },
                                new RequestParameter { Name = "tp_id", Value = tpId },
                                new RequestParameter { Name = "тип_формирования", Value = ftype }
                            }
                        };
                    }
                    else // field == "e"
                    {
                        value = (value.Length == 5) ? $"20{value.Substring(3, 2)}-{value.Substring(0, 2)}-01" : null;
                        rqp1 = new RequestPackage
                        {
                            SessionId = rqp.SessionId,
                            Command = "dbo.[спецификации_график_изменить_срок_годности]",
                            Parameters = new RequestParameter[]
                            {
                                new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                                new RequestParameter { Name = "tp_id", Value = tpId },
                                new RequestParameter { Name = "дата", Value = date },
                                new RequestParameter { Name = "тип_формирования", Value = ftype },
                                new RequestParameter { Name = "срок_годности", Value = value }
                            }
                        };
                    }
                    var rsp = rqp1.GetResponse("http://127.0.0.1:11012/");
                }
            }
            // изменяем дату
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (name.Length == 12 && (new Regex(@"\d\d\.\d\d\.\d\d \d [qe]")).IsMatch(name)
                    && value != null && (new Regex(@"\d\d\.\d\d\.\d\d")).IsMatch(value))
                {
                    DateTime.TryParse(name.Substring(0, 8), out DateTime oldDate);
                    DateTime.TryParse(value, out DateTime newDate);
                    Int32.TryParse(name.Substring(9, 1), out Int32 ftype);
                    RequestPackage rqp1 = new RequestPackage
                    {
                        SessionId = rqp.SessionId,
                        Command = "dbo.[спецификации_график_изменить_дату]",
                        Parameters = new RequestParameter[]
                        {
                            new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                            new RequestParameter { Name = "id", Value = SpecId },
                            new RequestParameter { Name = "old_date", Value = oldDate },
                            new RequestParameter { Name = "new_date", Value = newDate },
                            new RequestParameter { Name = "тип_формирования", Value = ftype }
                        }
                    };
                    rqp1.GetResponse("http://127.0.0.1:11012/");
                }
            }
            // обновить данные для страницы
            SpecGet();
        }
        public void Corr(RequestPackage rqp)
        {
            rqp.AddSessionIdToParameters();
            rqp.Command = "[Pharm-Sib].[dbo].[спецификации_зачёт_журнал_add]";
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11012");
        }
        public void AddColumn(RequestPackage rqp)
        {
            if (rqp == null) { throw new ArgumentException(); }

            String specId = rqp["код_спецификации"] as String;
            if (String.IsNullOrWhiteSpace(specId)
                || !Int32.TryParse(specId, out Int32 temp)) { throw new ArgumentException(); }

            String fType = rqp["тип_формирования"] as String;
            if (String.IsNullOrWhiteSpace(fType)
                || !(fType == "0" || fType == "1" || fType == "2")) { throw new ArgumentException(); }

            rqp.Command = "[Pharm-Sib].[dbo].[спецификации_график__добавить_колонку]";
            rqp.AddSessionIdToParameters();
            var rsp = rqp.GetResponse("http://127.0.0.1:11012/");
        }
        public void Send(RequestPackage rqp)
        {
            if (rqp == null) { throw new ArgumentException(); }
            rqp.Command = "[Pharm-Sib].[dbo].[спецификации_график__передать_в_отдел_снабжения]";
            rqp.AddSessionIdToParameters();
            var rsp = rqp.GetResponse("http://127.0.0.1:11012/");
        }

        private void SpecGet()
        {
            RequestPackage rqp = new RequestPackage()
            {
                Command = "[Pharm-Sib].[dbo].[спецификации_get]",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter() { Name = "session_id", Value = SessionId },
                    new RequestParameter() { Name = "id", Value = SpecId }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11012/");
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                Head = rsp.Data.Tables[0];
                if (rsp.Data.Tables.Count > 1)
                {
                    Table = rsp.Data.Tables[1];
                    if (rsp.Data.Tables.Count > 2)
                    {
                        //Shedule = rsp.Data.Tables[2];
                        График = new СпецификацииТаблицаГрафик(rsp.Data.Tables[2]);
                        Shedule = CreateSheduleTable();
                        if (rsp.Data.Tables.Count > 3)
                        {
                            Заявки = rsp.Data.Tables[3];
                        }
                    }
                }
            }
            if (Head != null && Head.Rows.Count > 0)
            {
                Object o = Head.Rows[0]["дата_первой_поставки"];

                Object oSiop = Head.Rows[0]["срок_исполнения_отгрузка_покупатель"];

                DateTime? dDpo = null;
                if (oSiop != DBNull.Value)
                {
                    sSiop = ((Int32)oSiop).ToString();
                    if (o != DBNull.Value)
                    {
                        dDpo = ((DateTime)o).AddDays(-(Int32)oSiop);
                        sDpo = ((DateTime)dDpo).ToString("dd.MM.yy");
                    }
                }

                Object oSiso = Head.Rows[0]["срок_исполнения_склад_отгрузка"];
                DateTime? dDps = null;
                if (oSiso != DBNull.Value)
                {
                    sSiso = ((Int32)oSiso).ToString();
                    if (dDpo != null)
                    {
                        dDps = ((DateTime)dDpo).AddDays(-(Int32)oSiso);
                        sDps = ((DateTime)dDps).ToString("dd.MM.yy");
                    }
                }

                Object oSizs = Head.Rows[0]["срок_исполнения_заявка_склад"];
                DateTime? dDozp = null;
                if (oSizs != DBNull.Value)
                {
                    sSizs = ((Int32)oSizs).ToString();
                    if (dDps != null)
                    {
                        dDozp = ((DateTime)dDps).AddDays(-(Int32)oSizs);
                        sDozp = ((DateTime)dDozp).ToString("dd.MM.yy");
                    }
                }
            }
        }

        public class СпецификацииТаблицаГрафик
        {
            private DataTable dt;
            public Int32 RowsCount { get => (dt == null) ? 0 : dt.Rows.Count; }
            public class ItemArray
            {
                public String id;
                public String код_спецификации;
                public String дата;
                public String количество;
                public String tp_id;
                public String тип_формирования;
                public String срок_годности;

                public String this[String fieldName]
                {
                    get
                    {
                        String s = null;
                        var field = typeof(ItemArray).GetField(fieldName);
                        if (field != null)
                        {
                            s = (String)field.GetValue(this);
                        }
                        return s;
                    }
                }
            }
            public ItemArray this[Int32 index]
            {
                get
                {
                    ItemArray items = null;
                    if (dt != null && index >= 0 && index < dt.Rows.Count)
                    {
                        DataRow dr = dt.Rows[index];
                        items = new ItemArray
                        {
                            id = ConvertToString(dr["id"]),
                            код_спецификации = ConvertToString(dr["код_спецификации"]),
                            дата = ConvertToString(dr["дата"]),
                            количество = ConvertToString(dr["количество"]),
                            tp_id = ConvertToString(dr["tp_id"]),
                            тип_формирования = ConvertToString(dr["тип_формирования"]),
                            срок_годности = ConvertToString(dr["срок_годности"])
                        };
                        if (items.количество.Length > 4) { items.количество = items.количество.Substring(0, items.количество.Length - 4); }
                        if (items.срок_годности.Length == 8) { items.срок_годности = items.срок_годности.Substring(3, 5); }
                    }
                    return items;
                }
            }
            public СпецификацииТаблицаГрафик(DataTable dt)
            {
                this.dt = dt;
            }
        }
        private static String ConvertToString(Object v)
        {
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
                        s = ((Int32)v).ToString();
                        break;
                    case "System.Boolean":
                        s = ((Boolean)v).ToString();
                        break;
                    case "System.String":
                        s = (String)v;
                        break;
                    case "System.Decimal":
                        s = ((Decimal)v).ToString("n3");
                        break;
                    case "System.DateTime":
                        s = ((DateTime)v).ToString("dd.MM.yy");
                        break;
                    default:
                        s = "FNet.Supply.Models.F0Model.ConvertToString() result: " + tfn;
                        break;
                }
            }
            return s;
        }
        private DataTable CreateSheduleTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("tp_id", typeof(String));
            foreach (DataRow sdr in Table.Rows)
            {
                DataRow ddr = dt.NewRow();
                ddr["tp_id"] = ConvertToString(sdr["tp_id"]);
                dt.Rows.Add(ddr);
            }
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT [tp_id]");
            for (int ri = 0; ri < График.RowsCount; ri++)
            {
                var items = График[ri];
                String colNameQty = $"{items.дата} {items.тип_формирования} q";
                String colNameExp = $"{items.дата} {items.тип_формирования} e";
                if (!dt.Columns.Contains(colNameQty))
                {
                    dt.Columns.Add(colNameQty, typeof(String));
                    sql.Append($", SUM([{colNameQty}] as [{colNameQty}])");
                    dt.Columns.Add(colNameExp, typeof(String));
                    sql.Append($", Max([{colNameExp}]) as [{colNameExp}]");
                }
                foreach (DataRow dr in dt.Rows)
                {
                    if ((String)dr["tp_id"] == items.tp_id)
                    {
                        dr[colNameQty] = items.количество;
                        dr[colNameExp] = items.срок_годности;
                        break;
                    }
                }
            }
            sql.Append($" GROUP BY [tp_id]");
            var newDt = dt.AsEnumerable()
                          .GroupBy(r => r.Field<String>("tp_id"))
                          .Select(g =>
                          {
                              var row = dt.NewRow();

                              row["tp_id"] = g.Key;
                              for (int ci = 1; ci < dt.Columns.Count; ci += 2)
                              {
                                  Decimal sum = 0;
                                  Int32 max = 0;
                                  foreach (var r in g)
                                  {
                                      if (Decimal.TryParse(r[ci] as String, out Decimal q))
                                      {
                                          sum += q;
                                      }
                                      String e = r[ci + 1] as String;
                                      if (!String.IsNullOrWhiteSpace(e) && e.Length == 5)
                                      {
                                          max = Math.Max(max, Int32.Parse(e.Substring(3, 2) + e.Substring(0, 2)));
                                      }
                                  }
                                  row[ci] = (sum != 0) ? sum.ToString() : String.Empty;
                                  row[ci + 1] = (max != 0) ? (max % 100).ToString("00") + "." + (max / 100).ToString("00") : String.Empty;
                              }
                              return row;
                          }).CopyToDataTable();
            return newDt;
        }
    }
}
