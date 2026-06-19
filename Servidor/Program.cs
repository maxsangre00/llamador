using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace SistemaTurnos
{
    class ServidorTurnos
    {
        // Contadores independientes
        private static int contadorBox = 0;
        private static int contadorMatrimonio = 0;
        private static int contadorNacimiento = 0;
        private static int contadorDefuncion = 0;

        // Historial de llamados
        private static List<string> historial = new List<string>();
        private static int posicionActual = -1;
        private static string ultimoMensaje = "ESPERANDO LLAMADA...";

        static void Main(string[] args)
        {
            HttpListener servidor = new HttpListener();
            servidor.Prefixes.Add("http://+:8081/");
            servidor.Start();
            Console.WriteLine("✅ Servidor de turnos iniciado en el puerto 8081");
            Console.WriteLine("📺 Pantalla: http://10.122.40.3:8081");
            Console.WriteLine("🎛️ Panel: abrí tu archivo HTML");

            while (true)
            {
                HttpListenerContext contexto = servidor.GetContext();
                ThreadPool.QueueUserWorkItem(ProcesarSolicitud, contexto);
            }
        }

        private static void ProcesarSolicitud(object estado)
        {
            HttpListenerContext contexto = (HttpListenerContext)estado;
            try
            {
                // ✅ AGREGADO: Permitir conexión desde cualquier origen
                contexto.Response.AddHeader("Access-Control-Allow-Origin", "*");
                contexto.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                contexto.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                // Responder a solicitudes OPTIONS
                if (contexto.Request.HttpMethod == "OPTIONS")
                {
                    contexto.Response.StatusCode = 200;
                    contexto.Response.Close();
                    return;
                }

                string ruta = contexto.Request.Url.AbsolutePath.ToLower();

                if (ruta.Equals("/llamar", StringComparison.OrdinalIgnoreCase))
                {
                    string nombreOficina = contexto.Request.QueryString["oficina"] ?? "BOX 1";
                    string mensaje = GenerarMensajeYActualizarContador(nombreOficina);
                    historial.Add(mensaje.Replace("<br>", " - "));
                    if (historial.Count > 20) historial.RemoveAt(0); // Guardar últimos 20
                    posicionActual = historial.Count - 1;
                    EnviarRespuesta(contexto, "OK", "text/plain");
                }
                else if (ruta.Equals("/reiniciar", StringComparison.OrdinalIgnoreCase))
                {
                    contadorBox = 0;
                    contadorMatrimonio = 0;
                    contadorNacimiento = 0;
                    contadorDefuncion = 0;
                    historial.Clear();
                    posicionActual = -1;
                    ultimoMensaje = "ESPERANDO LLAMADA...";
                    Console.WriteLine("🔄 Sistema reiniciado completamente");
                    EnviarRespuesta(contexto, "OK", "text/plain");
                }
                else if (ruta.Equals("/anterior", StringComparison.OrdinalIgnoreCase))
                {
                    if (posicionActual > 0)
                    {
                        posicionActual--;
                        ultimoMensaje = historial[posicionActual].Replace(" - ", "<br>");
                    }
                    EnviarRespuesta(contexto, "OK", "text/plain");
                }
                else if (ruta.Equals("/siguiente", StringComparison.OrdinalIgnoreCase))
                {
                    if (posicionActual < historial.Count - 1)
                    {
                        posicionActual++;
                        ultimoMensaje = historial[posicionActual].Replace(" - ", "<br>");
                    }
                    EnviarRespuesta(contexto, "OK", "text/plain");
                }
                else
                {
                    string pagina = GenerarPagina();
                    EnviarRespuesta(contexto, pagina, "text/html");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
                contexto.Response.StatusCode = 500;
            }
            finally
            {
                contexto.Response.Close();
            }
        }

        private static string GenerarMensajeYActualizarContador(string nombreOficina)
        {
            string etiqueta = nombreOficina;
            int numero = 0;

            if (nombreOficina.StartsWith("BOX"))
            {
                contadorBox++;
                numero = contadorBox;
                etiqueta = nombreOficina;
            }
            else if (nombreOficina == "Matrinio Ofi. Nº1" || nombreOficina == "Matrimonio Ofi. Nº1")
            {
                contadorMatrimonio++;
                numero = contadorMatrimonio;
                etiqueta = "Matrimonio Ofi. Nº1";
            }
            else if (nombreOficina == "Nacimiento Ofi. Nº2")
            {
                contadorNacimiento++;
                numero = contadorNacimiento;
                etiqueta = "Nacimiento Ofi. Nº2";
            }
            else if (nombreOficina == "Defuncion Ofi. Nº3")
            {
                contadorDefuncion++;
                numero = contadorDefuncion;
                etiqueta = "Defuncion Ofi. Nº3";
            }
            else
            {
                contadorBox++;
                numero = contadorBox;
                etiqueta = nombreOficina;
            }

            ultimoMensaje = $"{etiqueta}<br>N° {numero}";
            return ultimoMensaje;
        }

        private static string GenerarPagina()
        {
            string listaHistorial = "";
            for (int i = historial.Count - 1; i >= 0; i--)
            {
                listaHistorial += $"<div class='item-historial'>{historial[i]}</div>";
            }
            if (string.IsNullOrWhiteSpace(listaHistorial))
            {
                listaHistorial = "<div class='vacio'>Sin llamadas registradas</div>";
            }

            return @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta http-equiv='refresh' content='2'>
    <title>Sistema de Turnos</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; font-family: Arial, sans-serif; }
        body {
            background-color: #000;
            color: #00FF66;
            min-height: 100vh;
            display: grid;
            grid-template-columns: 1fr 320px;
        }

        /* Zona principal */
        .zona-principal {
            display: flex;
            flex-direction: column;
            padding: 20px;
        }

        .encabezado {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 20px 0;
            border-bottom: 2px solid #222;
            margin-bottom: 30px;
        }

        .titulo {
            font-size: 32px;
            font-weight: bold;
            color: #ffffff;
        }

        .hora {
            font-size: 36px;
            font-weight: bold;
            color: #ffffff;
        }

        .contenido {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
        }

        .mensaje {
            font-size: clamp(60px, 15vw, 140px);
            font-weight: bold;
            text-align: center;
            line-height: 1.2;
        }

        /* Panel historial derecha */
        .panel-historial {
            background-color: #111111;
            border-left: 2px solid #222;
            padding: 25px 20px;
            overflow-y: auto;
            max-height: 100vh;
        }

        .titulo-historial {
            font-size: 24px;
            font-weight: bold;
            color: #ffffff;
            text-align: center;
            padding-bottom: 15px;
            margin-bottom: 20px;
            border-bottom: 1px solid #333;
        }

        .item-historial {
            padding: 14px;
            margin-bottom: 12px;
            background-color: #1a1a1a;
            border-radius: 8px;
            font-size: 19px;
            color: #00FF66;
            line-height: 1.4;
        }

        .vacio {
            color: #555;
            text-align: center;
            padding: 40px 15px;
            font-style: italic;
            font-size: 18px;
        }
    </style>
</head>
<body>
    <div class='zona-principal'>
        <div class='encabezado'>
            <div class='titulo'>REGISTRO DE LAS PERSONAS</div>
            <div class='hora'>" + DateTime.Now.ToString("HH:mm:ss") + @"</div>
        </div>
        <div class='contenido'>
            <div class='mensaje'>" + ultimoMensaje + @"</div>
        </div>
    </div>

    <div class='panel-historial'>
        <h3 class='titulo-historial'>ÚLTIMOS LLAMADOS</h3>
        <div class='lista-historial'>" + listaHistorial + @"</div>
    </div>
</body>
</html>";
        }

        private static void EnviarRespuesta(HttpListenerContext contexto, string contenido, string tipo)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(contenido);
            contexto.Response.ContentType = tipo;
            contexto.Response.ContentLength64 = buffer.Length;
            contexto.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}