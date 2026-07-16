# Security Review — Event Image Upload

Scope: the event image upload feature (`IFormFile` → `ImageUploadService` → `EventService` → `EventController` / Admin Razor Pages), reviewed against the OWASP unrestricted file upload checklist. Four real weaknesses were found and fixed during this review; they're marked **[FIXED]** below.

## Security measures implemented

**File size validation** — capped at 5 MB, checked in `ImageUploadService.SaveEventImageAsync` before any content is read. **[FIXED]** Previously this check ran only after ASP.NET Core had already buffered the entire multipart body (Kestrel's default request-body ceiling is much higher than 5 MB), so an oversized payload was fully received before being rejected. Added `[RequestSizeLimit(6 MB)]` on the `Create`/`Update` actions in `EventController` so the framework itself rejects an oversized request early, before it's fully read into memory/disk.

**Extension validation** — whitelist of exactly `.jpg`, `.jpeg`, `.png`, `.webp` (case-insensitive), checked against `Path.GetExtension(file.FileName)`. Everything else is rejected regardless of declared `Content-Type`.

**Magic byte (file signature) validation** — the first 12 bytes of the actual file content are read and checked against the real signature for the claimed extension (JPEG `FF D8 FF`, PNG `89 50 4E 47 0D 0A 1A 0A`, WEBP `RIFF....WEBP`). A file renamed to `.png` that isn't actually a PNG is rejected even though its extension passed. The stream position is reset after this check so the same bytes get written to disk — no separate re-read, no gap between what was validated and what was saved.

**Unique filenames** — every stored file is named `{Guid.NewGuid()}{extension}`. The client's original filename is discarded entirely; it's never used to construct a path.

**Safe file storage** — the destination directory (`wwwroot/uploads/events`) is a fixed, hardcoded path with no user input in it. Combined with GUID filenames, there is no path-traversal surface: no user-controlled string ever reaches `Path.Combine` for the write path.

**Static file configuration** — `app.UseStaticFiles()` serves only `wwwroot` (only the uploads folder lives there), directory browsing is off by default, and files are served with the correct `Content-Type` derived from the extension we chose (not the client's claimed type). **[FIXED]** Added `X-Content-Type-Options: nosniff` to every static file response, so browsers are explicitly told not to content-sniff a served image into something else, closing a defense-in-depth gap even though the signature check already makes a disguised executable/script very unlikely to reach disk in the first place.

**Authorization** — **[FIXED — this was the most serious finding]** `EventController.Create` and `Update` had no `[Authorize]` at all. Anyone, unauthenticated, could create or update events and upload arbitrary (validated) images through the API. Added `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` to both actions, explicitly pinned to the JWT scheme — the app now has two auth schemes (Cookie for the Razor Pages admin UI, JWT for the API), and without pinning the scheme explicitly, an unauthenticated API caller would have been redirected (302) to the admin login page instead of getting a correct `401`. Verified live: unauthenticated `POST /api/events` now returns `401`; a valid JWT still succeeds; `GET` endpoints remain public by design (read-only, no data exposure risk).

**Error handling** — validation failures return specific, safe messages (`ImageValidationException` → 400 with a message describing exactly what failed) without leaking stack traces, file system paths, or internals. Unhandled exceptions (e.g. disk I/O failure) are not caught anywhere in this feature and fall through to the framework default, which returns an empty 500 body — safe (no information disclosure) but not descriptive; see Limitations.

**Logging** — **[FIXED]** `ImageUploadService` previously had no logging at all — a signature mismatch (the strongest signal of a deliberate spoofing attempt) left no trace. Added `ILogger<ImageUploadService>` with a `Warning` log on every rejection path (missing file, oversized, disallowed extension, signature mismatch — each includes the original filename and reason) and an `Information` log on every successful save/delete (original name, stored GUID name, size). Verified live: uploading a text file renamed to `.jpg` produces `warn: ImageUploadService[0] Image upload rejected: file 'fake.jpg' claims extension '.jpg' but its content signature does not match. Possible spoofed upload.` in the server log.

## Vulnerabilities prevented

- **Unrestricted file upload / remote code execution** — an attacker can't upload a `.php`, `.aspx`, `.exe`, or `.html` file and have it end up executable or servable as such: extension whitelist blocks the extension outright, and even if it somehow got a whitelisted extension, the magic-byte check requires real image content.
- **Extension spoofing** (e.g. `malware.exe` renamed to `malware.jpg`) — caught by the signature check specifically, independent of the extension check. Verified live during this review.
- **Double-extension tricks** (`evil.php.jpg`) — irrelevant here because the original filename is never used for storage; only `Path.GetExtension` (last segment) plus a fresh GUID are used, and the original name is discarded.
- **Path traversal / directory escape** (`../../web.config` as a filename) — impossible, since the stored filename is always a server-generated GUID, never derived from client input.
- **Stored/reflected content-type confusion (MIME-sniffing based issues)** — mitigated by serving correct extensions with `X-Content-Type-Options: nosniff`.
- **Unauthenticated data mutation via the API** — closed by the authorization fix; this was a live, exploitable gap prior to this review.
- **Resource exhaustion via oversized uploads** — the request-size limit now rejects large payloads at the framework layer instead of after full buffering.
- **Orphaned files on disk** — when an event's image is replaced or the event is deleted, the old file is deleted from disk (`DeleteEventImage`), preventing unbounded accumulation of unreferenced files.

## Remaining limitations

- **No malware/AV scanning** — a file that is a genuinely valid JPEG/PNG/WEBP by both extension and signature could still smuggle a payload in EXIF/metadata or exploit a bug in whatever eventually decodes it (browser, image library). This review only validates *that it's a real image of an allowed type*, not that it's free of embedded exploits.
- **No rate limiting** on upload endpoints — a single authenticated (or, before this review, unauthenticated) account can flood `wwwroot/uploads/events` with a large volume of valid-looking images.
- **No per-user upload ownership check** — any authenticated user can update/delete any event's image via the API (organizer/admin-specific authorization was out of scope for this review — see the Admin Panel's own role-based checks for the UI path, which the API path doesn't share).
- **No centralized exception handling** — an unhandled exception (e.g. disk full, permission denied) returns a bare empty 500 rather than a structured error; safe, but not diagnosable from the client side.
- **Static files served without cache-control or ETag tuning** — not a security issue by itself, but no explicit cache policy is set for uploaded images.
- **JWT signing key lives in `appsettings.json`** — functional today, but should move to a secret store (user-secrets locally, Key Vault/env var in production) before this goes anywhere near production.

## Recommended future improvements

1. **Add antivirus/malware scanning** (e.g. ClamAV integration) on uploaded files before they're written to disk, for defense against embedded-payload attacks that pass signature validation.
2. **Rate-limit upload endpoints** (ASP.NET Core's built-in rate limiting middleware, keyed by user/IP) to bound abuse.
3. **Re-encode uploaded images** server-side (e.g. via `ImageSharp`) rather than storing the raw uploaded bytes — this strips EXIF/metadata and any non-image bytes a crafted "valid" image might carry, at the cost of a bit of CPU.
4. **Move uploaded files out of `wwwroot`** and serve them through a controller action with its own access control, if per-user visibility rules are ever needed (currently all event images are intentionally public).
5. **Add a global exception-handling middleware** (`IExceptionHandler` in .NET 8) to return consistent, safe `ProblemDetails` responses instead of empty 500s.
6. **Enforce ownership/role checks on event mutation** in the API (currently: any authenticated user can edit/delete any event, not just their own or as Admin) — the Admin Panel already has this via `[Authorize(Roles = "Admin")]`; the API does not.
7. **Move the JWT signing key** to `dotnet user-secrets` (dev) / environment variable or Key Vault (prod), matching the pattern already used for the OpenWeatherMap API key.
