using System.Data.SqlTypes;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        /// <summary>
        /// Controller for testing error responses (auth, not-found, server-error, bad-request).
        /// Used for development and client-side error handling practice.
        /// </summary>
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;
            
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            // Simple auth test - returns a string if authorized
            return "secret text";
        }
        
        [Authorize]
        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            // Intentional 404 test: queries non-existent user (-1)
            var thing = _context.Users.Find(-1);

            if (this == null) return NotFound();
            
            return thing; // Simplified null check
        }
        
        [Authorize]
        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            // Forces a NullReferenceException by calling ToString() on null
             var thing = _context.Users.Find(-1);

             var thingToReturn = thing.ToString(); // Deliberate error for testing

            return thingToReturn;
        }
        
        [Authorize]
        [HttpGet("bad-requst")]
        public ActionResult<string> GetBadRequest()
        {
            // Explicit 400 response test
            return BadRequest ("This was a failed request;");
        }
    }
}
