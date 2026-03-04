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
                // ถ้ายังไม่ล็อกอิน ให้เด้งไปหน้า Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // ป้องกัน browser cache หน้าที่ต้อง login
            // เมื่อ logout แล้วกด back จะ re-request server แทนที่จะโหลดจาก cache
            var response = context.HttpContext.Response;
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";

            base.OnActionExecuting(context);
        }
    }
}