namespace BookToAnki.Services;
using System.IO;
using System.Net;

public class LocalWebServer
{
    public void StartLocalServer(string directory, string prefix = "http://localhost:8080/")
    {
        var httpListener = new HttpListener();

        httpListener.Prefixes.Add("http://localhost:8080/");

        httpListener.Start();

        BeginListening(httpListener, directory);
    }

    private void BeginListening(HttpListener httpListener, string directory)
    {
        httpListener.BeginGetContext(async (result) =>
        {
            var context = httpListener.EndGetContext(result);

            // Begin listening for the next request
            BeginListening(httpListener, directory);

            var filename = Path.Combine(directory, context.Request.Url.LocalPath.TrimStart('/'));

            if (File.Exists(filename))
            {
                await using (var fs = File.OpenRead(filename))
                {
                    await fs.CopyToAsync(context.Response.OutputStream);
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.Close();

        }, null);
    }
}
