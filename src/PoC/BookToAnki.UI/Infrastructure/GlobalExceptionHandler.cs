using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BookToAnki.UI.Infrastructure;

internal class GlobalExceptionHandler
{
    public void SetUpExceptionHandling(App app)
    {
        app.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Debug.WriteLine(e.Exception.ToString());
        MessageBox.Show("An unexpected error occurred: " + e.Exception.Message, "Unexpected error");

        // Prevent default unhandled exception processing
        e.Handled = true;
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            MessageBox.Show("An unexpected error occurred: " + ex.Message, "Unexpected error");
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Handle or log the exception
        Debug.WriteLine(e.Exception.ToString());

        MessageBox.Show("An unexpected error occurred (task scheduler): " + e.Exception.Message, "Unexpected error");
        e.SetObserved(); // Prevent the exception from escalating
    }
}
