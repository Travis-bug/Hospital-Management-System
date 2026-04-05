using Microsoft.AspNetCore.Mvc;
namespace Hospital_Management_System.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{ 
    
    [Route("Error/{statusCode:int}")]
    public IActionResult ErrorPage(int statusCode)
    {
        switch (statusCode)
        
        {
            case 404: return View ("404");
            
            case 403: return View ("403");
            
            case 505: return View ("505");
            
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
