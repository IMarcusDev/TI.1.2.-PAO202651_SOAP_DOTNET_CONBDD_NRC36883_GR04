using System.Windows.Input;

namespace ec.edu.monster.vista.modelos;

/// <summary>ICommand for an asynchronous action; the view model reports its own failures.</summary>
public sealed class AsyncRelayCommand(Func<Task> ejecutar, Func<bool>? puedeEjecutar = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => puedeEjecutar?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        try
        {
            await ejecutar();
        }
        catch (Exception)
        {
            // The view model turns failures into a message; an unhandled exception here would
            // close the application.
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
