using NovaTime.IdentityAuth;
using NovaTime.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Cors;
using System.Linq;

namespace NovaTime.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    [EnableCors("MyCorsImplimentationPolicy")]
    public class UsersController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
                                      IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;


        }

        //POST : /api/ApplicationUser/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> PostApplicationUser([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Already Exists." });
            ApplicationUser User = new()
            {
                Id = model.Matricule,
                Email = model.Email,
                
                 SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(User, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            {
                var defaultrole = _roleManager.FindByNameAsync(model.Role).Result;
                if (defaultrole != null)
                {
                    await _userManager.AddToRoleAsync(User, defaultrole.Name);
                }
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully" });
        }

        [HttpGet]
        [Route("GetAllUsers")] 
        public async Task<ActionResult<List<UserRoleModel>>> GetAllUser()
        {
            var users = _userManager.Users.Select(c => new Models.UserRoleModel()
            {
                Username = c.UserName,
                Email = c.Email,
                Role = string.Join(",", _userManager.GetRolesAsync(c).Result.ToArray()),
                Matricule = c.Id,
                PhoneNumber = c.PhoneNumber,
            }).ToList();
            if (users.Count > 0)
            {
                return users;
            }
            return NotFound(new Response { Status = "error", Message = "il y a aucun des utilisateurs" });
        }

   
        [HttpDelete("DeleteUserById/{matricule}")]
        public async Task<ActionResult<ApplicationUser>> DeleteUser(string matricule)
        {
            var user = await _userManager.FindByIdAsync(matricule);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return Ok(new Response { Status = "succes", Message = "l'utilisateur a été supprime avec succés" });
            }
            return NotFound(new Response { Status = "error", Message = "l'utilisateur n'a pas trouvée" });
        }

        [HttpPut("UpdateUserById/{id}")]
        public async Task<ActionResult<ApplicationUser>> Update(string id, [FromBody] RegisterModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(model.Email))
                    user.Email = model.Email;
                if (!string.IsNullOrEmpty(model.Password))
                    user.PasswordHash = model.Password;
                if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
                {
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return Ok(new Response { Status = "succes", Message = "l'utilisateur a été modifie avec succés" });
                }
            }
            return NotFound(new Response { Status = "error", Message = "l'utilisateur n'a pas trouvée" });
        }
    } 
}
