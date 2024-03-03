using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.File {
    public interface IFileStorage {
        Task<string> uploadFileAsync(IFormFile file, string fileName);
        Task<bool> deleteFile(string fileName);
        Task<bool> getFileInto(string fileName, string nFile);
        string getSASToken(string filename, string fullFile, string contentType, long SASExpiryMins = -1);
    }
}
