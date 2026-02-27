using Microsoft.AspNetCore.Mvc;
namespace Clinic_Management.Controllers;
public class ErrorController : Controller
{ 
    
    [Route("Error/{statusCode}")] 
    public IActionResult ErrorPage (int StatusCode)
    {
        switch (StatusCode)
        
        {
            case 404: return View ("404");
            
            case 403: return View ("403");
            
            case 405: return View ("505");
            
            default: 
                return View ("404");
        }
    }
    [Route ("Error/500")]
    public IActionResult Error500 ()
    {
        return View ("500");
    }
    
    
}