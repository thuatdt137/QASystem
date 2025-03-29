using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QASystem.Models;

namespace QASystem.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly QasystemContext _context;
        private readonly UserManager<User> _userManager;
        private const int PageSize = 5;

        public QuestionsController(QasystemContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var question = await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Tags)
                .Include(q => q.Votes)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .Include(q => q.Answers).ThenInclude(a => a.Votes)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null || ((question.IsDisabled) && !(User.IsInRole("Admin") || User.IsInRole("Moderator"))))
            {
                return NotFound();
            }

            ViewBag.QuestionVoteCount = question.Votes.Sum(v => v.VoteType);
            ViewBag.AnswerVoteCounts = question.Answers.ToDictionary(a => a.AnswerId, a => a.Votes.Sum(v => v.VoteType));
            ViewBag.TotalAnswers = question.Answers.Count;

            var answers = question.Answers
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            question.Answers = answers;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)question.Answers.Count / PageSize);

            return View(question);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer(int questionId, string content, IFormFile image)
        {
            if (string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Content is required.";
                return RedirectToAction("Details", new { id = questionId });
            }

            var user = await _userManager.GetUserAsync(User);
            var answer = new Answer
            {
                QuestionId = questionId,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.Now
            };

            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                answer.ImageUrl = "/images/" + fileName;
            }

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = questionId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int? questionId, int? answerId, int voteType)
        {
            if (!questionId.HasValue && !answerId.HasValue)
            {
                TempData["Error"] = "Must provide either questionId or answerId.";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0 });
            }

            if (voteType != 1 && voteType != -1)
            {
                TempData["Error"] = "Invalid vote type.";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0 });
            }

            var user = await _userManager.GetUserAsync(User);
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == user.Id &&
                                         (questionId.HasValue ? v.QuestionId == questionId : v.AnswerId == answerId));

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    _context.Votes.Remove(existingVote);
                }
                else
                {
                    existingVote.VoteType = voteType;
                }
            }
            else
            {
                var vote = new Vote
                {
                    UserId = user.Id,
                    VoteType = voteType,
                    QuestionId = questionId,
                    AnswerId = answerId
                };
                _context.Votes.Add(vote);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving vote: {ex.Message}";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0 });
            }

            var redirectId = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId;
            if (redirectId == null)
            {
                TempData["Error"] = "Could not determine redirect ID.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Details", new { id = redirectId });
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int? questionId, int? answerId, string reason)
        {
            if (!questionId.HasValue && !answerId.HasValue)
            {
                TempData["Error"] = "Must provide either questionId or answerId.";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId))?.QuestionId ?? 0 });
            }

            if (string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "Reason for reporting is required.";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId)).QuestionId });
            }

            var user = await _userManager.GetUserAsync(User);
            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.UserId == user.Id &&
                                         (questionId.HasValue ? r.QuestionId == questionId : r.AnswerId == answerId));

            if (existingReport != null)
            {
                TempData["Error"] = "You have already reported this content.";
                return RedirectToAction("Details", new { id = questionId ?? (await _context.Answers.FindAsync(answerId)).QuestionId });
            }

            var report = new Report
            {
                UserId = user.Id,
                QuestionId = questionId,
                AnswerId = answerId,
                Reason = reason,
                ReportedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your report has been submitted.";
            var redirectId = questionId ?? (await _context.Answers.FindAsync(answerId)).QuestionId;
            return RedirectToAction("Details", new { id = redirectId });
        }
    }
}