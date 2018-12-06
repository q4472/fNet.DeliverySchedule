using Nskd;
using System;
using System.Data;
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

        public F3Model(RequestPackage rqp)
        {
            SessionId = rqp.SessionId;
            Int32.TryParse(rqp["id"] as String, out SpecId);
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
                String value = p.Value as String;
                if (name.Length == 50 && value != null && (new Regex(@"_[0-9a-f-]{36}_\d\d\d\d\-\d\d\-\d\d \d")).IsMatch(name))
                {
                    Guid.TryParse(name.Substring(1, 36), out Guid tpId);
                    DateTime.TryParse(name.Substring(38, 10), out DateTime date);
                    Int32.TryParse(name.Substring(49, 1), out Int32 ftype);
                    RequestPackage rqp1 = new RequestPackage
                    {
                        SessionId = rqp.SessionId,
                        Command = "dbo.[спецификации_график_изменить_количество]",
                        Parameters = new RequestParameter[]
                        {
                            new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                            new RequestParameter { Name = "код_спецификации", Value = SpecId },
                            new RequestParameter { Name = "tp_id", Value = tpId },
                            new RequestParameter { Name = "дата", Value = date },
                            new RequestParameter { Name = "количество", Value = value },
                            new RequestParameter { Name = "тип_формирования", Value = ftype }
                        }
                    };
                    rqp1.GetResponse("http://127.0.0.1:11012/");
                }
            }
            // изменяем дату
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (name.Length == 12 && (new Regex(@"\d\d\d\d\-\d\d\-\d\d \d")).IsMatch(name)
                    && value != null && (new Regex(@"\d\d\.\d\d\.\d\d")).IsMatch(value))
                {
                    DateTime.TryParse(name.Substring(0, 10), out DateTime oldDate);
                    DateTime.TryParse("20" + value.Substring(6, 2) + "-" + value.Substring(3, 2) + "-" + value.Substring(0, 2), out DateTime newDate);
                    Int32.TryParse(name.Substring(11, 1), out Int32 ftype);
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
        private void SpecGet()
        {
            RequestPackage rqp = new RequestPackage()
            {
                Command = "[dbo].[спецификации_get]",
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
                        Shedule = rsp.Data.Tables[2];
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
    }
}
