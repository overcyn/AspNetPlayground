using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace aspnetplayground.Controllers
{
    public class AddCustomHeaderFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            filterContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        }
    }
}