using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace InvoicingApp.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _appDataPath;
        private readonly IDialogService _dialogService;

        public BackupService(string appDataPath, IDialogService dialogService)
        {
            _appDataPath = appDataPath;
            _dialogService = dialogService;
        }

        public async Task<bool> BackupDataAsync()
        {
            try
            {
                // Create a save file dialog
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Backup Files (*.zip)|*.zip",
                    Title = "Zapisz kopię zapasową",
                    FileName = $"faktury-MOVING-backup-{DateTime.Now:yyyy-MM-dd}.zip"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string backupPath = saveFileDialog.FileName;

                    // Create a temporary directory for the backup
                    string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);

                    try
                    {
                        // Copy all data to the temporary directory
                        await Task.Run(() => CopyDirectory(_appDataPath, tempDir));

                        // Create a zip file
                        if (File.Exists(backupPath))
                            File.Delete(backupPath);

                        await Task.Run(() => ZipFile.CreateFromDirectory(tempDir, backupPath));

                        _dialogService.ShowInformation(
                            $"Kopia zapasowa została zapisana w: {backupPath}",
                            "Kopia zapasowa utworzona");

                        return true;
                    }
                    finally
                    {
                        // Clean up the temporary directory
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Nie udało się utworzyć kopii zapasowej: {ex.Message}",
                    "Błąd kopii zapasowej");
            }

            return false;
        }

        public async Task<bool> RestoreDataAsync()
        {
            try
            {
                // Create an open file dialog
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Backup Files (*.zip)|*.zip",
                    Title = "Wybierz kopię zapasową do przywrócenia"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string backupPath = openFileDialog.FileName;

                    // Confirm restoration
                    if (_dialogService.ShowQuestion(
                        "Przywrócenie kopii zapasowej spowoduje nadpisanie wszystkich istniejących danych. Czy na pewno chcesz kontynuować?",
                        "Potwierdzenie przywrócenia"))
                    {
                        // Create a temporary directory for restoration
                        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        Directory.CreateDirectory(tempDir);

                        try
                        {
                            // Extract the backup
                            await Task.Run(() => ZipFile.ExtractToDirectory(backupPath, tempDir));

                            // Clear existing data
                            await Task.Run(() =>
                            {
                                foreach (var file in Directory.GetFiles(_appDataPath, "*.json", SearchOption.AllDirectories))
                                {
                                    File.Delete(file);
                                }
                            });

                            // Copy restored data
                            await Task.Run(() => CopyDirectory(tempDir, _appDataPath));

                            _dialogService.ShowInformation(
                                "Dane zostały przywrócone z kopii zapasowej. Aby zobaczyć zmiany, uruchom aplikację ponownie.",
                                "Przywracanie zakończone");

                            return true;
                        }
                        finally
                        {
                            // Clean up the temporary directory
                            if (Directory.Exists(tempDir))
                                Directory.Delete(tempDir, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Nie udało się przywrócić kopii zapasowej: {ex.Message}",
                    "Błąd przywracania");
            }

            return false;
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            // Create the target directory if it doesn't exist
            Directory.CreateDirectory(targetDir);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            // Copy subdirectories
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectory(directory, targetSubDir);
            }
        }
    }

    public interface IBackupService
    {
        Task<bool> BackupDataAsync();
        Task<bool> RestoreDataAsync();
    }
}