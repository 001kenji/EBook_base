
using EBook.Data;
using EBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;

[Authorize]
public class LibrariesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public LibrariesController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: LIBRARYS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Library.ToListAsync());
    }

    // GET: LIBRARYS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var library = await _context.Library
            .FirstOrDefaultAsync(m => m.id == id);
        if (library == null)
        {
            return NotFound();
        }

        return View(library);
    }

    // GET: LIBRARYS/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }


    [HttpGet]
    public async Task<IActionResult> ReadFile(int id)
    {
        var library = await _context.Library.FindAsync(id);
        if (library == null || string.IsNullOrWhiteSpace(library.FilePath))
            return NotFound(new { error = "File not found." });

        // Normalize the stored FilePath.
        // Expected format: "/uploads/<filename>" or "uploads/<filename>"
        var stored = library.FilePath.Replace('\\', '/').Trim();

        // Strip leading slashes and the "wwwroot" prefix if present, so we can
        // combine cleanly with WebRootPath.
        var relative = stored.TrimStart('/');
        if (relative.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            relative = relative.Substring("wwwroot/".Length);

        // Build the physical path under wwwroot.
        var physicalPath = System.IO.Path.Combine(_environment.WebRootPath, relative);

        // Safety: prevent path traversal outside wwwroot.
        var fullRoot = System.IO.Path.GetFullPath(_environment.WebRootPath);
        var fullPath = System.IO.Path.GetFullPath(physicalPath);
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Invalid file path." });

        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { error = "The file no longer exists on disk." });

        var extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
        var fileName = System.IO.Path.GetFileName(fullPath);

        // Build the browser-facing URL (always under /uploads/<fileName>).
        var webUrl = "/uploads/" + Uri.EscapeDataString(fileName);

        // Plain-text formats → return content inline.
        var textExtensions = new[]
        {
        ".txt", ".md", ".csv", ".json", ".xml",
        ".html", ".htm", ".log", ".cs", ".js", ".css"
    };

        if (textExtensions.Contains(extension))
        {
            var content = await System.IO.File.ReadAllTextAsync(fullPath);
            return Json(new { type = "text", name = fileName, content });
        }

        // Binary formats → return a URL the browser can render.
        return Json(new
        {
            type = extension switch
            {
                ".pdf" => "pdf",
                ".png" or ".jpg" or ".jpeg" or ".gif"
                    or ".webp" or ".bmp" or ".svg" => "image",
                _ => "unsupported"
            },
            name = fileName,
            url = webUrl
        });
    }

    // POST: LIBRARYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,id,Scope,Curriculum")] EBook.Models.Library library, IFormFile uploadFile)
    {
        if(uploadFile != null && uploadFile.Length > 0)
        {
            // define storage directory in wwwroot/uploads
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            // generate a unique filename to prevent overwriting existing files
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            // svae physical file to wwwroot/uploads
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadFile.CopyToAsync(fileStream);
            }
            // save the relative URL path to the dateabase entity
            library.FilePath = "/uploads/" +uniqueFileName;
            _context.Add(library);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", "Please select a valid file to upload");
        return View(library);
    }

    // GET: LIBRARYS/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var library = await _context.Library.FindAsync(id);
        if (library == null)
        {
            return NotFound();
        }
        return View(library);
    }

    // POST: LIBRARYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost, ActionName("Edit")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id, IFormFile uploadFile)
    {
        if (id == null) return NotFound();

        var LibraryToEdit = await _context.Library.FirstOrDefaultAsync(l => l.id == id);
        if (LibraryToEdit == null) return NotFound();

        // Store old path for cleanup after new file saves successfully
        string oldFilePath = LibraryToEdit.FilePath;

        // 1. Update text properties from form submission (excluding FilePath)
        if (await TryUpdateModelAsync<EBook.Models.Library>(
            LibraryToEdit,
            "",
            lib => lib.Name, lib => lib.Curriculum, lib => lib.Scope))
        {
            // 2. Process replacement file if uploaded
            if (uploadFile != null && uploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFile.CopyToAsync(fileStream);
                }

                // Assign new relative path directly to model
                LibraryToEdit.FilePath = "/uploads/" + uniqueFileName;

                // Delete previous physical file from wwwroot
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    string oldPhysicalPath = Path.Combine(_environment.WebRootPath, oldFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        System.IO.File.Delete(oldPhysicalPath);
                    }
                }
            }

            // 3. Save all changes (text + updated FilePath)
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
        }

        return View(LibraryToEdit);
    }

    // GET: LIBRARYS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var library = await _context.Library
            .FirstOrDefaultAsync(m => m.id == id);
        if (library == null)
        {
            return NotFound();
        }

        return View(library);
    }

    // POST: LIBRARYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var library = await _context.Library.FindAsync(id);
        if (library != null)
        {
            _context.Library.Remove(library);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LibraryExists(int? id)
    {
        return _context.Library.Any(e => e.id == id);
    }
}
