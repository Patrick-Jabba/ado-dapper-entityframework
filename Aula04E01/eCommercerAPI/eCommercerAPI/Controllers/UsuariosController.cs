using eCommercerAPI.Models;
using eCommercerAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommercerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioRepository _repository;
        public UsuariosController() 
        {
        
            _repository = new UsuarioRepository();
        }

        /*
            www.minhaapi.com.br/api/Usuarios 
        */

        [HttpGet]
        public IActionResult GetAll() 
        {
            return Ok(_repository.GetUsuarios());
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id) 
        {
            var usuario = _repository.GetUsuario(id);

            if(usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario); 
        }

        [HttpPost]
        public IActionResult Create([FromBody] Usuario usuario)
        {
            try
            {
                _repository.InsertUsuario(usuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public IActionResult Update([FromBody] Usuario usuario)
        {
            try
            {
                _repository.UpdateUsuario(usuario);
                return Ok(usuario);

            }
            catch (Exception ex) 
            { 
                return StatusCode(500, ex.Message);
            }

        }

        [HttpDelete]
        public IActionResult DeleteById(int id) 
        {
            _repository.DeleteUsuario(id);

            return Ok();
        }
    }
}
