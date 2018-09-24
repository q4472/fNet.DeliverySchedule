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
            // сначала исправляем количество со старой датой
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (name.Length == 48 && value != null && (new Regex(@"_[0-9a-f-]{36}_\d\d\d\d\-\d\d-\d\d")).IsMatch(name))
                {
                    Guid.TryParse(name.Substring(1, 36), out Guid tpId);
                    DateTime.TryParse(name.Substring(38, 10), out DateTime date);
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
                            new RequestParameter { Name = "количество", Value = value }
                        }
                    };
                    rqp1.GetResponse("http://127.0.0.1:11002/");
                }
            }
            // теперь изменяем дату
            foreach (RequestParameter p in rqp.Parameters)
            {
                String name = p.Name;
                String value = p.Value as String;
                if (name.Length == 11 && value != null && (new Regex(@"\d\d\.\d\d.\d\d")).IsMatch(value))
                {
                    DateTime.TryParse(name.Substring(1, 10), out DateTime oldDate);
                    DateTime.TryParse("20" + value.Substring(6, 2) + "-" + value.Substring(3, 2) + "-" + value.Substring(0, 2), out DateTime newDate);
                    RequestPackage rqp1 = new RequestPackage
                    {
                        SessionId = rqp.SessionId,
                        Command = "dbo.[спецификации_график_изменить_дату]",
                        Parameters = new RequestParameter[]
                        {
                            new RequestParameter { Name = "session_id", Value = rqp.SessionId },
                            new RequestParameter { Name = "id", Value = SpecId },
                            new RequestParameter { Name = "old_date", Value = oldDate },
                            new RequestParameter { Name = "new_date", Value = newDate }
                        }
                    };
                    rqp1.GetResponse("http://127.0.0.1:11002/");
                }
            }
            // обновить данные для страницы
            SpecGet();
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
            ResponsePackage rsp = rqp.GetResponse("http://127.0.0.1:11002/");
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
        }
    }
}
