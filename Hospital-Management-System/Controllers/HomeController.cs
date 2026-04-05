using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // <-- Add this
using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.Models.ViewModels;

namespace Hospital_Management_System.Controllers; 

[Authorize] // <-- This forces the login redirect!
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous] // <-- Lets unauthenticated users see errors
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}