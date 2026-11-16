using Charatec.Sorteo.Business.DTOs.Requests.Auth;
using Charatec.Sorteo.Business.DTOs.Responses.Auth;
using Charatec.Sorteo.Business.Interfaces;
using Charatec.Sorteo.Common.Results;
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Data.Interfaces;
using Charatec.Sorteo.Entities.Enums;
using Charatec.Sorteo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Charatec.Sorteo.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClienteRepository _clienteRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(
            IUnitOfWork unitOfWork,
            IClienteRepository clienteRepository,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _clienteRepository = clienteRepository;
            _userManager = userManager;
        }
        public async Task<Result<ClienteRegistradoResponse>>
        RegistrarClienteAsync(
            ClienteRegistrarRequest request,
            CancellationToken cancellationToken = default)
        {
            var nombres = request.Nombres?.Trim() ?? string.Empty;
            var apellidos = request.Apellidos?.Trim() ?? string.Empty;
            var email = request.Email?.Trim().ToLowerInvariant()
                ?? string.Empty;
            var telefono = request.Telefono?.Trim() ?? string.Empty;

            cancellationToken.ThrowIfCancellationRequested();

            var usuarioExistente =
                await _userManager.FindByEmailAsync(email);

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

                // Identity valida la contraseña y guarda
                // solamente su hash en AspNetUsers.

                var identityResult =
                    await _userManager.CreateAsync(
                        usuario,
                        request.Password);

                if (!identityResult.Succeeded)
                {
                    await _unitOfWork.RollBackAsync();

                    // Correo o nombre de usuario duplicado
                    if (identityResult.Errors.Any(e =>
                        e.Code is "DuplicateEmail" or
                                  "DuplicateUserName"))
                    {
                        return Result<ClienteRegistradoResponse>.Failure(
                            new Error(
                                "EMAIL_ALREADY_EXISTS",
                                "El correo electrónico ya está registrado."));
                    }

                    // Contraseña inválida
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

                // =====================================
                // 6. ASIGNAR ROL CLIENTE
                // =====================================

                var resultadoRol =
                    await _userManager.AddToRoleAsync(
                        usuario,
                        Rol.Cliente.ToString());//TODO: aqui lo seteo como string

                if (!resultadoRol.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo asignar el rol CLIENTE. " +
                        "Comprueba que exista en AspNetRoles.");
                }

                // =====================================
                // 7. CREAR PERFIL CLIENTE
                // =====================================

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
    }
}
