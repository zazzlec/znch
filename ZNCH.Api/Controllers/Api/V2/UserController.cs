using ZNCH.Api.Entities;
using ZNCH.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ZNCH.Api.Controllers.api.v2
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v2/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ZNCHDbContext _dbContext;
        public UserController(ZNCHDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult List()
        {
            using (_dbContext)
            {
                var list = _dbContext.DncUser.ToList();
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(list);
                return Ok(response);
            }
        }
    }
}