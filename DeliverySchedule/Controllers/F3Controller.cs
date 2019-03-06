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
                F3Model m = new F3Model(rqp);
                switch (rqp.Command)
                {
                    case "DeliverySchedule.F3.Index.Save2":
                        m.Update2(rqp);
                        m.Load(rqp);
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "DeliverySchedule.F3.Index.AddColumn":
                        m.AddColumn(rqp);
                        m.Load(rqp);
                        v = PartialView("~/Views/F3/Table.cshtml", m);
                        break;
                    case "GoToDeliverySchedule":
                        m.Load(rqp);
                        v = PartialView("~/Views/F3/Index.cshtml", m);
                        break;
                    default:
                        v += $"Не известная команда: '{rqp.Command}'<br />";
                        break;
                }
            }
            return v;
        }
        public Object Save()
        {
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp);
            m.Update(rqp);
            PartialViewResult pvr = PartialView("~/Views/F3/Index.cshtml", m);
            return pvr;
        }
        public Object Corr()
        {
            Object v = "DeliverySchedule.Controllers.F3Controller.Corr()";
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp);
            m.Corr(rqp);
            return v;
        }
    }
}
