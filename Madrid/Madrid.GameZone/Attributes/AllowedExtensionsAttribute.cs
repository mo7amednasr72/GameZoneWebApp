﻿using System.ComponentModel.DataAnnotations;

namespace Madrid.GameZone.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
         private readonly string _allowedExtensions;
        public AllowedExtensionsAttribute(string allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid
            (object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                var isAllowed = _allowedExtensions.Split(",").Contains(extension, StringComparer.OrdinalIgnoreCase); // to ignore letter case
                if(!isAllowed)
                {
                    return new ValidationResult($"Only {_allowedExtensions} are allowed!");
                }
            }
            return ValidationResult.Success;
        }
    }
}
