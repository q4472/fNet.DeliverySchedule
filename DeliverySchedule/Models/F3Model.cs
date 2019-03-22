using Nskd;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DeliverySchedule.Models
{
    public class F3Model
    {
        private RequestPackage Rqp;
        private Boolean IsDebuggingEnabled;

        public Guid SessionId;
        public Int32 SpecId;
        public DataTable Shedule;
        public String sDpo;
        public String sSiso;
        public String sDps;
        public String sSizs;
        public String sDozp;
        public Спецификация.Шапка СпецификацияШапка;
        public Спецификация.Таблица СпецификацияТаблица;
        public ЗаявкиНаЗакупку.Шапка ЗаявкиНаЗакупкуШапка;
        public ЗаявкиНаЗакупку.Таблица ЗаявкиНаЗакупкуТаблица;

        public F3Model(RequestPackage rqp, Boolean isDebuggingEnabled)
        {
            Rqp = rqp;
            IsDebuggingEnabled = isDebuggingEnabled;
            SessionId = rqp.SessionId;
            if (Int32.TryParse(rqp["id"] as String, out SpecId)) { }
            else { Int32.TryParse(rqp["код_спецификации"] as String, out SpecId); }
        }
        public void Load()
        {
            RequestPackage rqp = new RequestPackage()
            {
                Command = "[Pharm-Sib].[dbo].[спецификации__получить]",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter() { Name = "session_id", Value = SessionId },
                    new RequestParameter() { Name = "id", Value = SpecId }
                }
            };
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11012/");
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                СпецификацияШапка = new Спецификация.Шапка(rsp.Data.Tables[0]);
                if (rsp.Data.Tables.Count > 1)
                {
                    СпецификацияТаблица = new Спецификация.Таблица(rsp.Data.Tables[1]);
                    СпецификацияТаблица.Sort("[номер_строки]");
                }
            }

            if (СпецификацияШапка != null && СпецификацияШапка.RowsCount > 0)
            {
                String o = СпецификацияШапка[0]["дата_первой_поставки"];

                String oSiop = СпецификацияШапка[0]["срок_исполнения_отгрузка_покупатель"];

                DateTime? dDpo = null;
                if (oSiop != null)
                {
                    if (o != "")
                    {
                        dDpo = (DateTime.Parse(o)).AddDays(-Int32.Parse(oSiop));
                        sDpo = ((DateTime)dDpo).ToString("dd.MM.yy");
                    }
                }

                String oSiso = СпецификацияШапка[0]["срок_исполнения_склад_отгрузка"];
                DateTime? dDps = null;
                if (oSiso != null)
                {
                    sSiso = oSiso;
                    if (dDpo != null)
                    {
                        dDps = ((DateTime)dDpo).AddDays(-Int32.Parse(oSiso));
                        sDps = ((DateTime)dDps).ToString("dd.MM.yy");
                    }
                }

                String oSizs = СпецификацияШапка[0]["срок_исполнения_заявка_склад"];
                DateTime? dDozp = null;
                if (oSizs != null)
                {
                    sSizs = oSizs;
                    if (dDps != null)
                    {
                        dDozp = ((DateTime)dDps).AddDays(-Int32.Parse(oSizs));
                        sDozp = ((DateTime)dDozp).ToString("dd.MM.yy");
                    }
                }
            }

            rqp = new RequestPackage()
            {
                Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__получить]",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter() { Name = "session_id", Value = SessionId },
                    new RequestParameter() { Name = "код_спецификации", Value = SpecId }
                }
            };
            rsp = rqp.GetResponse("http://127.0.0.1:11012/");
            if (rsp != null && rsp.Data != null && rsp.Data.Tables.Count > 0)
            {
                ЗаявкиНаЗакупкуШапка = new ЗаявкиНаЗакупку.Шапка(rsp.Data.Tables[0]);
                ЗаявкиНаЗакупкуШапка.Sort("[дата_поставки_покупателю], [тип_формирования]");
                if (rsp.Data.Tables.Count > 1)
                {
                    ЗаявкиНаЗакупкуТаблица = new ЗаявкиНаЗакупку.Таблица(rsp.Data.Tables[1]);
                }
            }

            Shedule = CreateSheduleTable();
        }
        public void Update()
        {
            Rqp.Command = "[dbo].[спецификации_шапка_update_from_not_null_values]";
            Rqp.AddSessionIdToParameters();
            Rqp.GetResponse("http://127.0.0.1:11012/");

            Load();
        }
        public void Update2()
        {
            // исправляем количество со старой датой
            foreach (RequestParameter p in Rqp.Parameters)
            {
                String name = p.Name;
                if (!String.IsNullOrWhiteSpace(name)
                    && name.Length == 88
                    && (new Regex(@"[0-9a-f-]{36} [0-9a-f-]{36} \d\d\.\d\d\.\d\d \d \d [qel]")).IsMatch(name))
                {
                    Guid.TryParse(name.Substring(0, 36), out Guid tpId);
                    Guid.TryParse(name.Substring(37, 36), out Guid uid);
                    String field = name.Substring(87, 1);
                    String value = p.Value as String;
                    RequestPackage rqp1 = null;
                    if (field == "q")
                    {
                        rqp1 = new RequestPackage
                        {
                            SessionId = SessionId,
                            Command = "[DeliverySchedule].[dbo].[заявки_на_закупку_таблица__изменить_количество]",
                            Parameters = new RequestParameter[]
                            {
                                new RequestParameter { Name = "session_id", Value = SessionId },
                                new RequestParameter { Name = "tp_id", Value = tpId },
                                new RequestParameter { Name = "uid", Value = uid },
                                new RequestParameter { Name = "количество", Value = Nskd.Convert.ToDecimalOrNull(value) }
                            }
                        };
                    }
                    else if (field == "e")
                    {
                        value = (value.Length == 5) ? $"20{value.Substring(3, 2)}-{value.Substring(0, 2)}-01" : null;
                        rqp1 = new RequestPackage
                        {
                            SessionId = SessionId,
                            Command = "[DeliverySchedule].[dbo].[заявки_на_закупку_таблица__изменить_срок_годности]",
                            Parameters = new RequestParameter[]
                            {
                                new RequestParameter { Name = "session_id", Value = SessionId },
                                new RequestParameter { Name = "tp_id", Value = tpId },
                                new RequestParameter { Name = "uid", Value = uid },
                                new RequestParameter { Name = "срок_годности", Value = value }
                            }
                        };
                    }
                    else if (field == "l")
                    {
                        rqp1 = new RequestPackage
                        {
                            SessionId = SessionId,
                            Command = "[DeliverySchedule].[dbo].[заявки_на_закупку_таблица__изменить_срок_исполнения]",
                            Parameters = new RequestParameter[]
                            {
                                new RequestParameter { Name = "session_id", Value = SessionId },
                                new RequestParameter { Name = "tp_id", Value = tpId },
                                new RequestParameter { Name = "uid", Value = uid },
                                new RequestParameter { Name = "срок_исполнения_отгрузка_покупатель", Value = Nskd.Convert.ToDecimalOrNull(value) }
                            }
                        };
                    }
                    if (rqp1 != null)
                    {
                        var rsp = rqp1.GetResponse("http://127.0.0.1:11012/");
                    }
                }
            }
            // изменяем дату
            foreach (RequestParameter p in Rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (!String.IsNullOrWhiteSpace(name)
                    && name.Length == 51
                    && (new Regex(@"[0-9a-f-]{36} \d\d\.\d\d\.\d\d \d \d [qe]")).IsMatch(name)
                    && value != null && (new Regex(@"\d\d\.\d\d\.\d\d")).IsMatch(value))
                {
                    Guid.TryParse(name.Substring(0, 36), out Guid uid);
                    DateTime.TryParse(name.Substring(37, 8), out DateTime oldDate);
                    DateTime.TryParse(value, out DateTime newDate);
                    RequestPackage rqp1 = new RequestPackage
                    {
                        SessionId = SessionId,
                        Command = "[DeliverySchedule].[dbo].[заявки_на_закупку_шапка__изменить_дату]",
                        Parameters = new RequestParameter[]
                        {
                            new RequestParameter { Name = "session_id", Value = SessionId },
                            new RequestParameter { Name = "uid", Value = uid },
                            new RequestParameter { Name = "old_date", Value = oldDate },
                            new RequestParameter { Name = "new_date", Value = newDate }
                        }
                    };
                    ResponsePackage rsp = rqp1.GetResponse("http://127.0.0.1:11012/");
                }
            }
            // обновить данные для страницы
            Load();
        }
        public void Corr()
        {
            Rqp.AddSessionIdToParameters();
            Rqp.Command = "[Pharm-Sib].[dbo].[спецификации_зачёт_журнал_add]";
            ResponsePackage rsp = Rqp.GetResponse("http://127.0.0.1:11012");
        }
        public void AddColumn()
        {
            if (Rqp == null || SpecId == 0) { throw new ArgumentException(); }
            String fType = Rqp["тип_формирования"] as String;
            if (String.IsNullOrWhiteSpace(fType)
                || !(fType == "0" || fType == "1" || fType == "2")) { throw new ArgumentException(); }
            RequestPackage rqp1 = new RequestPackage()
            {
                SessionId = SessionId,
                Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__добавить]",
                Parameters = new RequestParameter[]
                {
                            new RequestParameter { Name = "session_id", Value = SessionId },
                            new RequestParameter { Name = "код_спецификации", Value = SpecId },
                            new RequestParameter { Name = "тип_формирования", Value = fType }
                }
            };
            var rsp = rqp1.GetResponse("http://127.0.0.1:11012/");
        }
        public void DelColumn()
        {
            if (Rqp == null || SpecId == 0) { throw new ArgumentException(); }
            if (!DateTime.TryParse(Rqp["дата_поставки_покупателю"] as String, out DateTime dpp)) { throw new ArgumentException(); }
            Rqp["дата_поставки_покупателю"] = dpp;
            Rqp.Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__удалить]";
            Rqp.AddSessionIdToParameters();
            var rsp = Rqp.GetResponse("http://127.0.0.1:11012/");
        }
        public void Send()
        {
            if (Rqp == null) { throw new ArgumentException(); }
            Rqp.Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__передать_в_отдел_снабжения]";
            Rqp.AddSessionIdToParameters();
            ResponsePackage rsp = Rqp.GetResponse("http://127.0.0.1:11012/");

            String address = "sokolov_ea@farmsib.ru";
            String subject = "Заявка на закупку";
            String body = Nskd.JsonV3.ToString(Rqp); ;
            String attachment = $@"
                <!DOCTYPE HTML>
                <html>
                 <head>
                  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                  <title>DeliverySchedule</title>
                 </head>
                 <body>
                  <div>{body}</div>
                 </body>
                </html>";
            RequestPackage rqp1 = new RequestPackage()
            {
                SessionId = Rqp.SessionId,
                Command = "Prep.F4.SendEmail",
                Parameters = new RequestParameter[]
                {
                    new RequestParameter(){ Name = "session_id", Value = Rqp.SessionId },
                    new RequestParameter(){ Name = "address", Value = address },
                    new RequestParameter(){ Name = "subject", Value = subject },
                    new RequestParameter(){ Name = "body", Value = body },
                    new RequestParameter(){ Name = "attachment", Value = attachment }
                }
            };
            rqp1.GetResponse("http://127.0.0.1:11007/");

            if (!IsDebuggingEnabled)
            {
                rqp1["address"] = "grshanin@farmsib.ru";
                rqp1.GetResponse("http://127.0.0.1:11007/");

                rqp1["address"] = "dm@farmsib.ru";
                rqp1.GetResponse("http://127.0.0.1:11007/");
            }
        }

        public class Спецификация
        {
            public class Шапка
            {
                private DataTable dt;
                public Шапка(DataTable dt) { this.dt = dt; }
                public Int32 RowsCount { get => (dt == null) ? 0 : dt.Rows.Count; }
                public СтрокаДанных this[Int32 index]
                {
                    get
                    {
                        СтрокаДанных row = null;
                        if (dt != null && index >= 0 && index < dt.Rows.Count)
                        {
                            DataRow dr = dt.Rows[index];
                            row = new СтрокаДанных(dr);
                        }
                        return row;
                    }
                }
                public class СтрокаДанных
                {
                    private DataRow dr;
                    public СтрокаДанных(DataRow dr)
                    {
                        this.dr = dr;
                    }
                    public String this[String fieldName]
                    {
                        get
                        {
                            String v = String.Empty;
                            if (!String.IsNullOrWhiteSpace(fieldName) && dr != null && dr.Table.Columns.Contains(fieldName))
                            {
                                v = ConvertToString(dr[fieldName]);
                            }
                            return v;
                        }
                    }
                }
            }
            public class Таблица
            {
                private DataTable dt;
                public Таблица(DataTable dt) { this.dt = dt; }
                public Int32 RowsCount { get => (dt == null) ? 0 : dt.Rows.Count; }
                public СтрокаДанных this[Int32 index]
                {
                    get
                    {
                        СтрокаДанных row = null;
                        if (dt != null && index >= 0 && index < dt.Rows.Count)
                        {
                            DataRow dr = dt.Rows[index];
                            row = new СтрокаДанных(dr);
                        }
                        return row;
                    }
                }
                public class СтрокаДанных
                {
                    private DataRow dr;
                    public СтрокаДанных(DataRow dr)
                    {
                        this.dr = dr;
                    }
                    public String this[String fieldName]
                    {
                        get
                        {
                            String v = String.Empty;
                            if (!String.IsNullOrWhiteSpace(fieldName) && dr != null && dr.Table.Columns.Contains(fieldName))
                            {
                                v = ConvertToString(dr[fieldName]);
                            }
                            return v;
                        }
                    }
                }
                public void Sort(String expression)
                {
                    dt.DefaultView.Sort = expression;
                    dt = dt.DefaultView.ToTable();
                }
            }
        }
        public class ЗаявкиНаЗакупку
        {
            public class Шапка
            {
                private DataTable dt;
                public Шапка(DataTable dt) { this.dt = dt; }
                public Int32 RowsCount { get => (dt == null) ? 0 : dt.Rows.Count; }
                public СтрокаДанных this[Int32 index]
                {
                    get
                    {
                        СтрокаДанных row = null;
                        if (dt != null && index >= 0 && index < dt.Rows.Count)
                        {
                            DataRow dr = dt.Rows[index];
                            row = new СтрокаДанных(dr);
                        }
                        return row;
                    }
                }
                public class СтрокаДанных
                {
                    private DataRow dr;
                    public СтрокаДанных(DataRow dr)
                    {
                        this.dr = dr;
                    }
                    public String this[String fieldName]
                    {
                        get
                        {
                            String v = String.Empty;
                            if (!String.IsNullOrWhiteSpace(fieldName) && dr != null && dr.Table.Columns.Contains(fieldName))
                            {
                                v = ConvertToString(dr[fieldName]);
                            }
                            return v;
                        }
                    }
                }
                public void Sort(String expression)
                {
                        dt.DefaultView.Sort = expression;
                        dt = dt.DefaultView.ToTable();
                }
            }
            public class Таблица
            {
                private DataTable dt;
                public Таблица(DataTable dt) { this.dt = dt; }
                public Int32 RowsCount { get => (dt == null) ? 0 : dt.Rows.Count; }
                public СтрокаДанных this[Int32 index]
                {
                    get
                    {
                        СтрокаДанных row = null;
                        if (dt != null && index >= 0 && index < dt.Rows.Count)
                        {
                            DataRow dr = dt.Rows[index];
                            row = new СтрокаДанных(dr);
                        }
                        return row;
                    }
                }
                public class СтрокаДанных
                {
                    private DataRow dr;
                    public СтрокаДанных(DataRow dr)
                    {
                        this.dr = dr;
                    }
                    public String this[String fieldName]
                    {
                        get
                        {
                            String v = String.Empty;
                            if (!String.IsNullOrWhiteSpace(fieldName) && dr != null && dr.Table.Columns.Contains(fieldName))
                            {
                                v = ConvertToString(dr[fieldName]);
                            }
                            return v;
                        }
                    }
                }
            }
        }

        private static String ConvertToString(Object v)
        {
            String s = String.Empty;
            CultureInfo ic = CultureInfo.InvariantCulture;
            if (v != null && v != DBNull.Value)
            {
                String tfn = v.GetType().FullName;
                switch (tfn)
                {
                    case "System.Guid":
                        s = ((Guid)v).ToString(null, ic);
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
                        s = ((Decimal)v).ToString("n3", ic);
                        break;
                    case "System.DateTime":
                        s = ((DateTime)v).ToString("dd.MM.yy", ic);
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
            for (int ri = 0; ri < ЗаявкиНаЗакупкуШапка.RowsCount; ri++)
            {
                var row = ЗаявкиНаЗакупкуШапка[ri];
                String uid = ConvertToString(row["uid"]);
                String cn = ConvertToString(row["дата_поставки_покупателю"]);
                String ct = ConvertToString(row["тип_формирования"]);
                String pz = (ConvertToString(row["передано_в_закупку"]) == "True") ? "1" : "0";
                String colNameQty = $"{uid} {cn} {ct} {pz} q";
                String colNameExp = $"{uid} {cn} {ct} {pz} e";
                String colNameLag = $"{uid} {cn} {ct} {pz} l";
                dt.Columns.Add(colNameQty, typeof(String));
                dt.Columns.Add(colNameExp, typeof(String));
                dt.Columns.Add(colNameLag, typeof(String));
            }

            for (int ri = 0; ri < СпецификацияТаблица.RowsCount; ri++)
            {
                var row = СпецификацияТаблица[ri];
                String tpId = ConvertToString(row["tp_id"]);
                if (!String.IsNullOrWhiteSpace(tpId))
                {
                    DataRow ddr = dt.NewRow();
                    ddr["tp_id"] = tpId;
                    dt.Rows.Add(ddr);
                }
            }
            for (int ri = 0; ri < ЗаявкиНаЗакупкуТаблица.RowsCount; ri++)
            {
                ЗаявкиНаЗакупку.Таблица.СтрокаДанных row = ЗаявкиНаЗакупкуТаблица[ri];
                String uid = String.Empty;
                String cn = String.Empty;
                String ct = String.Empty;
                String pz = String.Empty;
                for (int i = 0; i < ЗаявкиНаЗакупкуШапка.RowsCount; i++)
                {
                    var r = ЗаявкиНаЗакупкуШапка[i];
                    if (r["uid"] == row["parent_uid"])
                    {
                        uid = r["uid"];
                        cn = r["дата_поставки_покупателю"];
                        ct = r["тип_формирования"];
                        pz = (r["передано_в_закупку"] == "True") ? "1" : "0";
                        break;
                    }
                }
                String colNameQty = $"{uid} {cn} {ct} {pz} q";
                String colNameExp = $"{uid} {cn} {ct} {pz} e";
                String colNameLag = $"{uid} {cn} {ct} {pz} l";
                DataRow[] drs = dt.Select($"[tp_id] = '{row["tp_id"]}'");
                if (drs.Length > 0)
                {
                    drs[0][colNameQty] = row["количество"];
                    drs[0][colNameExp] = row["срок_годности"];
                    drs[0][colNameLag] = row["срок_исполнения_отгрузка_покупатель"];
                }
            }
            return dt;
        }
    }
}
