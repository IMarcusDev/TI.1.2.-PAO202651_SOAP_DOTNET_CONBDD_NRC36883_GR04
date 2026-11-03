namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

internal static class NavegacionSesion
{
    public const string RutaPrincipal = "//principal/inicio";
    public const string RutaMovimientos = "//principal/movimientos";
    public const string RutaOperaciones = "//principal/operaciones";

    private static bool _avisoVisible;

    /// <param name="aviso">Mensaje a mostrar antes de volver (sesión expirada); null para volver directamente.</param>
    public static async Task VolverAlLoginAsync(Page pagina, string? aviso)
    {
        if (aviso is not null)
        {
            // Varias pestañas pueden detectar a la vez la sesión expirada; el aviso se muestra una sola vez.
            if (_avisoVisible)
                return;
            _avisoVisible = true;
            try
            {
                await pagina.DisplayAlertAsync(Textos.Sesion, aviso, Textos.Aceptar);
            }
            finally
            {
                _avisoVisible = false;
            }
        }

        // Un Shell nuevo descarta las pestañas abiertas y su estado, como FLAG_ACTIVITY_CLEAR_TASK en Android.
        if (Application.Current?.Windows.FirstOrDefault() is Window ventana)
            ventana.Page = new AppShell();
        else
            await Shell.Current.GoToAsync("//login");
    }
}
