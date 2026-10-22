using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace SistemaPedagogico.Web.Portal;
public class LoginInput { [Required,EmailAddress,MaxLength(150)] public string Email {get;set;}=""; [Required,MaxLength(256)] public string Password {get;set;}=""; [MaxLength(120)] public string Name {get;set;}=""; public bool AdoptAccount {get;set;} }
[Route("conta")]
public class AccountController(EducaDb db):Controller {
 [AllowAnonymous,HttpGet("entrar")] public async Task<IActionResult> Login(){ViewBag.Setup=!await db.Users.AnyAsync();ViewBag.Migrate=!await db.Settings.AnyAsync(x=>x.Key==SingleTeacher.OwnerKey);return View("Login",new LoginInput());}
 [AllowAnonymous,HttpPost("entrar"),ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginInput input){
 ViewBag.Setup=!await db.Users.AnyAsync();ViewBag.Migrate=!await db.Settings.AnyAsync(x=>x.Key==SingleTeacher.OwnerKey);
 if(!ModelState.IsValid)return View("Login",input);
 if(ViewBag.Setup==true){ModelState.AddModelError("","Crie a conta da professora primeiro.");return View("Login",input);}
 var key=input.Email.Trim().ToLowerInvariant();var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==key&&x.Active);
 if(u==null||new PasswordHasher<PortalUser>().VerifyHashedPassword(u,u.PasswordHash,input.Password)==PasswordVerificationResult.Failed){await Task.Delay(600);ModelState.AddModelError("","E-mail ou senha inválidos.");return View("Login",input);}
 if(u.Role is "Admin" or "Professor"){
 if(ViewBag.Migrate==true&&!input.AdoptAccount){ModelState.AddModelError("","Confirme que esta será a conta única da professora.");return View("Login",input);}
 if(!await SingleTeacher.AdoptAsync(db,u)){ModelState.AddModelError("","Use a conta única da professora escolhida na atualização.");return View("Login",input);}
 }
 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()),new Claim(ClaimTypes.Name,u.Name),new Claim(ClaimTypes.Role,u.Role),new Claim("entity",u.EntityId?.ToString()??""),new Claim("stamp",u.Stamp)},CookieAuthenticationDefaults.AuthenticationScheme)),new AuthenticationProperties{IsPersistent=false});return Redirect("/portal");
 }
 [AllowAnonymous,HttpPost("configurar"),ValidateAntiForgeryToken] public async Task<IActionResult> Setup(LoginInput input){
 ViewBag.Setup=true;
 if(await db.Users.AnyAsync())return Forbid();
 if(HttpContext.Connection.RemoteIpAddress is not {} ip||!System.Net.IPAddress.IsLoopback(ip)){ModelState.AddModelError("","O primeiro administrador deve ser criado no computador que executa o sistema.");return View("Login",input);}
 if(input.Password.Length<10||string.IsNullOrWhiteSpace(input.Name))ModelState.AddModelError("","Informe seu nome e uma senha com pelo menos 10 caracteres.");
 if(!ModelState.IsValid)return View("Login",input);
 // Serializa a configuração inicial por transação SQLite; a segunda requisição revalida.
 await using var tx=await db.Database.BeginTransactionAsync();if(await db.Users.AnyAsync())return Forbid();
 var u=new PortalUser{Name=input.Name.Trim(),Email=input.Email.Trim().ToLowerInvariant(),Role="Admin"};u.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(u,input.Password);db.Users.Add(u);await db.SaveChangesAsync();await tx.CommitAsync();await tx.DisposeAsync();await SingleTeacher.AdoptAsync(db,u);TempData["Success"]="Conta da professora criada. Entre com seu e-mail e senha.";return RedirectToAction(nameof(Login));
 }
 [Authorize,HttpPost("sair"),ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction(nameof(Login));}
 [AllowAnonymous,HttpGet("negado")] public IActionResult Denied()=>Content("Você não tem permissão para acessar esta área.");
}
