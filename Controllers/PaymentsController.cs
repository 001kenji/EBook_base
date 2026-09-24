
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EBook.Models;
using EBook.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PAYMENTS
    public async Task<IActionResult> Index()    
    {
        //create a query
        var query = _context.Payment.AsQueryable();
        // if user is not an admin, filter by their user id
        if (!User.IsInRole("Admin"))
        {
            //get user id logged in
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            query = query.Where(pay => pay.UserId == currentUserId);
        }
        //execute query asynchronously
        var userPayments = await query.ToListAsync();
        return View(userPayments);
    }

    // GET: PAYMENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _context.Payment.AsQueryable();
        if (!User.IsInRole("Admin"))
        {
            query = query.Where(pay => pay.id == id & pay.UserId == currentUserId);
        }
        var payment = await query.FirstOrDefaultAsync(pay => pay.id == id);

        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);   // ✅ single Payment → Details.cshtml
    }

    // GET: PAYMENTS/Create
    [Authorize(Roles ="Admin"),HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: PAYMENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ActionName("Create")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost([Bind("Amount,PaymentMethod,PaymentDate,PaymentStatus")] Payment payment,string UserEmail)
    {
        if (string.IsNullOrWhiteSpace(UserEmail))
        {
            ModelState.AddModelError("UserEmail", "User email is required");
        }
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        if (ModelState.IsValid)
        {
            var UserRef = _context.Users.FirstOrDefault(userref => userref.Email == UserEmail);
            if (UserRef == null)
            {
                // Adding a model error is better UX than returning a blank 404 NotFound page
                ModelState.AddModelError("UserEmail", "No user found with this email address");
                return View("Create", payment);
            }
            payment.User = UserRef;
            payment.UserId = UserRef.Id;
            _context.Add(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View("Create",payment);
    }

    // GET: PAYMENTS/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await _context.Payment.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }
        return View(payment);
    }

    // POST: PAYMENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [Authorize(Roles = "Admin"), ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var PaymentToEdit = await _context.Payment.FirstOrDefaultAsync(paymentref => paymentref.id == id);
        if (PaymentToEdit == null) return NotFound();
        ModelState.Remove("User");
        ModelState.Remove("UserId");
        if (await TryUpdateModelAsync<Payment>(
            PaymentToEdit,
            "",
            pay => pay.Amount, pay => pay.PaymentMethod, pay => pay.PaymentDate, pay => pay.PaymentStatus))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Please try again.");
            }
        }

        
        return View(PaymentToEdit);
    }

    // GET: PAYMENTS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await _context.Payment
            .FirstOrDefaultAsync(m => m.id == id);
        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    // POST: PAYMENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var payment = await _context.Payment.FindAsync(id);
        if (payment != null)
        {
            _context.Payment.Remove(payment);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PaymentExists(int? id)
    {
        return _context.Payment.Any(e => e.id == id);
    }
}
