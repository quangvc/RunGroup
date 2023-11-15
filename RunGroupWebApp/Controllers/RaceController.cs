using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroupWebApp.Data;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using RunGroupWebApp.Repository;
using RunGroupWebApp.Services;
using RunGroupWebApp.ViewModels;

namespace RunGroupWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var races = await _raceRepository.GetAll();
            return View(races);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            return View(race);
        }
        public IActionResult Create()
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var createRaceViewModel = new CreateRaceViewModel { AppUserId = curUserId };
            return View(createRaceViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Photo upload failed");
                return View(raceVM);
            }
            var result = await _photoService.AddPhotoAsync(raceVM.Image);
            var race = new Race
            {
                Title = raceVM.Title,
                Description = raceVM.Description,
                Image = result.Url.ToString(),
                AppUserId = raceVM.AppUserId,
                Address = new Address
                {
                    Street = raceVM.Address.Street,
                    City = raceVM.Address.City,
                    State = raceVM.Address.State,
                }
            };

            _raceRepository.Add(race);
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null)
            {
                return View("Error");
            }
            var raceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                URL = race.Image,
                AddressId = race.AddressId,
                Address = race.Address,
                RaceCategory = race.RaceCategory,
            };
            return View(raceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel RaceVM)
        {
            if (!ModelState.IsValid) return View("Edit", RaceVM);

            // lưu ý dùng asnotracking đoạn này
            var userRace = await _raceRepository.GetByIdAsyncNoTracking(id);
            if (userRace == null)
            {
                return View(RaceVM);
            }
            try
            {
                await _photoService.DeletePhotoAsync(userRace.Image);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Could not delete photo");
                return View(RaceVM);
            }

            var photoResult = await _photoService.AddPhotoAsync(RaceVM.Image);
            var race = new Race
            {
                Id = id,
                Title = RaceVM.Title,
                Description = RaceVM.Description,
                Image = photoResult.Url.ToString(),
                AddressId = RaceVM.AddressId,
                Address = RaceVM.Address,
            };
            _raceRepository.Update(race);

            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Delete(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null)
            {
                return View("Error");
            }

            return View(raceDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null)
            {
                return View("Error");
            }
            _raceRepository.Delete(raceDetails);
            return RedirectToAction("Index");
        }



    }
}
