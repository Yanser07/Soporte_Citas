using System.Globalization;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SoporteCitas.Api.Models;

namespace SoporteCitas.Api.Services;

public interface IEmailService
{
    Task NotificarNuevaCitaAlAdminAsync(Cita cita, Usuario cliente);
    Task NotificarRespuestaAlUsuarioAsync(Cita cita);
    Task NotificarCancelacionAlAdminAsync(Cita cita, Usuario cliente);
    Task NotificarCitaCompletadaAsync(Cita cita);
    Task NotificarRecuperacionPasswordAsync(string nombre, string correo, string enlace);
    Task NotificarPasswordCambiadaAsync(string nombre, string correo);
}

public class EmailService : IEmailService
{
    private static readonly CultureInfo Cultura = new("es-DO");

    private readonly EmailSettings _settings;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, IConfiguration config, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _config = config;
        _logger = logger;
    }

    public async Task NotificarNuevaCitaAlAdminAsync(Cita cita, Usuario cliente)
    {
        var destino = DestinoAdmin();
        if (string.IsNullOrWhiteSpace(destino))
        {
            _logger.LogWarning("No hay correo de admin configurado; no se envió la notificación de nueva cita.");
            return;
        }

        var contenido = $@"
            <p>Te han solicitado un nuevo trabajo de soporte técnico.</p>
            <table style='border-collapse:collapse'>
              {Fila("Cliente", cliente.Nombre)}
              {Fila("Correo", cliente.Correo)}
              {Fila("Teléfono", cliente.Telefono)}
              {Fila("Fecha y hora", Fecha(cita.FechaHora))}
              {Fila("Servicio", cita.TipoServicio)}
              {Fila("Modalidad", Modalidad(cita))}
              {FilaDireccion(cita)}
              {Fila("Descripción", cita.Descripcion)}
            </table>
            <p>Entra al panel de administración para aceptar o rechazar la solicitud.</p>";

        await EnviarSeguroAsync(destino, $"Nueva solicitud de soporte: {cita.TipoServicio}",
            Plantilla("Nueva solicitud de trabajo", contenido));
    }

    public async Task NotificarRespuestaAlUsuarioAsync(Cita cita)
    {
        var aceptada = cita.Estado == EstadoCita.Aceptada;
        var titulo = aceptada ? "Tu solicitud fue aceptada" : "Tu solicitud no pudo ser aceptada";

        var motivo = string.IsNullOrWhiteSpace(cita.MotivoRespuesta)
            ? ""
            : $"<p><b>Motivo / comentario:</b> {Enc(cita.MotivoRespuesta)}</p>";

        var mensajeFinal = aceptada
            ? "<p>Te esperamos en la fecha y hora indicadas.</p>"
            : "<p>Puedes volver a agendar otra cita en un horario distinto.</p>";

        var contenido = $@"
            <p>Hola {Enc(cita.Usuario.Nombre)},</p>
            <p>{(aceptada ? "Tu solicitud de soporte técnico fue <b>aceptada</b>." : "Lamentamos informarte que tu solicitud de soporte técnico fue <b>rechazada</b>.")}</p>
            <table style='border-collapse:collapse'>
              {Fila("Servicio", cita.TipoServicio)}
              {Fila("Fecha y hora", Fecha(cita.FechaHora))}
              {Fila("Modalidad", Modalidad(cita))}
              {FilaDireccion(cita)}
            </table>
            {motivo}
            {mensajeFinal}";

        await EnviarSeguroAsync(cita.Usuario.Correo,
            aceptada ? "Tu cita fue aceptada" : "Tu cita fue rechazada",
            Plantilla(titulo, contenido));
    }

    public async Task NotificarCancelacionAlAdminAsync(Cita cita, Usuario cliente)
    {
        var destino = DestinoAdmin();
        if (string.IsNullOrWhiteSpace(destino)) return;

        var contenido = $@"
            <p>{Enc(cliente.Nombre)} canceló su cita.</p>
            <table style='border-collapse:collapse'>
              {Fila("Servicio", cita.TipoServicio)}
              {Fila("Fecha y hora", Fecha(cita.FechaHora))}
              {Fila("Modalidad", Modalidad(cita))}
            </table>
            <p>El horario quedó libre nuevamente.</p>";

        await EnviarSeguroAsync(destino, "Una cita fue cancelada", Plantilla("Cita cancelada", contenido));
    }

    public async Task NotificarCitaCompletadaAsync(Cita cita)
    {
        var resumen = string.IsNullOrWhiteSpace(cita.ResumenTrabajo)
            ? ""
            : $"<p><b>Resumen del trabajo realizado:</b><br/>{Enc(cita.ResumenTrabajo).Replace("\n", "<br/>")}</p>";

        var contenido = $@"
            <p>Hola {Enc(cita.Usuario.Nombre)},</p>
            <p>Tu servicio de soporte técnico fue marcado como <b>completado</b>.</p>
            <table style='border-collapse:collapse'>
              {Fila("Servicio", cita.TipoServicio)}
              {Fila("Fecha y hora", Fecha(cita.FechaHora))}
              {Fila("Modalidad", Modalidad(cita))}
            </table>
            {resumen}
            <p>Gracias por confiar en nosotros. Si algo no quedó resuelto, puedes agendar una nueva cita.</p>";

        await EnviarSeguroAsync(cita.Usuario.Correo, "Tu servicio fue completado",
            Plantilla("Servicio completado", contenido));
    }

    public async Task NotificarRecuperacionPasswordAsync(string nombre, string correo, string enlace)
    {
        var contenido = $@"
            <p>Hola {Enc(nombre)},</p>
            <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón para elegir una nueva.
               El enlace es válido por <b>30 minutos</b> y solo puede usarse una vez.</p>
            <p style='margin:24px 0'>
              <a href='{Enc(enlace)}' style='background:#2563eb;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:bold'>
                Restablecer contraseña
              </a>
            </p>
            <p style='font-size:13px;color:#555'>Si el botón no funciona, copia y pega este enlace en tu navegador:<br/>{Enc(enlace)}</p>
            <p>Si no fuiste tú, ignora este mensaje: tu contraseña no cambiará.</p>";

        await EnviarSeguroAsync(correo, "Restablece tu contraseña", Plantilla("Restablecer contraseña", contenido));
    }

    public async Task NotificarPasswordCambiadaAsync(string nombre, string correo)
    {
        var contenido = $@"
            <p>Hola {Enc(nombre)},</p>
            <p>Te confirmamos que la contraseña de tu cuenta fue <b>cambiada</b> correctamente.</p>
            <p>Si no fuiste tú, solicita un nuevo restablecimiento de inmediato y contacta al administrador.</p>";

        await EnviarSeguroAsync(correo, "Tu contraseña fue cambiada", Plantilla("Contraseña actualizada", contenido));
    }

    // ---------- Internos ----------

    private string? DestinoAdmin() =>
        !string.IsNullOrWhiteSpace(_settings.AdminDestino) ? _settings.AdminDestino : _config["Admin:Correo"];

    private static string Enc(string? texto) => WebUtility.HtmlEncode(texto ?? string.Empty);

    private static string Fecha(DateTime fecha) =>
        fecha.ToString("dddd d 'de' MMMM 'de' yyyy, h:mm tt", Cultura);

    private static string Modalidad(Cita cita) => cita.Modalidad switch
    {
        ModalidadAtencion.Remoto => "Remoto (en línea)",
        ModalidadAtencion.Domicilio => "A domicilio",
        _ => "En el local"
    };

    private static string Fila(string etiqueta, string? valor) =>
        $"<tr><td style='padding:2px 12px 2px 0;vertical-align:top'><b>{Enc(etiqueta)}:</b></td><td style='padding:2px 0'>{Enc(valor)}</td></tr>";

    private static string FilaDireccion(Cita cita) =>
        cita.Modalidad == ModalidadAtencion.Domicilio && !string.IsNullOrWhiteSpace(cita.Direccion)
            ? Fila("Dirección", cita.Direccion)
            : string.Empty;

    private static string Plantilla(string titulo, string contenido) => $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #ddd;border-radius:8px;overflow:hidden'>
          <div style='background:#2563eb;color:#fff;padding:16px 24px'><h2 style='margin:0'>{Enc(titulo)}</h2></div>
          <div style='padding:24px;color:#222;line-height:1.5'>{contenido}</div>
        </div>";

    // Si falla el envío, se registra el error pero NO se rompe el flujo de la API
    private async Task EnviarSeguroAsync(string para, string asunto, string html)
    {
        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_settings.NombreRemitente, _settings.Usuario));
            mensaje.To.Add(MailboxAddress.Parse(para));
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Usuario, _settings.Password);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo a {Destino} con asunto '{Asunto}'.", para, asunto);
        }
    }
}