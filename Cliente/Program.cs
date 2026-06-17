using System;
using System.Net.Sockets;
using System.Text;
using System.Net.Http;

// ⚠️ PONÉ TU IP DEL SERVIDOR
string IPServidor = "10.122.40.3"; // Cambiá por tu IP
int Puerto = 8888;

string NombreBox = "BOX 1"; // Cambiá a BOX 2 para la otra

using HttpClient http = new HttpClient();

Console.WriteLine("=================================");
Console.WriteLine($"        {NombreBox}");
Console.WriteLine("=================================");
Console.WriteLine($"Servidor: {IPServidor}:{Puerto}");
Console.WriteLine("\n✅ ENTER → LLAMAR");
Console.WriteLine("🔙 FLECHA IZQUIERDA ← → ANTERIOR");
Console.WriteLine("➡️ FLECHA DERECHA → → SIGUIENTE");
Console.WriteLine("🔄 F2 → REINICIAR TODO");
Console.WriteLine("❌ Cerrar ventana → SALIR\n");

while (true)
{
    Console.WriteLine("Esperando acción...");
    var tecla = Console.ReadKey(true);

    if (tecla.Key == ConsoleKey.Enter)
    {
        try
        {
            using (TcpClient cliente = new TcpClient(IPServidor, Puerto))
            using (var escritor = new StreamWriter(cliente.GetStream(), Encoding.UTF8))
            {
                escritor.Write(NombreBox);
                escritor.Flush();
            }

            string respuesta = await http.GetStringAsync($"http://{IPServidor}:8081");
            int inicio = respuesta.IndexOf("<div class='mensaje'>") + 20;
            int fin = respuesta.IndexOf("</div>", inicio);
            string mensaje = respuesta.Substring(inicio, fin - inicio);
            
            Console.WriteLine($"\n✅ LLAMADO: {mensaje}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }
    else if (tecla.Key == ConsoleKey.LeftArrow)
    {
        try
        {
            await http.GetStringAsync($"http://{IPServidor}:8081/anterior");
            Console.WriteLine("\n🔙 Volviendo al anterior...");
        }
        catch
        {
            Console.WriteLine("\n❌ No se pudo retroceder");
        }
    }
    else if (tecla.Key == ConsoleKey.RightArrow)
    {
        try
        {
            await http.GetStringAsync($"http://{IPServidor}:8081/siguiente");
            Console.WriteLine("\n➡️ Avanzando al siguiente...");
        }
        catch
        {
            Console.WriteLine("\n❌ No se pudo avanzar");
        }
    }
    else if (tecla.Key == ConsoleKey.F2)
    {
        Console.Write("\n⚠️ ¿Reiniciar todo? (S/N): ");
        var confirmar = Console.ReadKey(true);
        // ✅ Corregido: usamos las teclas correctas
        if (confirmar.Key == ConsoleKey.S || confirmar.Key == ConsoleKey.S)
        {
            try
            {
                await http.GetStringAsync($"http://{IPServidor}:8081/reiniciar");
                Console.WriteLine("\n🔄 Sistema reiniciado correctamente");
            }
            catch
            {
                Console.WriteLine("\n❌ No se pudo reiniciar");
            }
        }
        else
        {
            Console.WriteLine("\n❌ Reinicio cancelado");
        }
    }

    Console.WriteLine("\n----------------------------------------");
}