namespace SAN_API.Helper
{
    public class GenerateFileType
    {
        public string SaveFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // Créer le dossier s'il n'existe pas
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media", folder);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Générer un nom unique
            var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(directory, fileName);

            // Sauvegarder le fichier
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Retourner le chemin accessible (URL)
            return $"/Media/{folder}/{fileName}";
        }

        //Conversion d'image en byte[]
        public byte[] OutputFile(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileName);
            // Lire le fichier sous forme de tableau d'octets
            return System.IO.File.Exists(filePath) ? System.IO.File.ReadAllBytes(filePath) : null;
        }

        //Conversion en base64
        public string GetImageAsBase64(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileName);

            if (!System.IO.File.Exists(filePath))
                return null; // Si le fichier n'existe pas, retourne null

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath); // Lire l'image en bytes
            return Convert.ToBase64String(fileBytes); // Convertir en Base64
        }
        public bool IsDelete(string fileName, IFormFile file)
        {
            if (file is not null || file.Length != 0)
            {
                string fileBd = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileName);
                string newFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Media/Images" + file.FileName);
                if (!System.IO.File.Exists(newFile))
                {
                    if (System.IO.File.Exists(fileBd))
                    {
                        System.IO.File.Delete(fileBd);
                        return true;
                    }
                }

            }
            return false;

        }
        public bool DeleteFile(string fileName)
        {
            string file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileName);
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                return true;
            }
            return false;
        }
    }
}
