using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WEBAPP_FitMatch.Filters // เช็ค namespace ให้ตรงกับของคุณด้วยนะครับ
{
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // ตรวจสอบว่ามี user_id ใน Session ไหม
            var userId = context.HttpContext.Session.GetInt32("user_id");
            
            if (userId == null)
            {
                // ถ้ายังไม่ล็อกอิน ให้เด้งไปหน้า Login (สมมติว่าอยู่ที่ HomeController Action "Login")
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            
            base.OnActionExecuting(context);
        }
    }
}