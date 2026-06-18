using Microsoft.AspNetCore.Mvc;

namespace FormaPdfExcelComparerDotNet.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
