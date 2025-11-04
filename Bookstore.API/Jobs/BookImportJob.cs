using Bookstore.Application.Interfaces;
using Quartz;

namespace Bookstore.API.Jobs
{
    [DisallowConcurrentExecution]
    public class BookImportJob(
        IBookImportService bookImportService,
        ILogger<BookImportJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("BookImportJob started at {Time}", DateTime.UtcNow);

            try
            {
                var importedCount = await bookImportService.ImportBooksAsync();

                logger.LogInformation(
                    "BookImportJob completed successfully at {Time}. Imported {Count} books",
                    DateTime.UtcNow, importedCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "BookImportJob failed at {Time}", DateTime.UtcNow);
                throw new JobExecutionException(ex);
            }
        }
    }
}
