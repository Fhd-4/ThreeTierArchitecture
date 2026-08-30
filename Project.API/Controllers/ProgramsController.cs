using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.DTOs;
using Project.BLL.Services;
using Project.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService)
    {
        _programService = programService;
    }

    // 1. جلب كل البرامج مع ميزة الفلترة
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProgramDetailsDto>>> GetPrograms(
        [FromQuery] int? portfolioId,
        [FromQuery] string? keyword,
        [FromQuery] int? status)
    {
        var programs = await _programService.GetProgramsAsync(portfolioId, keyword, status);
        return Ok(programs);
    }

    // 2. جلب تفاصيل برنامج معين لمعاينته
    [HttpGet("{id}")]
    public async Task<ActionResult<ProgramDetailsDto>> GetProgram(int id)
    {
        var program = await _programService.GetProgramByIdAsync(id);
        if (program == null)
            return NotFound();

        return Ok(program);
    }

    // 3. إنشاء برنامج جديد وربطه بالمحفظة الأب
    [HttpPost]
    public async Task<ActionResult<ProjectProgram>> CreateProgram([FromBody] CreateProgramDto dto)
    {
        var program = await _programService.CreateProgramAsync(dto);
        return CreatedAtAction(nameof(GetProgram), new { id = program.Id }, program);
    }

    // 4. تعديل بيانات البرنامج
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProgram(int id, [FromBody] UpdateProgramDto dto)
    {
        var program = await _programService.UpdateProgramAsync(id, dto);
        if (program == null)
            return NotFound();

        return Ok(program);
    }

    // 5. حذف البرنامج مع التحقق الأمني لحماية البيانات التابعة له
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProgram(int id)
    {
        var (success, errorMessage) = await _programService.DeleteProgramAsync(id);

        if (!success)
        {
            if (errorMessage == "Not Found")
                return NotFound();

            return BadRequest(errorMessage);
        }

        return Ok(new { Message = "Program deleted successfully." });
    }

    // 6. رفع الملفات للبرنامج
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var uploadedFilesList = new List<object>();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var originalName = file.FileName;
                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(originalName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var sizeInMb = (file.Length / (1024.0 * 1024.0)).ToString("F1") + " MB";
                var ext = Path.GetExtension(originalName).TrimStart('.').ToLower();

                uploadedFilesList.Add(new
                {
                    name = originalName,
                    path = "/uploads/" + uniqueName,
                    size = sizeInMb,
                    type = ext
                });
            }
        }

        return Ok(uploadedFilesList);
    }
}