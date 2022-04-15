using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tantalus.Models;
using Tantalus.Services;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase {
    private readonly IUserService _userService;

    public UsersController(IUserService userService) {
        _userService = userService;
    }

    private Guid UserGuid =>
        Guid.Parse((HttpContext.User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   string.Empty);

    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> Login(UserLoginRequest loginRequest) {
        var response = await _userService.Login(loginRequest.Name, loginRequest.Password);
        if (response == null) return Unauthorized();

        SetTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken() {
        var token = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token)) return BadRequest("Missing token");

        try {
            _userService.RevokeToken(UserGuid, token);
        }
        catch (Exception) {
            return BadRequest("Attempting to revoke invalid token");
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken() {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return BadRequest("Missing Token");

        try {
            var response = _userService.RefreshToken(refreshToken);
            SetTokenCookie(response.RefreshToken); // tk shouldn't the cookie be removed from the response?
            return Ok(response);
        }
        catch (Exception) {
            return BadRequest("Invalid Token");
        }
    }

    [HttpPost]
    [Route("register")]
    public async Task<ActionResult> Register(UserRegisterRequest registerRequest) {
        if (await _userService.Exists(registerRequest.Name))
            return Forbid();

        await _userService.Register(registerRequest);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public ActionResult Delete(Guid id) {
        if (UserGuid != id) return Unauthorized();
        _userService.Delete(id);
        return Ok();
    }

    // append cookie with refresh token to the http response
    private void SetTokenCookie(string token) {
        var cookieOptions = new CookieOptions {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            SameSite = SameSiteMode.None,
            Secure = true,
            IsEssential = true,
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    public record UserLoginRequest([Required] string Name, [Required] string Password);
}