
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

//revisado y aprovado por Marcos Escobar
// Acentos de los mensajes del servidor y el arte de inicio.
Console.OutputEncoding = System.Text.Encoding.UTF8;

//revisado y aprovado por Marcos Escobar
// Dirección del servidor opcional: dotnet run -- http://<ip>:<puerto>
if (args.Length > 0)
  CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP.ConfigHTTP.BaseUrl = args[0].TrimEnd('/');

// Splash Art
MessagesUtils.PrintMonster();
Thread.Sleep(TimeSpan.FromSeconds(3));

//revisado y aprovado por Marcos Escobar
// Login y menú del perfil; al cerrar sesión (o si expira) se vuelve al inicio de sesión.
while (LoginView.UserHasLogin())
{
  await MenuView.Show();
}

MessagesUtils.PrintMessage("Hasta Luego!");
Thread.Sleep(TimeSpan.FromSeconds(3));

