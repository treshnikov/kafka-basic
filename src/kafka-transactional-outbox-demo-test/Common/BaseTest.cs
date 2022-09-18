using System;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Events;

public class BaseTest : IDisposable
{
    protected readonly BooksDbContext Context;

    public BaseTest()
    {
        Context = AppTestContext.CreateDbContext();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            AppTestContext.DestroyDbContext(Context);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BaseTest()
    {
        Dispose(false);
    }
}