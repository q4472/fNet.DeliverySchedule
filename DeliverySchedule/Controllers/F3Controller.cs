using DeliverySchedule.Models;
using Nskd;
using System;
using System.Web.Mvc;

namespace MvcApplication2.Areas.Order.Controllers
{
    public class F3Controller : Controller
    {
        public Object Index()
        {
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp);
            m.Load(rqp);
            PartialViewResult pvr = PartialView("~/Views/F3/Index.cshtml", m);
            return pvr;
        }
        public Object Save()
        {
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp);
            m.Update(rqp);
            PartialViewResult pvr = PartialView("~/Views/F3/Index.cshtml", m);
            return pvr;
        }
        public Object Save2()
        {
            RequestPackage rqp = RequestPackage.ParseRequest(Request.InputStream, Request.ContentEncoding);
            F3Model m = new F3Model(rqp);
            m.Update2(rqp);
            PartialViewResult pvr = PartialView("~/Views/F3/Index.cshtml", m);
            return pvr;
        }
    }
}
