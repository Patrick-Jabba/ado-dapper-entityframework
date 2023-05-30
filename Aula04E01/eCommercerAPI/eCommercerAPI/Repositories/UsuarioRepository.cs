using eCommercerAPI.Models;
using System.Data;
using System.Data.SqlClient;

/*
 * Connection => Estabelecer Conexão com o banco.
 * Command => INSERT, UPDATE, DELETE.
 * DataReader => Utiliza uma arquitetura conectada com o banco. SELECT.
 * DataAdapter => Arquitetura desconectada. SELECT, ou seja, vai trazer os dados para a memória do computador e vai desconectar.
 */

namespace eCommercerAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository() 
        {
            _connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> GetUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "SELECT * FROM Usuarios";
                cmd.Connection = (SqlConnection) _connection;

                _connection.Open();

                SqlDataReader dataReader = cmd.ExecuteReader();
                // Dapper, EF, NHibernate (ORM - Object-POO Relational-MER Mapper)

                while (dataReader.Read())
                {
                    Usuario usuario = new Usuario();
                    usuario.Id = dataReader.GetInt32("Id");
                    usuario.Nome = dataReader.GetString("Nome");
                    usuario.Email = dataReader.GetString("Email");
                    usuario.Sexo= dataReader.GetString("Sexo");
                    usuario.RG= dataReader.GetString("RG");
                    usuario.CPF= dataReader.GetString("CPF");
                    usuario.NomeMae= dataReader.GetString("NomeMae");
                    usuario.SituacaoCadastro= dataReader.GetString("SituacaoCadastro");
                    usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                    usuarios.Add(usuario);
                };

            }
            finally { _connection.Close(); }

            return usuarios;
        }

        public Usuario GetUsuario(int id)
        {
            try
            {
                // SQL Injection: SELECT * FROM Usuarios WHERE Nome = 'José' JOSE OR Nome LIKE '%'; DELETE FROM Usuarios;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = $"SELECT *, c.Id ContatoId " +
                                  $"FROM Usuarios u " +
                                  $"LEFT JOIN Contatos c " +
                                  $"ON c.UsuarioId = u.Id " +
                                  $"LEFT JOIN EnderecosEntrega ee " +
                                  $"ON ee.UsuarioId = u.Id " +
                                  $"LEFT JOIN UsuariosDepartamentos ud " +
                                  $"ON ud.UsuarioId = u.Id " +
                                  $"LEFT JOIN Departamentos d " +
                                  $"ON ud.DepartamentoId = d.Id " +
                                  $"WHERE u.Id = @Id ";
                cmd.Connection = (SqlConnection) _connection;
                cmd.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                SqlDataReader dataReader = cmd.ExecuteReader();

                // Dicionario trabalha com uma coleçao de chaves e valores
                Dictionary<int, Usuario> usuarioDicionario = new Dictionary<int, Usuario>();

                while (dataReader.Read())
                {
                    Usuario usuario = new Usuario();

                    if (!usuarioDicionario.ContainsKey(dataReader.GetInt32(0)))
                    {
                        usuario.Id = dataReader.GetInt32(0);
                        usuario.Nome = dataReader.GetString("Nome");
                        usuario.Email = dataReader.GetString("Email");
                        usuario.Sexo = dataReader.GetString("Sexo");
                        usuario.RG = dataReader.GetString("RG");
                        usuario.CPF = dataReader.GetString("CPF");
                        usuario.NomeMae = dataReader.GetString("NomeMae");
                        usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                        usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                        Contato contato = new Contato();
                        contato.Id = dataReader.GetInt32("ContatoId");
                        contato.UsuarioId = usuario.Id;
                        contato.Telefone = dataReader.GetString("Telefone");
                        contato.Celular = dataReader.GetString("Celular");

                        usuario.Contato = contato;

                        usuarioDicionario.Add(usuario.Id, usuario);
                    }
                    else
                    {
                        usuario = usuarioDicionario[dataReader.GetInt32(0)];
                    };
                    
                    EnderecoEntrega enderecoEntrega = new EnderecoEntrega();
                    enderecoEntrega.Id = dataReader.GetInt32(13);
                    enderecoEntrega.UsuarioId = usuario.Id;
                    enderecoEntrega.NomeEndereco = dataReader.GetString("NomeEndereco");
                    enderecoEntrega.CEP = dataReader.GetString("CEP");
                    enderecoEntrega.Estado = dataReader.GetString("Estado");
                    enderecoEntrega.Cidade = dataReader.GetString("Cidade");
                    enderecoEntrega.Bairro = dataReader.GetString("Bairro");
                    enderecoEntrega.Endereco = dataReader.GetString("Endereco");
                    enderecoEntrega.Numero = dataReader.GetString("Numero");
                    enderecoEntrega.Complemento = dataReader.GetString("Complemento");

                    usuario.EnderecosEntrega = (usuario.EnderecosEntrega == null) 
                                            ? new List<EnderecoEntrega>() 
                                            : usuario.EnderecosEntrega;
                    if(usuario.EnderecosEntrega.FirstOrDefault(ee => ee.Id == enderecoEntrega.Id) == null)
                    {
                        usuario.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    Departamento departamento = new Departamento();
                    departamento.Id = dataReader.GetInt32(26);
                    departamento.Nome = dataReader.GetString(27);

                    usuario.Departamentos = (usuario.Departamentos == null)
                                           ? new List<Departamento>()
                                           : usuario.Departamentos;
                    if(usuario.Departamentos.FirstOrDefault(d => d.Id == departamento.Id) == null)
                    {
                        usuario.Departamentos.Add(departamento);
                    }
                
                };
                return usuarioDicionario[usuarioDicionario.Keys.First()];
            }
            catch(Exception ex)
            {
                return null;
            }
            finally { _connection.Close(); }
        }


        public void InsertUsuario(Usuario usuario)
        {
            _connection.Open();
            SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Transaction = transaction;
                cmd.Connection = (SqlConnection)_connection;

                cmd.CommandText = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES(@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(scope_identity() AS int);";

                cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Sexo", usuario.Sexo);
                cmd.Parameters.AddWithValue("@RG", usuario.RG);
                cmd.Parameters.AddWithValue("@CPF", usuario.CPF);
                cmd.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
                cmd.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
                cmd.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

                usuario.Id = (int)cmd.ExecuteScalar();

                cmd.CommandText = "INSERT INTO Contatos(UsuarioId, Telefone, Celular) VALUES(@UsuarioId, @Telefone, @Celular); SELECT CAST(scope_identity() AS int);";
                cmd.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                cmd.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
                cmd.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);

                usuario.Contato.UsuarioId = usuario.Id;
                usuario.Contato.Id = (int)cmd.ExecuteScalar();

                foreach(var endereco in usuario.EnderecosEntrega)
                {
                    cmd = new SqlCommand();
                    cmd.Connection = (SqlConnection)_connection;
                    cmd.Transaction = transaction;

                    cmd.CommandText = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(scope_identity() AS int);";
                    
                    cmd.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                    cmd.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                    cmd.Parameters.AddWithValue("@CEP", endereco.CEP);
                    cmd.Parameters.AddWithValue("@Estado", endereco.Estado);
                    cmd.Parameters.AddWithValue("@Cidade", endereco.Cidade);
                    cmd.Parameters.AddWithValue("@Bairro", endereco.Bairro);
                    cmd.Parameters.AddWithValue("@Endereco", endereco.Endereco);
                    cmd.Parameters.AddWithValue("@Numero", endereco.Numero);
                    cmd.Parameters.AddWithValue("@Complemento", endereco.Complemento);


                    endereco.Id = (int)cmd.ExecuteScalar();
                    endereco.UsuarioId = usuario.Id;
                }

                foreach(var departamento in usuario.Departamentos)
                {
                    cmd = new SqlCommand();
                    cmd.Connection = (SqlConnection)_connection;
                    cmd.Transaction = transaction;

                    cmd.CommandText = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES(@UsuarioId, @DepartamentoId);";
                    
                    cmd.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                    cmd.Parameters.AddWithValue("@DepartamentoId", departamento.Id);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch(Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch(Exception)
                { 
                    // Adicionar no log que o Rollback falhou
                }

                throw new Exception("Erro ao tentar inserir os dados!");
            }
            finally { _connection.Close(); }
        }

        public void UpdateUsuario(Usuario usuario)
        {
            _connection.Open();
            SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id;";
                cmd.Connection = (SqlConnection)_connection;
                cmd.Transaction = transaction;

                cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Sexo", usuario.Sexo);
                cmd.Parameters.AddWithValue("@RG", usuario.RG);
                cmd.Parameters.AddWithValue("@CPF", usuario.CPF);
                cmd.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
                cmd.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
                cmd.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

                cmd.Parameters.AddWithValue("@Id", usuario.Id);

                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Connection= (SqlConnection)_connection; 
                cmd.Transaction = transaction;

                cmd.CommandText = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";
                
                cmd.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                cmd.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
                cmd.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);

                cmd.Parameters.AddWithValue("@Id", usuario.Contato.Id);

                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch(Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch(Exception e)
                {
                    // Registrar no log
                }
                throw new Exception("Erro, não conseguimos atualizar os dados!");
            }
            finally { _connection.Close(); }
        }
        public void DeleteUsuario(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "DELETE FROM Usuarios WHERE Id = @Id";
                cmd.Connection = (SqlConnection)_connection;
                
                cmd.Parameters.AddWithValue("@Id", id);

                _connection.Open();

                cmd.ExecuteNonQuery();
            }
            finally { _connection.Close(); }
        }

        //// Simular um Database
        //private static List<Usuario> _db = new List<Usuario>()
        //// com o static essas informações passam a pertencer a Classe e não a instância
        //{
        //    new Usuario { Id = 1, Nome = "Marcelo Monteiro", Email = "marcelo.monteiro@gmail.com"},
        //    new Usuario { Id = 3, Nome = "Marta Silva", Email = "marta.silva@gmail.com"}

        //};
    }
}
