﻿

namespace Madrid.GameZone.Services
{
    public class GamesService: IGamesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imagesPath;
        public GamesService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _imagesPath = $"{_webHostEnvironment.WebRootPath}{FileSettings.ImagesPath}"; // WebRootPath means go the the wwwroot folder
        }
        public IEnumerable<Game> GetAll()
        {
            var games = _context.Games
                .Include(g => g.Category)
                .Include(g => g.Devices)
                .ThenInclude(d => d.Device)
                .AsNoTracking()
                .OrderBy(g => g.Name)// episode 27
                .ToList();
            return games;
        }
        public Game? GetById(int id)
        {
            var game = _context.Games
                .Include(g => g.Category) // eager loading
                .Include(g => g.Devices)
                .ThenInclude(d => d.Device)
                .AsNoTracking()
                .SingleOrDefault(g => g.Id == id);// episode 27
                
            return game;
        }
        public async Task Create(CreateGameFormViewModel model)
        {
            var coverName = await SaveCover(model.Cover);
          
            Game game = new Game
            {
                Name = model.Name,
                Description = model.Description,
                CategoryId = model.CategoryId,
                Cover = coverName,
                Devices = model.SelectedDevices.Select(d => new GameDevice { DeviceId = d }).ToList() // note GameId will be assigned automatically // this called projection to another type
            };
            _context.Add(game);
            _context.SaveChanges();
        }

        public async Task<Game?> Update(EditGameFormViewModel model)
        {
            var game = _context.Games
                .Include(g => g.Devices)
                .SingleOrDefault(g => g.Id == model.Id);

            if(game is null)
                return null;

            var hasNewCover = model.Cover is not null;
            var oldCover = game.Cover;

            game.Name = model.Name;
            game.Description = model.Description;
            game.CategoryId = model.CategoryId;
            game.Devices = model.SelectedDevices.Select(d => new GameDevice { DeviceId = d}).ToList();

            if (hasNewCover)
            {
                game.Cover = await SaveCover(model.Cover!);
            }

            var effectedRows = _context.SaveChanges();
            if(effectedRows > 0)
            {
                if(hasNewCover)
                {
                    var cover = Path.Combine(_imagesPath, oldCover);
                    File.Delete(cover);
                }
                return game;
            }
            else
            {
                var cover = Path.Combine(_imagesPath, game.Cover);
                File.Delete(cover);
                return null;
            }
        }
        public bool Delete(int id)
        {
            var isDeleted = false;

            var game = _context.Games.Find(id);
            if (game is null)
                return isDeleted;

            _context.Games.Remove(game);

            var effectedRows = _context.SaveChanges();
            if (effectedRows > 0)
            {
                isDeleted = true;
                var cover = Path.Combine(_imagesPath, game.Cover);
                File.Delete(cover);
            }

            return isDeleted;
        }

        private async Task<string> SaveCover(IFormFile cover)
        {
            var coverName = $"{Guid.NewGuid()}{Path.GetExtension(cover.FileName)}";
            var path = Path.Combine(_imagesPath, coverName);
            using var stream = File.Create(path);
            await cover.CopyToAsync(stream); // coping the file into the server

            return coverName;
        }

    }
}