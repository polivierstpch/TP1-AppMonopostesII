using Blog.ApplicationCore.Entites;
using Blog.ApplicationCore.Interfaces;
using Blog.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IAsyncImagesService _imagesService;
        private readonly IAsyncBlogService _blogService;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration config,
            IAsyncBlogService blogService,
            IAsyncImagesService imagesService
        )
        {
            _logger = logger;
            _config = config;
            _blogService = blogService;
            _imagesService = imagesService;
        }

        public async Task<IActionResult> Index(string categorie)
        {
            var resultatPosts = await _blogService.ObtenirPublicationsAsync();

            var posts = resultatPosts
                .OrderByDescending(b => b.DatePublication)
                .ToList();

            if (!string.IsNullOrEmpty(categorie))
            {
                posts.RemoveAll(x => !x.Categorie.Libelle.Equals(categorie));
            }

            return View(posts);
        }

        public async Task<IActionResult> Post(int? id, string? urlslug)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _blogService.ObtenirPublicationParIdAsync(id.Value);

            if (post == null)
            {
                return NotFound();
            }

            //si urlslug absent des paramètres
            if (urlslug == null)
            {
                return RedirectToAction(nameof(Post), new { id = post.Id, urlslug = post.UrlSlug });
            }

            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Nouveau()
        {
            await GenererSelectList();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Nouveau(Publication publication, IFormFile Image)
        {
            await GenererSelectList();

            //Validation date
            if (publication.DatePublication < DateTime.Today)
            {
                ModelState.AddModelError("DatePublication", "La date de publication ne peut pas être dans le passé.");
                return View();
            }

            var auteur = await _blogService.ObtenirAuteurParIdAsync(publication.AuteurId);
            //Validation que l'auteur peut publier un billet
            if (!auteur.Actif)
            {
                ModelState.AddModelError("AuteurId", "L'auteur n'a pas le droit de publier");
                return View();
            }

            //On sauvegarde l'image dans le dossier wwwroot/images
            publication.NomImage = await _imagesService.SauvegarderImageAsync(Image);

            //Ajout de la publication
            await _blogService.AjouterPublicationAsync(publication);

            //Redirection vers la page d'affichage
            return RedirectToAction(nameof(Post), new { id = publication.Id, urlslug = publication.UrlSlug });
        }


        [HttpPost]
        public async Task<ActionResult> Supprimer(int id)
        {
            var post = await _blogService.ObtenirPublicationParIdAsync(id);

            if (post.DatePublication > DateTime.Today)
            {
                await _blogService.SupprimerPublicationAsync(post);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Post), new { id = post.Id, urlslug = post.UrlSlug });
        }

        private async Task GenererSelectList()
        {
            //Obtention des éléments dans le fichier de configuraion et ajout au ViewBag
            var auteurs = await _blogService.ObtenirAuteursAsync();

            ViewBag.Auteurs = auteurs.Select(a => new SelectListItem
            {
                Text = a.Nom,
                Value = a.Id.ToString()
            });

            var categories = await _blogService.ObtenirCategoriesAsync();

            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Text = c.Libelle,
                Value = c.Id.ToString()
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}