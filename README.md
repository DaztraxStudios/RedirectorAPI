# RedirectorAPI

A Simple ASP.NET Core WebAPI project to redirect links.
Add endpoints to redirect to specific sites in an easy way with JSON

## How to setup?
In the **`redirect.json`** file located in the root directory of the project, you define a list of redirects using key-value pairs.
- The **key** is the URL path segment (endpoint).
- The **value** is the target URL to redirect to.

### Example:
```json
{
  "/google": "https://www.google.com",
  "/github": "https://www.github.com",
  "/docs": "https://docs.microsoft.com"
}
```

### Result (Request	// Redirects to):
- `http://yourdomain.com/google` => `https://www.google.com`
- `http://yourdomain.com/github` => `https://www.github.com`
- `http://yourdomain.com/youtube` => `https://www.youtube.com`
