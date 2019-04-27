using Nskd;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static DeliverySchedule.Models.Lib;

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
        public ТаблицаДанных СпецификацияШапка;
        public ТаблицаДанных СпецификацияТаблица;
        public ТаблицаДанных ЗаявкиНаЗакупкуШапка;
        public ТаблицаДанных ЗаявкиНаЗакупкуТаблица;

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
                СпецификацияШапка = new ТаблицаДанных(rsp.Data.Tables[0]);
                if (rsp.Data.Tables.Count > 1)
                {
                    СпецификацияТаблица = new ТаблицаДанных(rsp.Data.Tables[1]);
                    СпецификацияТаблица.Sort = "[номер_строки]";
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
                ЗаявкиНаЗакупкуШапка = new ТаблицаДанных(rsp.Data.Tables[0]);
                ЗаявкиНаЗакупкуШапка.Sort = "[дата_поставки_покупателю], [тип_формирования]";
                if (rsp.Data.Tables.Count > 1)
                {
                    ЗаявкиНаЗакупкуТаблица = new ТаблицаДанных(rsp.Data.Tables[1]);
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
                    Guid.TryParse(name.Substring(0, 36), out Guid stUid);
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
                                new RequestParameter { Name = "спецификации_таблица_uid", Value = stUid },
                                new RequestParameter { Name = "uid", Value = uid },
                                new RequestParameter { Name = "количество", Value = global::Nskd.Convert.ToDecimalOrNull(value) }
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
                                new RequestParameter { Name = "спецификации_таблица_uid", Value = stUid },
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
                                new RequestParameter { Name = "спецификации_таблица_uid", Value = stUid },
                                new RequestParameter { Name = "uid", Value = uid },
                                new RequestParameter { Name = "срок_исполнения_отгрузка_покупатель", Value = global::Nskd.Convert.ToDecimalOrNull(value) }
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
            Rqp.Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__удалить]";
            Rqp.AddSessionIdToParameters();
            var rsp = Rqp.GetResponse("http://127.0.0.1:11012/");
        }
        public void Send(ControllerContext cc)
        {
            if (Rqp == null) { throw new ArgumentException(); }
            Rqp.Command = "[DeliverySchedule].[dbo].[заявки_на_закупку__передать_в_отдел_снабжения]";
            Rqp.AddSessionIdToParameters();
            ResponsePackage rsp = Rqp.GetResponse("http://127.0.0.1:11012/");
            String html = RenderPartialViewToString(cc, "~/Views/F3/ЗаявкаНаЗакупкуПечатнаяФорма1.cshtml", rsp.Data);
            String address = "sokolov_ea@farmsib.ru";
            String subject = "Заявка на закупку";
            String body = html;
            String attachment = html;
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

                rqp1["address"] = "su@farmsib.ru";
                rqp1.GetResponse("http://127.0.0.1:11007/");

                rqp1["address"] = "mihajlova_aa@farmsib.ru";
                rqp1.GetResponse("http://127.0.0.1:11007/");
            }
        }

        public static Object FileUpload(Guid sessionId, HttpFileCollectionBase files)
        {
            StringBuilder result = new StringBuilder();
            result.Append("{files:[ ");
            for(int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];
                result.Append("{");
                result.Append($"name:'{Nskd.JsonV3.ToString(file.FileName)}',");
                result.Append($"size:{file.ContentLength}");
                result.Append("},");
            }
            result.Length--;
            result.Append(" ]}");

            return result.ToString();
        }

        private DataTable CreateSheduleTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("спецификации_таблица_uid", typeof(String));
            for (int ri = 0; ri < ЗаявкиНаЗакупкуШапка.RowsCount; ri++)
            {
                var row = ЗаявкиНаЗакупкуШапка[ri];
                String uid = Lib.ConvertToString(row["uid"]);
                String cn = Lib.ConvertToString(row["дата_поставки_покупателю"]);
                String ct = Lib.ConvertToString(row["тип_формирования"]);
                String pz = (Lib.ConvertToString(row["передано_в_закупку"]) == "True") ? "1" : "0";
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
                String stUid = Lib.ConvertToString(row["uid"]);
                if (!String.IsNullOrWhiteSpace(stUid))
                {
                    DataRow ddr = dt.NewRow();
                    ddr["спецификации_таблица_uid"] = stUid;
                    dt.Rows.Add(ddr);
                }
            }
            for (int ri = 0; ri < ЗаявкиНаЗакупкуТаблица.RowsCount; ri++)
            {
                СтрокаДанных row = ЗаявкиНаЗакупкуТаблица[ri];
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
                DataRow[] drs = dt.Select($"[спецификации_таблица_uid] = '{row["спецификации_таблица_uid"]}'");
                if (drs.Length > 0)
                {
                    drs[0][colNameQty] = row["количество", "n0"];
                    drs[0][colNameExp] = row["срок_годности"];
                    drs[0][colNameLag] = row["срок_исполнения_отгрузка_покупатель"];
                }
            }
            return dt;
        }
        private String RenderPartialViewToString(ControllerContext cc, String viewName, Object model)
        {
            String html = null;
            //if (String.IsNullOrWhiteSpace(viewName)) viewName = cc.RouteData.GetRequiredString("Index");
            ControllerBase c = cc.Controller;
            c.ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(cc, viewName);
                ViewContext viewContext = new ViewContext(cc, viewResult.View, c.ViewData, c.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                html = sw.GetStringBuilder().ToString();
            }
            return html;
        }
    }
}
