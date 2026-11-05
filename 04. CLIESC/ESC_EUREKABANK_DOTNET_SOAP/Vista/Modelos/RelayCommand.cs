using System.Windows.Input;

namespace ec.edu.monster.vista.modelos;

/// <summary>ICommand for a synchronous action that may receive a parameter.</summary>
public sealed class RelayCommand(Action<object?> ejecutar, Func<object?, bool>? puedeEjecutar = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => puedeEjecutar?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => ejecutar(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
