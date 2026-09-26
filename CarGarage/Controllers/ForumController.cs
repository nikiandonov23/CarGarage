using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.ViewModels.Forum;
using CarGarage.Web.Controllers;
using CarGarage.Services.Core.Contracts;

namespace CarGarage.Controllers
{
    public class ForumController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICloudflareR2Service _r2Service;

        public ForumController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ICloudflareR2Service r2Service)
        {
            _context = context;
            _userManager = userManager;
            _r2Service = r2Service;
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
                .Include(p => p.ForumPostImages)
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
                    ImageUrl = p.ForumPostImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    PostImages = p.ForumPostImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => new ForumPostImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        SortOrder = img.SortOrder
                    }).ToList(),
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
                .Include(p => p.ForumPostImages)
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
                ImageUrl = post.ForumPostImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault() ?? post.ImageUrl,
                PostImages = post.ForumPostImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => new ForumPostImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    SortOrder = img.SortOrder
                }).ToList(),
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

            // Handle file uploads on create
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var filesToUpload = model.ImageFiles.Take(5);
                int currentOrder = 0;
                foreach (var file in filesToUpload)
                {
                    if (file == null || file.Length == 0) continue;
                    try
                    {
                        var (uploadedUrl, key) = await _r2Service.UploadImageAsync(file, "forum-images");
                        post.ForumPostImages.Add(new ForumPostImage
                        {
                            ImageUrl = uploadedUrl,
                            StorageKey = key,
                            SortOrder = currentOrder++
                        });
                    }
                    catch (NotSupportedException ex)
                    {
                        ModelState.AddModelError("ImageFiles", ex.Message);
                        return View(model);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, $"Грешка при качване на снимка: {ex.Message}");
                        return View(model);
                    }
                }
            }

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

            var post = await _context.ForumPosts
                .Include(p => p.ForumPostImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            var model = new ForumPostFormModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                ExistingImages = post.ForumPostImages
                    .OrderBy(img => img.SortOrder)
                    .ThenBy(img => img.Id)
                    .Select(img => new ForumPostImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        SortOrder = img.SortOrder
                    }).ToList()
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
            {
                var originalPost = await _context.ForumPosts.Include(p => p.ForumPostImages).FirstOrDefaultAsync(p => p.Id == id);
                if (originalPost != null)
                {
                    model.ExistingImages = originalPost.ForumPostImages
                        .OrderBy(img => img.SortOrder)
                        .ThenBy(img => img.Id)
                        .Select(img => new ForumPostImageViewModel { Id = img.Id, ImageUrl = img.ImageUrl, SortOrder = img.SortOrder })
                        .ToList();
                }
                return View(model);
            }

            var post = await _context.ForumPosts
                .Include(p => p.ForumPostImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            post.Title = model.Title;
            post.Content = model.Content;
            post.ImageUrl = model.ImageUrl;

            // Handle new image uploads on Edit
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var currentCount = post.ForumPostImages.Count;
                var maxOrder = post.ForumPostImages.Any() ? post.ForumPostImages.Max(img => img.SortOrder) : -1;
                var allowedSlots = 5 - currentCount;
                if (allowedSlots > 0)
                {
                    var filesToUpload = model.ImageFiles.Take(allowedSlots);
                    int currentOrder = maxOrder + 1;
                    foreach (var file in filesToUpload)
                    {
                        if (file == null || file.Length == 0) continue;
                        try
                        {
                            var (uploadedUrl, key) = await _r2Service.UploadImageAsync(file, "forum-images");
                            var newImg = new ForumPostImage
                            {
                                ForumPostId = post.Id,
                                ImageUrl = uploadedUrl,
                                StorageKey = key,
                                SortOrder = currentOrder++
                            };
                            post.ForumPostImages.Add(newImg);
                        }
                        catch (NotSupportedException ex)
                        {
                            ModelState.AddModelError("ImageFiles", ex.Message);
                            model.ExistingImages = post.ForumPostImages
                                .OrderBy(img => img.SortOrder)
                                .ThenBy(img => img.Id)
                                .Select(img => new ForumPostImageViewModel { Id = img.Id, ImageUrl = img.ImageUrl, SortOrder = img.SortOrder })
                                .ToList();
                            return View(model);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, $"Грешка при качване на снимка: {ex.Message}");
                            model.ExistingImages = post.ForumPostImages
                                .OrderBy(img => img.SortOrder)
                                .ThenBy(img => img.Id)
                                .Select(img => new ForumPostImageViewModel { Id = img.Id, ImageUrl = img.ImageUrl, SortOrder = img.SortOrder })
                                .ToList();
                            return View(model);
                        }
                    }
                }
            }

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

            // Clean up files in S3
            var images = await _context.ForumPostImages.Where(img => img.ForumPostId == id).ToListAsync();
            foreach (var img in images)
            {
                await _r2Service.DeleteImageAsync(img.StorageKey, "forum-images");
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int postId, int imageId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var postExists = await _context.ForumPosts.AnyAsync(p => p.Id == postId && p.UserId == userId);
            if (!postExists) return Json(new { success = false, message = "Нямате достъп до тази публикация." });

            var image = await _context.ForumPostImages.FirstOrDefaultAsync(img => img.Id == imageId && img.ForumPostId == postId);
            if (image == null) return Json(new { success = false, message = "Снимката не е намерена." });

            await _r2Service.DeleteImageAsync(image.StorageKey, "forum-images");
            _context.ForumPostImages.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet("/Forum/Image/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(string key)
        {
            if (string.IsNullOrEmpty(key)) return NotFound();

            var stream = await _r2Service.GetImageStreamAsync(key, "forum-images");
            if (stream == null) return NotFound();

            return File(stream, "image/jpeg");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImagePositions([FromBody] ImagePositionUpdateModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (model == null || model.ImageIds == null) return BadRequest();

            var post = await _context.ForumPosts.FirstOrDefaultAsync(p => p.Id == model.ItemId && p.UserId == userId);
            if (post == null) return Forbid();

            for (int i = 0; i < model.ImageIds.Count; i++)
            {
                var imgId = model.ImageIds[i];
                var img = await _context.ForumPostImages.FirstOrDefaultAsync(fpi => fpi.Id == imgId && fpi.ForumPostId == model.ItemId);
                if (img != null)
                {
                    img.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class ImagePositionUpdateModel
        {
            public int ItemId { get; set; }
            public List<int> ImageIds { get; set; } = new();
        }
    }
}
