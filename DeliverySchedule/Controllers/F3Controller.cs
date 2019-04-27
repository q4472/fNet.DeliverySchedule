using DeliverySchedule.Models;
using Nskd;
using System;
using System.Web.Mvc;

namespace DeliverySchedule.Controllers
{
    public class F3Controller : Controller
    {
        public Object Index()
        {
            Object v = "DeliverySchedule.Controllers.F3Controller.Index()<br />";
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            if (rqp != null && !String.IsNullOrWhiteSpace(rqp.Command))
            {
                Session["SessionId"] = rqp.SessionId;
                F3Model m = new F3Model(rqp, HttpContext.IsDebuggingEnabled);
                switch (rqp.Command)
                {
                    case "DeliverySchedule.F3.Index.DelColumn":
                        m.DelColumn();
                        m.Load();
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "DeliverySchedule.F3.Index.Send":
                        m.Send(ControllerContext);
                        m.Load();
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "DeliverySchedule.F3.Index.Save":
                        m.Update();
                        v = PartialView("~/Views/F3/Index.cshtml", m);
                        break;
                    case "DeliverySchedule.F3.Index.Save2":
                        m.Update2();
                        m.Load();
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "DeliverySchedule.F3.Index.AddColumn":
                        m.AddColumn();
                        m.Load();
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "GoToDeliverySchedule":
                        m.Load();
                        v = PartialView("~/Views/F3/Index.cshtml", m);
                        break;
                    default:
                        v += $"Неизвестная команда: '{rqp.Command}'<br />";
                        break;
                }
            }
            return v;
        }
        public Object Corr()
        {
            Object v = "DeliverySchedule.Controllers.F3Controller.Corr()";
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp, HttpContext.IsDebuggingEnabled);
            m.Corr();
            return v;
        }
        public Object FileUpload()
        {
            Object result;
            Guid sessionId = (Session["SessionId"] != null) ? (Guid)Session["SessionId"] : new Guid();
            result = F3Model.FileUpload(sessionId, Request.Files);
            return result;
        }
    }
}
