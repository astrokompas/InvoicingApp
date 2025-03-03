using System;
using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface IReportService
    {
        Task<ReportSummary> GenerateReportAsync(DateTime? startDate = null, DateTime? endDate = null, string clientId = null);
    }
}