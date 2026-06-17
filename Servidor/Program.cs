using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// CONFIGURACIÓN
int PuertoTCP = 8888;
int PuertoWeb = 8081;
string ultimoMensaje = "ESPERANDO LLAMADA...";
int numeroActual = 0;

// HISTORIAL DE LLAMADAS
List<string> historial = new List<string>();
int posicionActual = -1;

// Obtener IP automáticamente
string ipLocal = "";
foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
{
    if (ip.AddressFamily == AddressFamily.InterNetwork)
    {
        ipLocal = ip.ToString();
        break;
    }
}

// 📄 Función para enviar el logo
void EnviarLogo(HttpListenerContext contexto)
{
    try
    {
        string rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "logo.png");
        if (File.Exists(rutaCompleta))
        {
            byte[] imagen = File.ReadAllBytes(rutaCompleta);
            contexto.Response.ContentType = "image/png";
            contexto.Response.ContentLength64 = imagen.Length;
            contexto.Response.OutputStream.Write(imagen, 0, imagen.Length);
        }
        else
        {
            contexto.Response.StatusCode = 404;
            using (var writer = new StreamWriter(contexto.Response.OutputStream))
                writer.Write("Logo no encontrado");
        }
        contexto.Response.OutputStream.Close();
    }
    catch
    {
        contexto.Response.StatusCode = 500;
        contexto.Response.OutputStream.Close();
    }
}

// 📥 Procesar mensajes del cliente TCP
void ProcesarMensaje(TcpClient cliente)
{
    using (cliente)
    using (var lector = new StreamReader(cliente.GetStream(), Encoding.UTF8))
    {
        string nombreBox = lector.ReadToEnd().Trim();
        if (!string.IsNullOrEmpty(nombreBox))
        {
            numeroActual++;
            ultimoMensaje = nombreBox + " <br> " + numeroActual;
            
            historial.Add(ultimoMensaje);
            posicionActual = historial.Count - 1;
            
            Console.WriteLine("📥 Recibido desde cliente TCP: " + ultimoMensaje.Replace("<br>", " - "));
        }
    }
}

// 🖥️ Generar página para TV
string GenerarPagina()
{
    return 
    "<!DOCTYPE html>" +
    "<html lang='es'>" +
    "<head>" +
    "    <meta charset='UTF-8'>" +
    "    <meta http-equiv='refresh' content='2'>" +
    "    <title>Registro de las Personas</title>" +
    "    <style>" +
    "        * { margin: 0; padding: 0; box-sizing: border-box; font-family: Arial, sans-serif; }" +
    "        body { background-color: #000; color: #00FF66; min-height: 100vh; position: relative; }" +
    "" +
    "        .encabezado {" +
    "            display: flex;" +
    "            align-items: center;" +
    "            justify-content: space-between;" +
    "            padding: 20px 30px;" +
    "            border-bottom: 2px solid #222;" +
    "        }" +
    "" +
    "        .logo-titulo {" +
    "            display: flex;" +
    "            align-items: center;" +
    "            gap: 20px;" +
    "        }" +
    "" +
    "        .logo {" +
    "            height: 70px;" +
    "            width: auto;" +
    "            display: block;" +
    "        }" +
    "" +
    "        .titulo {" +
    "            font-size: 28px;" +
    "            font-weight: bold;" +
    "            color: #fff;" +
    "        }" +
    "" +
    "        .hora {" +
    "            font-size: 32px;" +
    "            font-weight: bold;" +
    "            color: #fff;" +
    "        }" +
    "" +
    "        .contenido {" +
    "            display: flex;" +
    "            flex-direction: column;" +
    "            align-items: center;" +
    "            justify-content: center;" +
    "            min-height: calc(100vh - 130px);" +
    "            padding: 20px;" +
    "        }" +
    "" +
    "        .mensaje {" +
    "            font-size: clamp(50px, 12vw, 130px);" +
    "            font-weight: bold;" +
    "            text-align: center;" +
    "            line-height: 1.2;" +
    "        }" +
    "" +
    "        .info {" +
    "            font-size: 22px;" +
    "            color: #666;" +
    "            margin-top: 40px;" +
    "        }" +
    "" +
    "        /* Estilo para el mensaje de confirmación en el centro */" +
    "        .modal-centro {" +
    "            position: fixed;" +
    "            top: 50%;" +
    "            left: 50%;" +
    "            transform: translate(-50%, -50%);" +
    "            background: #1a1a1a;" +
    "            border: 2px solid #dc2626;" +
    "            border-radius: 16px;" +
    "            padding: 40px;" +
    "            text-align: center;" +
    "            box-shadow: 0 0 40px rgba(220, 38, 38, 0.5);" +
    "            z-index: 9999;" +
    "            display: none;" +
    "            max-width: 90%;" +
    "        }" +
    "" +
    "        .modal-centro h3 {" +
    "            color: #fff;" +
    "            font-size: 28px;" +
    "            margin-bottom: 20px;" +
    "        }" +
    "" +
    "        .modal-centro p {" +
    "            color: #e5e7eb;" +
    "            font-size: 20px;" +
    "            margin-bottom: 30px;" +
    "        }" +
    "" +
    "        .botones-modal {" +
    "            display: flex;" +
    "            gap: 20px;" +
    "            justify-content: center;" +
    "        }" +
    "" +
    "        .btn-modal {" +
    "            padding: 12px 30px;" +
    "            border: none;" +
    "            border-radius: 8px;" +
    "            font-size: 18px;" +
    "            font-weight: bold;" +
    "            cursor: pointer;" +
    "        }" +
    "" +
    "        .btn-si { background: #dc2626; color: white; }" +
    "        .btn-no { background: #374151; color: white; }" +
    "    </style>" +
    "</head>" +
    "<body>" +
    "    <div class='encabezado'>" +
    "        <div class='logo-titulo'>" +
    "            <img src='/logo.png' alt='Logo' class='logo'>" +
    "            <div class='titulo'>Registro de las Personas</div>" +
    "        </div>" +
    "        <div class='hora'>" + DateTime.Now.ToString("HH:mm:ss") + "</div>" +
    "    </div>" +
    "" +
    "    <div class='contenido'>" +
    "        <div class='mensaje'>" + ultimoMensaje + "</div>" +
    "        <div class='info'>Sistema de turnos</div>" +
    "    </div>" +
    "" +
    "    <!-- Modal de confirmación centrado -->" +
    "    <div id='modalConfirmacion' class='modal-centro'>" +
    "        <h3>⚠️ ATENCIÓN</h3>" +
    "        <p>¿Está seguro que quiere reiniciar todo el sistema?<br>Se borrará todo el historial de llamadas.</p>" +
    "        <div class='botones-modal'>" +
    "            <button class='btn-modal btn-si' onclick='confirmarReinicio()'>Sí, reiniciar</button>" +
    "            <button class='btn-modal btn-no' onclick='cerrarModal()'>No, cancelar</button>" +
    "        </div>" +
    "    </div>" +
    "</body>" +
    "</html>";
}

// 🎛️ Generar panel de control con 10 box
string GenerarPanel()
{
    return 
    "<!DOCTYPE html>" +
    "<html lang='es'>" +
    "<head>" +
    "    <meta charset='UTF-8'>" +
    "    <meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
    "    <title>Panel de Control - Sistema de Turnos</title>" +
    "    <link href='https://cdn.jsdelivr.net/npm/font-awesome@4.7.0/css/font-awesome.min.css' rel='stylesheet'>" +
    "    <style>" +
    "        * { margin: 0; padding: 0; box-sizing: border-box; }" +
    "        body { background: linear-gradient(135deg, #f1f5f9, #e2e8f0); min-height: 100vh; font-family: Arial, sans-serif; display: flex; align-items: center; justify-content: center; padding: 20px; }" +
    "        .panel { background: white; border-radius: 24px; padding: 32px; width: 100%; max-width: 480px; box-shadow: 0 10px 40px rgba(0,0,0,0.15); }" +
    "        .cabecera { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }" +
    "        .titulo { font-size: 24px; font-weight: bold; color: #1e293b; }" +
    "        .estado-conectado { color: #16a34a; font-size: 14px; font-weight: 500; }" +
    "        .estado-desconectado { color: #dc2626; font-size: 14px; font-weight: 500; }" +
    "        .pantalla { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 16px; padding: 24px; text-align: center; margin-bottom: 24px; }" +
    "        .etiqueta { font-size: 14px; color: #64748b; margin-bottom: 8px; }" +
    "        .valor { font-size: 40px; font-weight: bold; color: #2563eb; word-break: break-word; }" +
    "        .navegacion { display: flex; justify-content: center; gap: 32px; margin-bottom: 32px; }" +
    "        .btn-nav { width: 56px; height: 56px; border-radius: 50%; border: none; background: #e2e8f0; font-size: 24px; cursor: pointer; transition: all 0.2s; }" +
    "        .btn-nav:hover { background: #cbd5e1; }" +
    "        .btn-nav:disabled { opacity: 0.4; cursor: not-allowed; }" +
    "        .botones { display: flex; flex-direction: column; gap: 16px; margin-bottom: 24px; }" +
    "        .btn { padding: 14px; border-radius: 12px; border: none; font-size: 18px; font-weight: 500; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; transition: all 0.2s; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }" +
    "        .btn:disabled { opacity: 0.4; cursor: not-allowed; }" +
    "        .btn-llamar { background: #16a34a; color: white; }" +
    "        .btn-llamar:hover { background: #15803d; }" +
    "        .btn-reiniciar { background: #dc2626; color: white; }" +
    "        .btn-reiniciar:hover { background: #b91c1c; }" +
    "        .config { background: #f8fafc; padding: 16px; border-radius: 12px; }" +
    "        .config label { font-size: 14px; color: #64748b; display: block; margin-bottom: 8px; }" +
    "        .config select { width: 100%; padding: 12px; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 16px; }" +
    "        .mensaje { margin-top: 16px; padding: 12px; border-radius: 8px; text-align: center; font-size: 14px; display: none; }" +
    "        .exito { background: #dcfce7; color: #166534; }" +
    "        .error { background: #fecaca; color: #991b1b; }" +
    "    </style>" +
    "    <script>" +
    "        const PUERTO_WEB = 8081;" +
    "        const IP_SERVIDOR = window.location.hostname;" +
    "        let sistemaConectado = false;" +
    "" +
    "        const mensajeActualEl = document.getElementById('mensajeActual');" +
    "        const estadoEl = document.getElementById('estadoSistema');" +
    "        const mensajeSistemaEl = document.getElementById('mensajeSistema');" +
    "        const btnAnterior = document.getElementById('btnAnterior');" +
    "        const btnSiguiente = document.getElementById('btnSiguiente');" +
    "        const btnLlamar = document.getElementById('btnLlamar');" +
    "        const btnReiniciar = document.getElementById('btnReiniciar');" +
    "" +
    "        async function verificarConexion() {" +
    "            try {" +
    "                const respuesta = await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/', { cache: 'no-cache' });" +
    "                if (respuesta.ok) {" +
    "                    sistemaConectado = true;" +
    "                    actualizarEstadoUI(true);" +
    "                    await obtenerEstadoActual();" +
    "                } else throw new Error('Sin respuesta');" +
    "            } catch (error) {" +
    "                sistemaConectado = false;" +
    "                actualizarEstadoUI(false);" +
    "                mensajeActualEl.textContent = 'SIN CONEXIÓN';" +
    "            }" +
    "        }" +
    "" +
    "        async function obtenerEstadoActual() {" +
    "            try {" +
    "                const respuesta = await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/');" +
    "                const html = await respuesta.text();" +
    "                const inicio = html.indexOf(\"<div class='mensaje'>\") + 20;" +
    "                const fin = html.indexOf(\"</div>\", inicio);" +
    "                if (inicio > 20 && fin > inicio) {" +
    "                    const mensaje = html.substring(inicio, fin).replace(/<br>/g, ' ');" +
    "                    mensajeActualEl.textContent = mensaje;" +
    "                }" +
    "            } catch (error) {}" +
    "        }" +
    "" +
    "        async function accionAnterior() {" +
    "            try {" +
    "                await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/anterior');" +
    "                mostrarMensaje('🔙 Retrocedido', 'exito');" +
    "                setTimeout(obtenerEstadoActual, 300);" +
    "            } catch { mostrarMensaje('❌ Error al retroceder', 'error'); }" +
    "        }" +
    "" +
    "        async function accionSiguiente() {" +
    "            try {" +
    "                await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/siguiente');" +
    "                mostrarMensaje('➡️ Avanzado', 'exito');" +
    "                setTimeout(obtenerEstadoActual, 300);" +
    "            } catch { mostrarMensaje('❌ Error al avanzar', 'error'); }" +
    "        }" +
    "" +
    "        async function accionLlamar() {" +
    "            const nombreBox = document.getElementById('nombreBox').value;" +
    "            try {" +
    "                await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/llamar?box=' + encodeURIComponent(nombreBox));" +
    "                mostrarMensaje('✅ Llamada desde ' + nombreBox, 'exito');" +
    "                setTimeout(obtenerEstadoActual, 300);" +
    "            } catch { mostrarMensaje('❌ Error al llamar', 'error'); }" +
    "        }" +
    "" +
    "        // Ahora mostramos el modal en lugar del confirm normal" +
    "        function accionReiniciar() {" +
    "            const modal = document.getElementById('modalConfirmacion', window.parent.document) || document.getElementById('modalConfirmacion');" +
    "            if (modal) {" +
    "                modal.style.display = 'block';" +
    "            } else {" +
    "                if (confirm('¿Reiniciar todo el sistema?')) {" +
    "                    ejecutarReinicio();" +
    "                }" +
    "            }" +
    "        }" +
    "" +
    "        async function ejecutarReinicio() {" +
    "            try {" +
    "                await fetch('http://' + IP_SERVIDOR + ':' + PUERTO_WEB + '/reiniciar');" +
    "                mostrarMensaje('🔄 Sistema reiniciado', 'exito');" +
    "                setTimeout(obtenerEstadoActual, 300);" +
    "            } catch { mostrarMensaje('❌ Error al reiniciar', 'error'); }" +
    "        }" +
    "" +
    "        function actualizarEstadoUI(conectado) {" +
    "            if (conectado) {" +
    "                estadoEl.className = 'estado-conectado';" +
    "                estadoEl.innerHTML = '<i class=\"fa fa-circle mr-1\"></i> Conectado';" +
    "                [btnAnterior, btnSiguiente, btnLlamar, btnReiniciar].forEach(function(b) { b.disabled = false; });" +
    "            } else {" +
    "                estadoEl.className = 'estado-desconectado';" +
    "                estadoEl.innerHTML = '<i class=\"fa fa-circle mr-1\"></i> Sin conexión';" +
    "                [btnAnterior, btnSiguiente, btnLlamar, btnReiniciar].forEach(function(b) { b.disabled = true; });" +
    "            }" +
    "        }" +
    "" +
    "        function mostrarMensaje(texto, tipo) {" +
    "            mensajeSistemaEl.textContent = texto;" +
    "            mensajeSistemaEl.className = 'mensaje ' + tipo;" +
    "            mensajeSistemaEl.style.display = 'block';" +
    "            setTimeout(function() { mensajeSistemaEl.style.display = 'none'; }, 3000);" +
    "        }" +
    "" +
    "        btnAnterior.addEventListener('click', accionAnterior);" +
    "        btnSiguiente.addEventListener('click', accionSiguiente);" +
    "        btnLlamar.addEventListener('click', accionLlamar);" +
    "        btnReiniciar.addEventListener('click', accionReiniciar);" +
    "" +
    "        window.addEventListener('load', function() {" +
    "            verificarConexion();" +
    "            setInterval(verificarConexion, 2000);" +
    "        });" +
    "    </script>" +
    "</head>" +
    "<body>" +
    "    <div class='panel'>" +
    "        <div class='cabecera'>" +
    "            <h2 class='titulo'>Panel de Control</h2>" +
    "            <div id='estadoSistema' class='estado-desconectado'>" +
    "                <i class='fa fa-circle mr-1'></i> Sin conexión" +
    "            </div>" +
    "        </div>" +
    "" +
    "        <div class='pantalla'>" +
    "            <p class='etiqueta'>Estado actual</p>" +
    "            <div id='mensajeActual' class='valor'>ESPERANDO...</div>" +
    "        </div>" +
    "" +
    "        <div class='navegacion'>" +
    "            <button id='btnAnterior' class='btn-nav' disabled>" +
    "                <i class='fa fa-arrow-left'></i>" +
    "            </button>" +
    "            <button id='btnSiguiente' class='btn-nav' disabled>" +
    "                <i class='fa fa-arrow-right'></i>" +
    "            </button>" +
    "        </div>" +
    "" +
    "        <div class='botones'>" +
    "            <button id='btnLlamar' class='btn btn-llamar' disabled>" +
    "                <i class='fa fa-phone'></i> Llamar siguiente" +
    "            </button>" +
    "            <button id='btnReiniciar' class='btn btn-reiniciar' disabled>" +
    "                <i class='fa fa-refresh'></i> Reiniciar sistema" +
    "            </button>" +
    "        </div>" +
    "" +
    "        <div class='config'>" +
    "            <label for='nombreBox'>Seleccionar Box:</label>" +
    "            <select id='nombreBox'>" +
    "                <option value='BOX 1'>BOX 1</option>" +
    "                <option value='BOX 2'>BOX 2</option>" +
    "                <option value='BOX 3'>BOX 3</option>" +
    "                <option value='BOX 4'>BOX 4</option>" +
    "                <option value='BOX 5'>BOX 5</option>" +
    "                <option value='BOX 6'>BOX 6</option>" +
    "                <option value='BOX 7'>BOX 7</option>" +
    "                <option value='BOX 8'>BOX 8</option>" +
    "                <option value='BOX 9'>BOX 9</option>" +
    "                <option value='BOX 10'>BOX 10</option>" +
    "            </select>" +
    "        </div>" +
    "" +
    "        <div id='mensajeSistema' class='mensaje'></div>" +
    "    </div>" +
    "</body>" +
    "</html>";
}

// --- SERVIDOR TCP ---
TcpListener servidorTCP = new TcpListener(IPAddress.Any, PuertoTCP);
servidorTCP.Start();
Console.WriteLine("✅ Servidor TCP activo - Puerto: " + PuertoTCP);

Thread hiloTCP = new Thread(() =>
{
    while (true)
    {
        try
        {
            TcpClient cliente = servidorTCP.AcceptTcpClient();
            ThreadPool.QueueUserWorkItem(_ => ProcesarMensaje(cliente));
        }
        catch { break; }
    }
}) { IsBackground = true };
hiloTCP.Start();

// --- SERVIDOR WEB ---
HttpListener servidorWeb = new HttpListener();
try
{
    // 🔴 AQUÍ ESTÁ LA MODIFICACIÓN: Escucha en TODAS las direcciones
    servidorWeb.Prefixes.Add("http://*:8081/");
    servidorWeb.Prefixes.Add("http://+:8081/");
    servidorWeb.Start();
    Console.WriteLine("✅ Servidor Web activo - Puerto: 8081");
    Console.WriteLine("📌 Proba en TV: http://10.122.40.4:8081");
    Console.WriteLine("📌 Proba alternativa: http://192.168.0.3:8081");
    Console.WriteLine("📌 Panel: http://10.122.40.4:8081/panel");
}
catch (Exception ex)
{
    Console.WriteLine("❌ ERROR: " + ex.Message);
    Console.WriteLine("💡 Solución: Ejecutá Visual Studio como ADMINISTRADOR");
}

ThreadPool.QueueUserWorkItem(_ =>
{
    while (servidorWeb.IsListening)
    {
        try
        {
            var contexto = servidorWeb.GetContext();
            string ruta = contexto.Request.Url.LocalPath;

            contexto.Response.AddHeader("Access-Control-Allow-Origin", "*");
            contexto.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            contexto.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            if (ruta.Equals("/logo.png", StringComparison.OrdinalIgnoreCase))
            {
                EnviarLogo(contexto);
                continue;
            }

            if (ruta.Equals("/anterior", StringComparison.OrdinalIgnoreCase))
            {
                if (posicionActual > 0)
                {
                    posicionActual--;
                    ultimoMensaje = historial[posicionActual];
                    Console.WriteLine("🔙 Anterior: " + ultimoMensaje.Replace("<br>", " - "));
                }
                contexto.Response.StatusCode = 200;
                contexto.Response.OutputStream.Close();
                continue;
            }

            if (ruta.Equals("/siguiente", StringComparison.OrdinalIgnoreCase))
            {
                if (posicionActual < historial.Count - 1)
                {
                    posicionActual++;
                    ultimoMensaje = historial[posicionActual];
                    Console.WriteLine("➡️ Siguiente: " + ultimoMensaje.Replace("<br>", " - "));
                }
                contexto.Response.StatusCode = 200;
                contexto.Response.OutputStream.Close();
                continue;
            }

            if (ruta.Equals("/reiniciar", StringComparison.OrdinalIgnoreCase))
            {
                historial.Clear();
                posicionActual = -1;
                numeroActual = 0;
                ultimoMensaje = "ESPERANDO LLAMADA...";
                Console.WriteLine("🔄 Sistema reiniciado");
                contexto.Response.StatusCode = 200;
                contexto.Response.OutputStream.Close();
                continue;
            }

            if (ruta.Equals("/llamar", StringComparison.OrdinalIgnoreCase))
            {
                string nombreBox = contexto.Request.QueryString["box"] ?? "BOX 1";
                numeroActual++;
                ultimoMensaje = nombreBox + " <br> " + numeroActual;
                
                historial.Add(ultimoMensaje);
                posicionActual = historial.Count - 1;
                
                Console.WriteLine("📥 Recibido desde panel web: " + ultimoMensaje.Replace("<br>", " - "));
                contexto.Response.StatusCode = 200;
                contexto.Response.OutputStream.Close();
                continue;
            }

            if (ruta.Equals("/panel", StringComparison.OrdinalIgnoreCase))
            {
                string panelHtml = GenerarPanel();
                byte[] datosPanel = Encoding.UTF8.GetBytes(panelHtml);
                contexto.Response.ContentType = "text/html; charset=utf-8";
                contexto.Response.ContentLength64 = datosPanel.Length;
                contexto.Response.OutputStream.Write(datosPanel, 0, datosPanel.Length);
                contexto.Response.OutputStream.Close();
                continue;
            }

            string html = GenerarPagina();
            byte[] datos = Encoding.UTF8.GetBytes(html);
            
            contexto.Response.ContentType = "text/html; charset=utf-8";
            contexto.Response.ContentLength64 = datos.Length;
            contexto.Response.OutputStream.Write(datos, 0, datos.Length);
            contexto.Response.OutputStream.Close();
        }
        catch { break; }
    }
});

Console.WriteLine("\n⌨️ Presioná ENTER para cerrar...");
Console.ReadLine();
servidorTCP.Stop();
servidorWeb?.Stop();