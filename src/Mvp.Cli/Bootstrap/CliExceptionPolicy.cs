using Mvp.Core.Errors;

namespace Mvp.Cli.Bootstrap;

public sealed class CliExceptionPolicy
{
    public async Task<int> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        TextWriter error,
        bool diagnostic,
        CancellationToken cancellationToken)
    {
        try
        {
            await operation(cancellationToken).ConfigureAwait(false);
            return CliExitCodes.Success;
        }
        catch (ManifestValidationException exception)
        {
            await error.WriteLineAsync("Input validation failed:").ConfigureAwait(false);
            foreach (var validationError in exception.Errors)
            {
                await error.WriteLineAsync($"- {validationError}").ConfigureAwait(false);
            }

            return CliExitCodes.InvalidInput;
        }
        catch (FileNotFoundException exception)
        {
            await WriteAsync(error, exception, diagnostic).ConfigureAwait(false);
            return CliExitCodes.InvalidInput;
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteAsync(error, exception, diagnostic).ConfigureAwait(false);
            return CliExitCodes.InvalidInput;
        }
        catch (GenerationConflictException exception)
        {
            await WriteAsync(error, exception, diagnostic).ConfigureAwait(false);
            return CliExitCodes.Conflict;
        }
        catch (OperationCanceledException)
        {
            await error.WriteLineAsync("Generation cancelled.").ConfigureAwait(false);
            return CliExitCodes.Cancelled;
        }
        catch (GenerationException exception)
        {
            await WriteAsync(error, exception, diagnostic).ConfigureAwait(false);
            return CliExitCodes.GenerationFailure;
        }
        catch (Exception exception)
        {
            if (diagnostic)
            {
                await WriteAsync(error, exception, diagnostic: true).ConfigureAwait(false);
            }
            else
            {
                await error.WriteLineAsync("An unexpected internal error occurred.").ConfigureAwait(false);
            }

            return CliExitCodes.InternalError;
        }
    }

    private Task WriteAsync(TextWriter error, Exception exception, bool diagnostic) =>
        error.WriteLineAsync(diagnostic ? exception.ToString() : exception.Message);
}
