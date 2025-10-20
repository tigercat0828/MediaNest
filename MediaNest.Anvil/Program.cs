using MediaNest.Shared.Models;
using System.Text;



Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Directory.CreateDirectory("C:\\Task");

string targetPath = Console.ReadLine();
ComicZipImporter importer = new("C:\\Task");
var files = importer.GetAllZipFiles(targetPath);
foreach (var file in files) {
    importer.ImportComic(file);
}
foreach (var file in files) {
    File.Delete(file);
}

