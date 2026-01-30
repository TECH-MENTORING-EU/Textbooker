using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Admin.Pages
{
    public class SchoolsModel : PageModel
    {
        private readonly SchoolService _schoolService;
        private readonly ILogger<SchoolsModel> _logger;

        public SchoolsModel(SchoolService schoolService, ILogger<SchoolsModel> logger)
        {
            _schoolService = schoolService;
            _logger = logger;
        }

        public List<SchoolWithUserCount> Schools { get; set; } = [];

        [FromQuery]
        public bool ShowInactive { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            Schools = await _schoolService.GetSchoolsWithUserCountAsync(ShowInactive);
            return Page();
        }

        public async Task<IActionResult> OnGetFilterAsync(bool showInactive = false)
        {
            ShowInactive = showInactive;
            Schools = await _schoolService.GetSchoolsWithUserCountAsync(showInactive);
            return Partial("_SchoolRows", Schools);
        }

        public async Task<IActionResult> OnPostAddAsync(string name, string? emailDomain)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(string.Empty, "Nazwa szkoły jest wymagana.");
                Schools = await _schoolService.GetSchoolsWithUserCountAsync(ShowInactive);
                return new BadRequestResult();
            }

            try
            {
                var school = await _schoolService.CreateSchoolAsync(name.Trim(), emailDomain?.Trim());
                _logger.LogInformation("Admin created new school: {SchoolName} (ID: {SchoolId})", school.Name, school.Id);
                
                // Return single row for the new school
                var schoolWithCount = new SchoolWithUserCount
                {
                    School = school,
                    UserCount = 0
                };
                return Partial("_SchoolRows", new List<SchoolWithUserCount> { schoolWithCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating school: {SchoolName}", name);
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas tworzenia szkoły.");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int id)
        {
            var school = await _schoolService.GetSchoolByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            var result = await _schoolService.SoftDeleteSchoolAsync(id);
            if (!result)
            {
                return new StatusCodeResult(500);
            }

            _logger.LogInformation("Admin soft-deleted school: {SchoolName} (ID: {SchoolId})", school.Name, school.Id);
            
            var userCount = await _schoolService.GetUserCountAsync(id);
            var schoolWithCount = new SchoolWithUserCount
            {
                School = school,
                UserCount = userCount
            };
            return Partial("_SchoolRows", new List<SchoolWithUserCount> { schoolWithCount });
        }

        public async Task<IActionResult> OnPostReactivateAsync(int id)
        {
            var school = await _schoolService.GetSchoolByIdAsync(id);
            if (school == null)
            {
                return NotFound();
            }

            var result = await _schoolService.ReactivateSchoolAsync(id);
            if (!result)
            {
                return new StatusCodeResult(500);
            }

            _logger.LogInformation("Admin reactivated school: {SchoolName} (ID: {SchoolId})", school.Name, school.Id);
            
            var userCount = await _schoolService.GetUserCountAsync(id);
            var schoolWithCount = new SchoolWithUserCount
            {
                School = school,
                UserCount = userCount
            };
            return Partial("_SchoolRows", new List<SchoolWithUserCount> { schoolWithCount });
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string? emailDomain)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new BadRequestResult();
            }

            var school = await _schoolService.UpdateSchoolAsync(id, name.Trim(), emailDomain?.Trim());
            if (school == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Admin updated school: {SchoolName} (ID: {SchoolId})", school.Name, school.Id);
            
            var userCount = await _schoolService.GetUserCountAsync(id);
            var schoolWithCount = new SchoolWithUserCount
            {
                School = school,
                UserCount = userCount
            };
            return Partial("_SchoolRows", new List<SchoolWithUserCount> { schoolWithCount });
        }
    }
}
