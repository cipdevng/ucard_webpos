using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.File {
    public interface IFileConverter {
        Task<IFormFile> md2docx(IFormFile md, string fileName);
        Task<FileStream> md2docxStream(IFormFile md, string fileName);
        Task<string> html2docx(IFormFile md, string fileName);
    }
}
