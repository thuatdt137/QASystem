using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QASystem.Models;

namespace QASystem.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminController : Controller
    {
        private readonly QasystemContext _context;

        public AdminController(QasystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ManageReports()
        {
            var reports = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableContent(int? questionId, int? answerId)
        {
            if (!questionId.HasValue && !answerId.HasValue)
            {
                TempData["Error"] = "Must provide either questionId or answerId.";
                return RedirectToAction("ManageReports");
            }

            if (questionId.HasValue)
            {
                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                {
                    TempData["Error"] = "Question not found.";
                    return RedirectToAction("ManageReports");
                }
                question.IsDisabled = true;
            }
            else if (answerId.HasValue)
            {
                var answer = await _context.Answers.FindAsync(answerId);
                if (answer == null)
                {
                    TempData["Error"] = "Answer not found.";
                    return RedirectToAction("ManageReports");
                }
                answer.IsDisable = true;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Content has been disabled.";
            return RedirectToAction("ManageReports");
        }
    }
}