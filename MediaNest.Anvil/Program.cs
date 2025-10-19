using MediaNest.Shared;
using MediaNest.Shared.Entities;
using MediaNest.Shared.Models;
using MediaNest.Shared.Services;
using MongoDB.Driver.GridFS;
using System.Dynamic;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;



Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Directory.CreateDirectory("C:\\Task");

string targetPath = Console.ReadLine();
ComicZipImporter importer = new("C:\\Task");
var files = importer.GetAllZipFiles(targetPath);
foreach(var file in files) {
    importer.ImportComic(file);
}
foreach (var file in files) {
    File.Delete(file);
}

