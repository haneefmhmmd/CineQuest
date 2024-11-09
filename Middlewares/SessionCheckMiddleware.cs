namespace _301301555_301287005_Laylay_Muhammad__Lab3.Middlewares
{
    public class SessionCheckMiddleware
    {
        private readonly RequestDelegate _next;

        // Define excluded paths within the middleware
        private readonly string[] _excludedPaths = new[]
        {
        "/Users/Login",
        "/Users/Register"
        };

        public SessionCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request path is in the excluded paths
            if (_excludedPaths.All(path => !context.Request.Path.Value.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
            {
                // If session ID is not present, redirect to login
                if (context.Session.GetInt32("UserId") == null)
                {
                    context.Response.Redirect("/Users/Login");
                    return; // Short-circuit the pipeline
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }


}
