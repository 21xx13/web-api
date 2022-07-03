using System;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IUserRepository userRepository;
        private IMapper mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [Produces("application/json", "application/xml")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var userFromRepo = userRepository.FindById(userId);
            if (userFromRepo == null)
                return NotFound();
            var userDto = mapper.Map<UserDto>(userFromRepo);
            return Ok(userDto);
        }

        [HttpPost]
        [Produces("application/json", "application/xml")]
        public IActionResult CreateUser([FromBody] UserCreateDto user)
        {
            if (user == null)
                return BadRequest();
            if (string.IsNullOrEmpty(user.Login) || !user.Login.All(c => char.IsLetterOrDigit(c)))
                ModelState.AddModelError("Login", "Empty or not numbers");
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            var userEntity = mapper.Map<UserEntity>(user);
            var createdUserEntity = userRepository.Insert(userEntity);
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = createdUserEntity.Id }, 
                createdUserEntity.Id);
        }
        
        [HttpPut("{userId}")]
        [Produces("application/json", "application/xml")]
        public IActionResult UpdateUser([FromBody] UserUpdateDto user, [FromRoute] Guid userId)
        {
            if (user == null || userId == Guid.Empty)
                return BadRequest();
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            var newUserEntity = new UserEntity(userId);
            mapper.Map(user, newUserEntity);
            userRepository.UpdateOrInsert(newUserEntity, out var isInserted);
            if (isInserted)
                return CreatedAtRoute(
                    nameof(GetUserById),
                    new { userId = newUserEntity.Id }, 
                    newUserEntity.Id);
            return NoContent();
        }
        
        [HttpPatch("{userId}")]
        [Produces("application/json", "application/xml")]
        public IActionResult PartiallyUpdateUser([FromBody] JsonPatchDocument<UserUpdateDto> patchDoc, [FromRoute] Guid userId)
        {
            if (patchDoc == null)
                return BadRequest();
            
            
            if (userId == Guid.Empty || userRepo == null)
                return NotFound();
            
            
            
            var userRepo = userRepository.FindById(userId);
            var updateDto = mapper.Map<UserUpdateDto>(userRepo);
            patchDoc.ApplyTo(updateDto, ModelState);
            TryValidateModel(updateDto);
            return NoContent();
        }
    }
}