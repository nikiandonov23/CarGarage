using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.ViewModels.Forum;
using CarGarage.Web.Controllers;

namespace CarGarage.Controllers
{
    public class ForumController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ForumController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Forum
        public async Task<IActionResult> Index(string? searchString)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Fetch posts
            var postsQuery = _context.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => 
                    p.Title.Contains(searchString) || 
                    p.Content.Contains(searchString));
            }

            var posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Fetch garages once to optimize user displayName & garageName resolution
            var allGarages = await _context.Garages.ToListAsync();

            var viewModels = posts.Select(p =>
            {
                var garage = allGarages.FirstOrDefault(g => g.OwnerId == p.UserId);
                var userDisplayName = garage?.OwnerName ?? p.User?.Email ?? "Анонимен потребител";
                var garageName = garage?.Name;

                return new ForumPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    UserId = p.UserId,
                    UserDisplayName = userDisplayName,
                    GarageName = garageName,
                    CreatedAt = p.CreatedAt,
                    LikesCount = p.Likes.Count,
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId),
                    CommentsCount = p.Comments.Count
                };
            }).ToList();

            ViewBag.SearchString = searchString;
            return View(viewModels);
        }

        // GET: Forum/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var allGarages = await _context.Garages.ToListAsync();
            var postGarage = allGarages.FirstOrDefault(g => g.OwnerId == post.UserId);
            var postUserDisplayName = postGarage?.OwnerName ?? post.User?.Email ?? "Анонимен";
            var postGarageName = postGarage?.Name;

            var viewModel = new ForumPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                UserId = post.UserId,
                UserDisplayName = postUserDisplayName,
                GarageName = postGarageName,
                CreatedAt = post.CreatedAt,
                LikesCount = post.Likes.Count,
                IsLikedByCurrentUser = post.Likes.Any(l => l.UserId == userId),
                CommentsCount = post.Comments.Count,
                Comments = post.Comments.Select(c =>
                {
                    var commentGarage = allGarages.FirstOrDefault(g => g.OwnerId == c.UserId);
                    return new ForumCommentViewModel
                    {
                        Id = c.Id,
                        PostId = c.PostId,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserDisplayName = commentGarage?.OwnerName ?? c.User?.Email ?? "Анонимен",
                        GarageName = commentGarage?.Name,
                        CreatedAt = c.CreatedAt
                    };
                }).OrderBy(c => c.CreatedAt).ToList()
            };

            return View(viewModel);
        }

        // GET: Forum/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ForumPostFormModel());
        }

        // POST: Forum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPostFormModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            var post = new ForumPost
            {
                Title = model.Title,
                Content = model.Content,
                ImageUrl = model.ImageUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Forum/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            var model = new ForumPostFormModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl
            };

            return View(model);
        }

        // POST: Forum/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ForumPostFormModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            post.Title = model.Title;
            post.Content = model.Content;
            post.ImageUrl = model.ImageUrl;

            _context.ForumPosts.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = post.Id });
        }

        // POST: Forum/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Forum/Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(ForumCommentFormModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }

            var postExists = await _context.ForumPosts.AnyAsync(p => p.Id == model.PostId);
            if (!postExists)
                return NotFound();

            var comment = new ForumComment
            {
                PostId = model.PostId,
                Content = model.Content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        // POST: Forum/DeleteComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int postId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var comment = await _context.ForumComments.FindAsync(id);
            if (comment == null)
                return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            _context.ForumComments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // POST: Forum/Like/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            var existingLike = await _context.ForumPostLikes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (existingLike != null)
            {
                _context.ForumPostLikes.Remove(existingLike);
            }
            else
            {
                var like = new ForumPostLike
                {
                    PostId = id,
                    UserId = userId
                };
                _context.ForumPostLikes.Add(like);
            }

            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action(nameof(Index))!);
        }
    }
}
