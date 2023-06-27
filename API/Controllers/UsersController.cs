using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;   
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers()
        {
            var users = _context.Users.ToList();

            return users; 
        }

        [HttpGet ("{1d)")]
        public ActionResult<AppUser> GetUser(int id)
        {
             return _context.Users.Find(id);
        }
    }
}