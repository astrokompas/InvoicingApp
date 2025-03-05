using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvoicingApp.ViewModels;

namespace InvoicingApp.Services
{
    public static class ViewModelPerformanceExtensions
    {
        public static async Task RunOnBackgroundThreadAsync(this BaseViewModel viewModel, Func<Task> action)
        {
            try
            {
                viewModel.IsLoading = true;
                await Task.Run(async () => await action());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in background thread: {ex.Message}");
                throw;
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}