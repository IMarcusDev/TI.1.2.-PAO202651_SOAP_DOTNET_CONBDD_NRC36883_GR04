using System.ComponentModel;
using System.Threading;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// Keeps the server indicator up to date: green while it answers, red when it does not.
/// The first check runs immediately and then every five seconds until Detener is called.
/// </summary>
public sealed class EstadoServidorViewModel(EstadoServidor estado) : INotifyPropertyChanged
{
    public const string DisponibleTexto = "Servicio disponible";
    public const string NoDisponibleTexto = "Servicio no disponible";
    public const string ComprobandoTexto = "Comprobando servicio…";

    public const string EstadoComprobando = "checking";
    public const string EstadoDisponible = "up";
    public const string EstadoNoDisponible = "down";

    public static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(5);

    private readonly CancellationTokenSource _cancelacion = new();
    private string _estado = EstadoComprobando;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Estado => _estado;

    public bool EstaDisponible => _estado == EstadoDisponible;

    public string Texto => _estado switch
    {
        EstadoDisponible => DisponibleTexto,
        EstadoNoDisponible => NoDisponibleTexto,
        _ => ComprobandoTexto,
    };

    /// <summary>Runs the first check and then one every Intervalo until Detener.</summary>
    public async Task IniciarAsync()
    {
        await RevisarAsync();
        using var temporizador = new PeriodicTimer(Intervalo);
        try
        {
            while (await temporizador.WaitForNextTickAsync(_cancelacion.Token))
                await RevisarAsync();
        }
        catch (OperationCanceledException)
        {
            // The window was closed.
        }
    }

    /// <summary>Asks the server right now and updates the indicator.</summary>
    public async Task RevisarAsync()
    {
        bool disponible = await estado.EstaDisponibleAsync();
        if (_cancelacion.IsCancellationRequested)
            return;
        Actualizar(disponible ? EstadoDisponible : EstadoNoDisponible);
    }

    public void Detener() => _cancelacion.Cancel();

    private void Actualizar(string estadoNuevo)
    {
        if (_estado == estadoNuevo)
            return;
        _estado = estadoNuevo;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Estado)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EstaDisponible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Texto)));
    }
}
