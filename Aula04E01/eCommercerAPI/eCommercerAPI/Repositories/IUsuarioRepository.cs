using eCommercerAPI.Models;

namespace eCommercerAPI.Repositories
{
    interface IUsuarioRepository
    {
        public List<Usuario> GetUsuarios();

        public Usuario GetUsuario(int id);

        public void InsertUsuario(Usuario usuario);

        public void UpdateUsuario(Usuario usuario);

        public void DeleteUsuario(int id);
    }
}
