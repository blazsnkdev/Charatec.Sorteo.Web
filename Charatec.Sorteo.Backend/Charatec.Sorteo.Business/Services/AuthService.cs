using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Responses.Auth;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Charatec.Sorteo.Data.Context;

namespace Charatec.Sorteo.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly SorteoDbContext _dbContext;

        private readonly IJwtTokenService _jwtTokenService;
        #region constructor
        public AuthService(
            IUnitOfWork unitOfWork,
            IClienteRepository clienteRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            SorteoDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
        }
        #endregion
        #region REGISTRAR CLIENTE INICIO
        public async Task<Result<ClienteRegistradoResponse>> RegistrarClienteAsync(ClienteRegistrarRequest request, CancellationToken cancellationToken = default)
        {
            var nombres = request.Nombres?.Trim() ?? string.Empty;
            var apellidos = request.Apellidos?.Trim() ?? string.Empty;
            var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var telefono = request.Telefono?.Trim() ?? string.Empty;

            cancellationToken.ThrowIfCancellationRequested();

            var usuarioExistente = await _userManager.FindByEmailAsync(email);

            if (usuarioExistente is not null)
            {
                return Result<ClienteRegistradoResponse>.Failure(
                    new Error(
                        "EMAIL_ALREADY_EXISTS",
                        "El correo electrónico ya está registrado."));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var fechaRegistro = DateTimeOffset.UtcNow;

                var usuario = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FechaRegistro = fechaRegistro,
                    Activo = true
                };

                var identityResult =
                    await _userManager.CreateAsync(
                        usuario,
                        request.Password);

                if (!identityResult.Succeeded)
                {
                    await _unitOfWork.RollBackAsync();

                    if (identityResult.Errors.Any(e =>
                        e.Code is "DuplicateEmail" or
                                  "DuplicateUserName"))
                    {
                        return Result<ClienteRegistradoResponse>.Failure(
                            new Error(
                                "EMAIL_ALREADY_EXISTS",
                                "El correo electrónico ya está registrado."));
                    }

                    if (identityResult.Errors.Any(e =>
                        e.Code.StartsWith(
                            "Password",
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        return Result<ClienteRegistradoResponse>.Failure(
                            new Error(
                                "INVALID_PASSWORD",
                                "La contraseña no cumple los requisitos de seguridad."));
                    }

                    return Result<ClienteRegistradoResponse>.Failure(
                        new Error(
                            "REGISTRATION_FAILED",
                            "No se pudo registrar el usuario."));
                }



                var resultadoRol = await _userManager.AddToRoleAsync(
                    usuario,
                    Rol.Cliente.ToString()
                );

                if (!resultadoRol.Succeeded)
                {
                    await _unitOfWork.RollBackAsync();

                    return Result<ClienteRegistradoResponse>.Failure(
                        new Error(
                            "REGISTRATION_FAILED",
                            "No se pudo completar el registro del cliente."
                        )
                    );
                }


                var cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuario.Id,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Telefono = telefono,
                    FechaCreacion = fechaRegistro,
                    Activo = true,
                    NumeroDocumento = null,
                    FechaNacimiento = null
                };

                await _clienteRepository.AddAsync(
                    cliente,
                    cancellationToken);


                await _unitOfWork.CommitTransactionAsync();


                var response = new ClienteRegistradoResponse(
                    cliente.Id,
                    cliente.Nombres,
                    cliente.Apellidos,
                    email,
                    telefono,
                    fechaRegistro,
                    cliente.Activo,
                    Rol.Cliente.ToString() //TODO: aqui lo seteo
                );

                return Result<ClienteRegistradoResponse>.Success(
                    response);
            }
            catch (Exception ex) when (EsCorreoDuplicado(ex))
            {
                await _unitOfWork.RollBackAsync();

                return Result<ClienteRegistradoResponse>.Failure(
                    new Error(
                        "EMAIL_ALREADY_EXISTS",
                        "El correo electrónico ya está registrado."));
            }
            catch
            {
                await _unitOfWork.RollBackAsync();
                throw;
            }
        }
        #endregion
        private static bool EsCorreoDuplicado(Exception exception)
        {
            var postgres = exception as PostgresException;

            if (exception is DbUpdateException dbException)
            {
                postgres = dbException.InnerException
                    as PostgresException;
            }

            if (postgres?.SqlState !=
                PostgresErrorCodes.UniqueViolation)
            {
                return false;
            }

            return postgres.ConstraintName is
                "UserNameIndex" or
                "UX_AspNetUsers_NormalizedEmail";
        }
        #region LOGIN INICIO SESION
        public async Task<Result<ClienteLoginResponse>> LoginAsync(ClienteLoginRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var email = request.Email?.Trim() ?? string.Empty;

            var password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "El correo es obligatoria."));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "VALIDATION_ERROR",
                        "La contraseña es obligatoria."));
            }


            var usuario = await _userManager.FindByEmailAsync(email);

            if (usuario is null || !usuario.Activo)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "Correo electrónico o contraseña incorrectos."
                    )
                );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var resultadoLogin =
                await _signInManager.CheckPasswordSignInAsync(
                    usuario,
                    password,
                    lockoutOnFailure: true
                );

            if (resultadoLogin.IsLockedOut)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "ACCOUNT_LOCKED",
                        "No se pudo iniciar sesión. Intenta más tarde."
                    )
                );
            }

            if (!resultadoLogin.Succeeded)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "Correo electrónico o contraseña incorrectos."
                    )
                );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var roles = await _userManager.GetRolesAsync(usuario);

            var cliente = await _dbContext.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UsuarioId == usuario.Id,
                    cancellationToken);

            if (cliente is not null && !cliente.Activo)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "Correo electrónico o contraseña incorrectos."));
            }

            if (roles.Contains(Rol.Cliente.ToString()) &&
                cliente is null)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "No se pudo iniciar sesión."));
            }

            var esCliente = roles.Contains(Rol.Cliente.ToString());

            if (esCliente && cliente is null)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "No se pudo iniciar sesión."
                    )
                );
            }

            if (esCliente && cliente is not null && !cliente.Activo)
            {
                return Result<ClienteLoginResponse>.Failure(
                    new Error(
                        "INVALID_CREDENTIALS",
                        "Correo electrónico o contraseña incorrectos."
                    )
                );
            }


            var jwt = _jwtTokenService.Generar(
                usuario.Id,
                usuario.Email ?? email,
                roles);

            var usuarioResponse = new UsuarioAutenticadoResponse(
                Id: usuario.Id,
                Email: usuario.Email ?? email,
                Nombres: cliente?.Nombres,
                Apellidos: cliente?.Apellidos,
                Roles: roles.ToArray()
            );

            var response = new ClienteLoginResponse(
                AccessToken: jwt.AccessToken,
                TokenType: "Bearer",
                ExpiresIn: jwt.ExpiresIn,
                Usuario: usuarioResponse
            );

            return Result<ClienteLoginResponse>.Success(response);
        }
#endregion
    }
}
