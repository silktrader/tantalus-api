using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Controllers; 

public abstract class TantalusController : ControllerBase {
    
    protected Guid UserGuid =>
        Guid.Parse((HttpContext.User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   string.Empty);
}