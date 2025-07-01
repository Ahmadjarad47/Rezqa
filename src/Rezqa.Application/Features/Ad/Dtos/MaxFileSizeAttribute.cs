using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.Ad.Dtos;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSizeInBytes;

    public MaxFileSizeAttribute(int maxFileSizeInBytes)
    {
        _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFileCollection files)
        {
            foreach (var file in files)
            {
                if (file.Length > _maxFileSizeInBytes)
                {
                    return new ValidationResult(GetErrorMessage(file.FileName));
                }
            }
        }
        else if (value is IFormFile singleFile)
        {
            if (singleFile.Length > _maxFileSizeInBytes)
            {
                return new ValidationResult(GetErrorMessage(singleFile.FileName));
            }
        }

        return ValidationResult.Success;
    }

    private string GetErrorMessage(string fileName)
    {
        return $"The file '{fileName}' exceeds the maximum allowed size of {_maxFileSizeInBytes / (1024 * 1024)}MB.";
    }
}
