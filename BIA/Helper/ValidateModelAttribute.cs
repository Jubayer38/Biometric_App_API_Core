using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace BIA.Helper
{
    //public class ValidateModelAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(HttpActionContext actionContext)
    //    {
    //        if (actionContext.ModelState.IsValid == false)
    //        {
    //            actionContext.Response = actionContext.Request.CreateErrorResponse(
    //                HttpStatusCode.BadRequest, actionContext.ModelState);
    //        }
    //    }
    //} 
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
