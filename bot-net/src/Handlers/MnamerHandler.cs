using System.Diagnostics;
using System.Text;

namespace Bot.Handlers;

public class MnamerHandler
{
    public async Task<string> ExecuteMnamer(string arguments)
    {
        Log.Info($"Executing maner with arguments: {arguments}");
        
        var psi = new ProcessStartInfo
        {
            FileName = "mnamer",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = psi };
        process.Start();

        var output = new StringBuilder();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) output.AppendLine("[ERR] " + e.Data);
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        
        Log.Info("Mnamer Result: " + output);
        
        return output.ToString();
    }
}